namespace X100_Message
{
    public partial class Form1 : Form
    {
        Uart uart = new Uart();


        // 
        private bool firstConnectFlg = true;
        private bool djx100ConnectFlg = false;
        private string lastCommandSent = "";

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

        // �����@�ɐڑ�
        private async void ConnectBtn_Click(object sender, EventArgs e)
        {

            if (connectBtn.Text.Equals("�ؒf"))
            {
                CloseConnection();
                return;
            }


            if (uart.InitSerialPort(comComboBox.Text))
            {
                await Task.Delay(500);

                if (djx100ConnectFlg)
                {
                    connectBtn.Text = "�ؒf";
                    msgOutputBtn.Enabled = true;
                }
                else
                {
                    MessageBox.Show("�ڑ��G���[���������܂���: ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CloseConnection();
                }

            }
        }




        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = e.Data;


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

            switch (lastCommandSent)
            {
                case "VER":
                    MessageBox.Show(response, "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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

            //await Task.Delay(100);
            uart.SendCmd(Command.OUTLINE);
        }



        // COM�|�[�g�ꗗ�̏���������
        private void InitComPort()
        {
            foreach (String portName in uart.GetPortLists())
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

    }


}