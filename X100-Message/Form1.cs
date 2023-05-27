namespace X100_Message
{
    public partial class Form1 : Form
    {
        private string version = "2.0.0";
        Uart uart = new();
        Extend extend = new();

        private bool isWaitMessage = false;
        private bool isRestart = false;

        public Form1()
        {
            InitializeComponent();
            uart.DataReceived += DataReceived;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitComPort();
            InitFont();
            this.Text = "DJ-X100 ���b�Z�[�W���K�[ Ver" + version;


        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            uart.Close();
            Application.Exit();
        }

        private String SendCmd(string cmd)
        {
            String response = uart.SendCmd(cmd).Replace("\r\n", "");

            if (response.Equals(Command.NG))
            {
                MessageBox.Show("�����ُ�", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "���X�|���X�ُ�";
            }

            return response;
        }

        private bool SendCmd(string cmd, string expectResponse)
        {
            String response = uart.SendCmd(cmd).Replace("\r\n", "");

            if (response.Equals(Command.NG))
            {
                MessageBox.Show("�����ُ�", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            if (response.Equals(expectResponse)) return true;
            return false;
        }

        private String SendRawdCmd(String cmd)
        {
            String response = uart.SendRawCmd(cmd).Replace("\r\n", "");

            if (response.Equals(Command.NG))
            {
                MessageBox.Show("�����ُ�", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "���X�|���X�ُ�";
            }

            return response;
        }

        private void ConnectX100()
        {
            if (uart.InitSerialPort(comComboBox.Text))
            {
                if (SendCmd(Command.WHO, "DJ-X100"))
                {

                    //if (!isRestart && SendCmd(Command.DSPTHRU, "  SLEEP"))
                    //{
                    //    MessageBox.Show("�d���������Ă��܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //     CloseConnection();
                    //     return;
                    // }

                    connectBtn.Text = "�ؒf";
                    msgOutputBtn.Enabled = true;
                    extMenuItem.Enabled = true;
                    restartBtn.Enabled = true;

                    warnLabel.Text = "DJ-X100�ڑ��ς�";

                    GetX100Info();
                }
                else
                {
                    warnLabel.Text = "�ڑ����s";
                    MessageBox.Show("�ڑ��G���[���������܂���: ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CloseConnection();
                }

            }
        }

        // DJ-X100�ɐڑ�
        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            if (connectBtn.Text.Equals("�ؒf"))
            {
                CloseConnection();
                return;
            }

            ConnectX100();

        }

        private void GetX100Info()
        {
            mcuLabel.Text = SendCmd(Command.VER);
            ext1Label.Text = SendCmd(Command.EXT1_IS_VAILD, Command.ENABLE) ? "�g���@�\1:�L��" : "�g���@�\1:����";
            ext2Label.Text = SendCmd(Command.EXT2_IS_VAILD, Command.ENABLE) ? "�g���@�\2:�L��" : "�g���@�\2:����";
        }




        //�񓯊��ŃR�}���h��M�ҋ@���Ă���i�C�x���g�n���h���j
        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!isWaitMessage) return;

            String response = e.Data;
            if (response.Equals("\r\nOK\r\n")) return;

            // ��M�f�[�^���e�L�X�g�{�b�N�X�ɕ\���iUI�X���b�h�Ŏ��s�j
            this.Invoke(new Action(() =>
            {
                DateTime now = DateTime.Now;
                logTextBox.AppendText($"{now} >> {response}\r\n");

                if (logFileOutFlg.Checked)
                {
                    try
                    {
                        File.AppendAllText($"received_message_{now.ToString("yyyyMMdd")}.txt", $"{now} >> {response}" + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("���b�Z�[�W���O�o�̓G���[ " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }));
        }

        private void CloseConnection()
        {
            uart.Close();
            warnLabel.Text = "�ڑ����Ă��܂���";
            connectBtn.Text = "�ڑ�";

            msgOutputBtn.Enabled = false;
            extMenuItem.Enabled = false;
            restartBtn.Enabled = false;

            mcuLabel.Text = "ver";
            ext1Label.Text = "�g���@�\1:";
            ext2Label.Text = "�g���@�\2:";
        }


        private void MsgOutputBtn_Click(object sender, EventArgs e)
        {
            if (isWaitMessage)
            {
                if (SendRawdCmd("QUIT\r\n").Equals(Command.OK))
                {
                    msgOutputBtn.Text = "���b�Z�[�W�o�͊J�n";
                    warnLabel.Text = "DJ-X100�ڑ��ς�";
                    extMenuItem.Enabled = true;
                    restartBtn.Enabled = true;
                    isWaitMessage = false;
                }
                return;
            }

            if (SendCmd(Command.OUTLINE, Command.OK))
            {
                msgOutputBtn.Text = "���b�Z�[�W�o�͏I��";
                warnLabel.Text = "���b�Z�[�W�ҋ@���c(���g�����̕ύX����)";
                extMenuItem.Enabled = false;
                restartBtn.Enabled= false;
                isWaitMessage = true;
            }
        }

        // COM�|�[�g�ꗗ�̏���������
        private void InitComPort()
        {
            foreach (String portName in Uart.GetPortLists())
            {
                comComboBox.Items.Add(portName);
            }
            if (comComboBox.Items.Count > 0)
            {
                comComboBox.SelectedIndex = 0;
            }
        }

        private void InitFont()
        {
            fontSizeComboBox.Items.Add("7");
            fontSizeComboBox.Items.Add("8");
            fontSizeComboBox.Items.Add("10");
            fontSizeComboBox.Items.Add("12");
            fontSizeComboBox.Items.Add("14");
            fontSizeComboBox.Items.Add("16");
            fontSizeComboBox.Items.Add("18");
            fontSizeComboBox.Items.Add("20");
            fontSizeComboBox.Items.Add("24");
            // �f�t�H���g�̃t�H���g�T�C�Y��ݒ�
            float defaultFontSize = 8.0f;
            logTextBox.Font = new Font(logTextBox.Font.FontFamily, defaultFontSize);
            fontSizeComboBox.SelectedItem = defaultFontSize.ToString();
            foreach (FontFamily font in FontFamily.Families)
            {
                fontComboBox.Items.Add(font.Name);
            }
            // �f�t�H���g�̃t�H���g��ݒ�
            fontComboBox.SelectedItem = logTextBox.Font.FontFamily.Name;

        }

        private void FontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFontSize = (string)fontSizeComboBox.SelectedItem;
            float fontSize = 8.0f;
            if (float.TryParse(selectedFontSize, out fontSize))
            {
                float.Parse(selectedFontSize);
            }
            logTextBox.Font = new Font(logTextBox.Font.FontFamily, fontSize);
        }

        private void FontComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFont = (string)fontComboBox.SelectedItem;
            logTextBox.Font = new Font(selectedFont, logTextBox.Font.Size);
        }

        private void �I��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uart.Close();
            Application.Exit();
        }

        private void ���O�N���ACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.ResetText();
        }

        private void �o�[�W�������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DJ-X100���b�Z�[�W���K�[\nVer" + version + "\nCopyright(C) 2023 by kaz", "�o�[�W�������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ext1EnableBtn_Click(object sender, EventArgs e)
        {
            extend.IsExtendAccept(sender);
        }

        private void ext1DisableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender))
            {

                if (SendCmd(Command.EXT1_DISABLE, Command.OK))
                {
                    MessageBox.Show("�g���@�\1�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                ext1Label.Text = SendCmd(Command.EXT1_IS_VAILD, Command.ENABLE) ? "�g���@�\1:�L��" : "�g���@�\1:����";
            }
        }

        private void ext2EnableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender))
            {
                if (SendCmd(Command.EXT2_ENABLE, Command.OK))
                {
                    MessageBox.Show("�g���@�\2��L�������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                ext2Label.Text = SendCmd(Command.EXT2_IS_VAILD, Command.ENABLE) ? "�g���@�\2:�L��" : "�g���@�\2:����";
            }
        }
        private void ext2DisableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender))
            {
                if (SendCmd(Command.EXT2_DISABLE, Command.OK))
                {
                    MessageBox.Show("�g���@�\2�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                ext2Label.Text = SendCmd(Command.EXT2_IS_VAILD, Command.ENABLE) ? "�g���@�\2:�L��" : "�g���@�\2:����";
            }
        }

        private async void restartBtn_Click(object sender, EventArgs e)
        {
            if (SendCmd(Command.RESTART, Command.OK))
            {
                isRestart = true;
                CloseConnection();
                await Task.Delay(5000);

                ConnectX100();
            }
        }
    }


}