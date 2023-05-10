using System.IO.Ports;
using System.Text;



namespace X100_Message
{
    public partial class Form1 : Form
    {

        private static string DJ_X100_OPEN = "AL~DJ-X100\r";
        private SerialPort serialPort;

        private bool firstConnectFlg = true;
        private bool djx100ConnextFlg = false;

        public Form1()
        {
            InitializeComponent();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitComPort();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
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
            if (InitSerialPort())
            {
                await Task.Delay(500);

                if (djx100ConnextFlg)
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

        private bool InitSerialPort()
        {
            // COM�|�[�g�̐ݒ�
            var portName = comComboBox.Text;
            var baudRate = 9600;
            var dataBits = 8;
            var parity = Parity.None;
            var stopBits = StopBits.One;

            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;

            serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                serialPort.Open();
                var open = Encoding.ASCII.GetBytes(DJ_X100_OPEN);
                serialPort.BaseStream.WriteAsync(open, 0, open.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("�ڑ��G���[���������܂���: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                serialPort.Close();
                return false;
            }
            return true;
        }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var buffer = new byte[serialPort.BytesToRead];
            var bytesRead = serialPort.Read(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (firstConnectFlg)
            {
                if (response.Equals("\r\nOK\r\n"))
                {
                    djx100ConnextFlg = true;
                    firstConnectFlg = false;
                    warnLabel.Text = "DJ-X100�ڑ��ς�";
                }
                else
                {
                    warnLabel.Text = "�ڑ����s";
                }
                return;
            }
            if (response.Equals("\r\nOK\r\n"))
            {
                return;
            }

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
            serialPort.Close();
            MessageBox.Show("�Đڑ�����ꍇ��DJ-X100���ċN�����Ă�������", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            warnLabel.Text = "�ڑ����Ă��܂���";
            connectBtn.Text = "�ڑ�";
            msgOutputBtn.Text = "���b�Z�[�W�o�͊J�n";
            msgOutputBtn.Enabled = false;
            firstConnectFlg = true;

        }

        private async void MsgOutputBtn_Click(object sender, EventArgs e)
        {
            if (msgOutputBtn.Text.Equals("���b�Z�[�W�o�͏I��") && serialPort.IsOpen)
            {
                CloseConnection();
                return;
            }

            msgOutputBtn.Text = "���b�Z�[�W�o�͏I��";

            var open = Encoding.ASCII.GetBytes(DJ_X100_OPEN);
            await serialPort.BaseStream.WriteAsync(open, 0, open.Length);
            await Task.Delay(100);

            var thru = Encoding.ASCII.GetBytes("AL~THRU\r");
            await serialPort.BaseStream.WriteAsync(thru, 0, thru.Length);
        }



        // COM�|�[�g�ꗗ�̏���������
        private void InitComPort()
        {
            String[] portList = SerialPort.GetPortNames();

            foreach (String portName in portList)
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
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            Application.Exit();
        }

        private void ���O�N���ACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.ResetText();
        }
    }


}