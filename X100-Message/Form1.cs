namespace X100_Message
{
    public partial class Form1 : Form
    {
        Uart uart = new Uart();

        // 
        private bool firstConnectFlg = true;
        private bool djx100ConnectFlg = false;
        private string lastSendCmd = "";

        public Form1()
        {
            InitializeComponent();
            uart.DataReceived += DataReceived;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitComPort();
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

                if (djx100ConnectFlg)
                {
                    connectBtn.Text = "�ؒf";
                    msgOutputBtn.Enabled = true;
                    djx100Ver.Enabled = true;
                    ext1EnableBtn.Enabled = true;
                    ext1DisableBtn.Enabled = true;
                    ext2EnableBtn.Enabled = true;
                    ext2DisableBtn.Enabled = true;
                }
                else
                {
                    MessageBox.Show("�ڑ��G���[���������܂���: ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CloseConnection();
                }

            }
        }



        //�񓯊��ŃR�}���h��M�ҋ@���Ă���i�C�x���g�n���h���j
        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = e.Data;

            // �����M��
            if (firstConnectFlg)
            {
                if (response.Equals("\r\nDJ-X100\r\n"))
                {
                    djx100ConnectFlg = true;
                    firstConnectFlg = false;
                    warnLabel.Text = "DJ-X100�ڑ��ς�";

                }
                else
                {
                    warnLabel.Text = "�ڑ����s";
                }
                return;
            }

            switch (lastSendCmd)
            {
                case Command.VER:
                    MessageBox.Show(response, "DJ-X100�o�[�W�������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                case Command.EXT1_DISABLE:
                    MessageBox.Show("�g���@�\1�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                case Command.EXT2_ENABLE:
                    MessageBox.Show("�g���@�\2��L�������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                case Command.EXT2_DISABLE:
                    MessageBox.Show("�g���@�\2�𖳌������܂���", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
            MessageBox.Show("�Đڑ�����ꍇ��DJ-X100���ċN�����Ă�������", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            warnLabel.Text = "�ڑ����Ă��܂���";
            connectBtn.Text = "�ڑ�";
            msgOutputBtn.Text = "���b�Z�[�W�o�͊J�n";
            msgOutputBtn.Enabled = false;
            firstConnectFlg = true;
        }


        private void MsgOutputBtn_Click(object sender, EventArgs e)
        {
            if (msgOutputBtn.Text.Equals("���b�Z�[�W�o�͏I��") && uart.IsOpen())
            {
                CloseConnection();
                return;
            }

            msgOutputBtn.Text = "���b�Z�[�W�o�͏I��";
            djx100Ver.Enabled = false;
            ext1EnableBtn.Enabled = false;
            ext1DisableBtn.Enabled = false;
            ext2EnableBtn.Enabled = false;
            ext2DisableBtn.Enabled = false;
            SendCmd(Command.OUTLINE);
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

        private void �I��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uart.Close();
            Application.Exit();
        }

        private void ���O�N���ACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.ResetText();
        }

        private void djx100Ver_Click(object sender, EventArgs e)
        {
            SendCmd(Command.VER);
        }

        private void �o�[�W�������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DJ-X100���b�Z�[�W���K�[\nVer1.2.0\nCopyright(C) 2023 by kaz", "�o�[�W�������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }


        private void ext1DisableBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("�g���@�\�֘A�̑���͎��ȐӔC�ł��B\n��낵���ł����H", "�x��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendCmd(Command.EXT1_DISABLE);
            }
        }

        private void ext2EnableBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("�g���@�\�֘A�̑���͎��ȐӔC�ł��B\n��낵���ł����H", "�x��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendCmd(Command.EXT2_ENABLE);
            }
        }
        private void ext2DisableBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("�g���@�\�֘A�̑���͎��ȐӔC�ł��B\n��낵���ł����H", "�x��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendCmd(Command.EXT2_DISABLE);
            }

        }

        private void ext1EnableBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("�H����", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        }
    }


}