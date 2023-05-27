namespace X100_Message
{
    public partial class Form1 : Form
    {
        private string version = "1.3.0";
        Uart uart = new();
        Extend extend = new();

        // 
        private bool isFirstConnect = true;
        private bool isSucceedConnect = false;
        private string lastSendCmd = "";

        // DJ-X100���
        private string mcuVer = "";
        private bool isVaildExt1 = false;
        private bool isVaildExt2 = false;



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

        private void SendCmd(string cmd)
        {
            lastSendCmd = cmd;
            uart.SendCmd(cmd);
        }

        private void SendRawdCmd(String cmd)
        {
            lastSendCmd = cmd;
            uart.SendRawCmd(cmd);
        }

        private bool IsResponseOk(String response)
        {
            if (response != null && response.Equals("\r\nOK\r\n"))
            {
                return true;
            }
            return false;
        }

        // DJ-X100�ɐڑ�
        private async void ConnectBtn_Click(object sender, EventArgs e)
        {

            if (connectBtn.Text.Equals("�ؒf"))
            {
                CloseConnection();
                return;
            }


            if (uart.InitSerialPort(comComboBox.Text))
            {
                await Task.Delay(500); // �񓯊��Ŏ�M���Ă��邽�ߑҋ@

                if (isSucceedConnect)
                {
                    connectBtn.Text = "�ؒf";
                    msgOutputBtn.Enabled = true;

                    ext1MenuItem.Enabled = true;
                    ext2MenuItem.Enabled = true;

                    warnLabel.Text = "DJ-X100�ڑ��ς�";

                    GetX100Info();
                    await Task.Delay(1000);

                    isFirstConnect = false;
                }
                else
                {
                    warnLabel.Text = "�ڑ����s";
                    MessageBox.Show("�ڑ��G���[���������܂���: ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CloseConnection();
                }

            }
        }

        private async void GetX100Info()
        {
            SendCmd(Command.VER);
            await Task.Delay(100);
            mcuLabel.Text = "MCU:" + mcuVer;

            SendCmd(Command.EXT1_IS_VAILD);
            await Task.Delay(100);
            ext1Label.Text = isVaildExt1 ? "�g���@�\1:�L��" : "�g���@�\1:����";


            SendCmd(Command.EXT2_IS_VAILD);
            await Task.Delay(100);
            ext2Label.Text = isVaildExt2 ? "�g���@�\2:�L��" : "�g���@�\2:����";

        }




        //�񓯊��ŃR�}���h��M�ҋ@���Ă���i�C�x���g�n���h���j
        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = e.Data;

            // �����M���̂�
            if (isFirstConnect)
            {
                if (response.Equals("\r\nDJ-X100\r\n"))
                {
                    isSucceedConnect = true;
                    return;
                }
            }



            switch (lastSendCmd)
            {

                case Command.EXT1_DISABLE:
                    if (IsResponseOk(response))
                    {
                        MessageBox.Show("�g���@�\1�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show(response, "����Ɏ��s���܂����B", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;
                case Command.EXT2_ENABLE:
                    if (IsResponseOk(response))
                    {
                        MessageBox.Show("�g���@�\2��L�������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show(response, "����Ɏ��s���܂����B", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;
                case Command.EXT2_DISABLE:
                    if (IsResponseOk(response))
                    {
                        MessageBox.Show("�g���@�\2�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show(response, "����Ɏ��s���܂����B", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return;

                case Command.VER:
                    mcuVer = response.Replace("\r\n", "");
                    mcuVer = mcuVer.Replace("ver", "");
                    return;
                case Command.EXT1_IS_VAILD:
                    isVaildExt1 = response.Equals("\r\n0001\r\n");
                    return;
                case Command.EXT2_IS_VAILD:
                    isVaildExt2 = response.Equals("\r\n0001\r\n");
                    return;


                default:
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
                    break;

            }
        }

        private void CloseConnection()
        {
            uart.Close();
            warnLabel.Text = "�ڑ����Ă��܂���";
            connectBtn.Text = "�ڑ�";

            msgOutputBtn.Enabled = false;
            isFirstConnect = true;
        }


        private void MsgOutputBtn_Click(object sender, EventArgs e)
        {
            if (msgOutputBtn.Text.Equals("���b�Z�[�W�o�͏I��"))
            {
                SendRawdCmd("QUIT\r\n");
                msgOutputBtn.Text = "���b�Z�[�W�o�͊J�n";
                warnLabel.Text = "DJ-X100�ڑ��ς�";


                ext1MenuItem.Enabled = true;
                ext2MenuItem.Enabled = true;

                return;
            }

            SendCmd(Command.OUTLINE);
            msgOutputBtn.Text = "���b�Z�[�W�o�͏I��";
            warnLabel.Text = "���b�Z�[�W�ҋ@���c(���g�����̕ύX����)";

            ext1MenuItem.Enabled = false;
            ext2MenuItem.Enabled = false;
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

        private async void ext1DisableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender)) SendCmd(Command.EXT1_DISABLE);

            SendCmd(Command.EXT1_IS_VAILD);
            await Task.Delay(100);
            ext1Label.Text = isVaildExt1 ? "�g���@�\1:�L��" : "�g���@�\1:����";
        }

        private async void ext2EnableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender)) SendCmd(Command.EXT2_ENABLE);
            SendCmd(Command.EXT2_IS_VAILD);
            await Task.Delay(200);
            ext2Label.Text = isVaildExt2 ? "�g���@�\2:�L��" : "�g���@�\2:����";
        }
        private async void ext2DisableBtn_Click(object sender, EventArgs e)
        {
            if (extend.IsExtendAccept(sender)) SendCmd(Command.EXT2_DISABLE);
            SendCmd(Command.EXT2_IS_VAILD);
            await Task.Delay(200);
            ext2Label.Text = isVaildExt2 ? "�g���@�\2:�L��" : "�g���@�\2:����";
        }
    }


}