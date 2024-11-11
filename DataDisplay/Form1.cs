using System.Net.Sockets;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DataDisplay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StartDataCollection();
        }

        private void StartDataCollection()
        {
            Thread receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void ReceiveData()
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 5000);
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[256];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string receivedValue = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (double.TryParse(receivedValue, out double value))
                    {

                        Invoke((MethodInvoker)delegate () { lblData.Text = receivedValue; });
                    }
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private delegate void UpdateLabelDelegate(string status);
        public void UpdateLabel(string updatedLabel)
        {
            if (lblData.InvokeRequired)
            {
                lblData.Invoke((MethodInvoker)delegate () { lblData.Text = updatedLabel; });
                return;
            }
            lblData.Text = updatedLabel;  
        }
    }
}
