using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System.Net.Sockets;
using System.Text;

namespace TimePlotDataDisplay
{
    public partial class Form1 : Form
    {
        private LineSeries lineSeries;
        private PlotView plotView;
        private List<(DateTime, double)> dataPoints;
        private DbInstance _dbInstance;

        public Form1(DbInstance dbInstance)
        {
            InitializeComponent();
            _dbInstance = dbInstance;

            dataPoints = new List<(DateTime, double)>();

            lineSeries = new LineSeries { Title = "Random Walk" };
            plotView = new PlotView
            {
                Dock = DockStyle.Fill,
                Model = new PlotModel { Title = "Random Walk" }

            };
            plotView.Model.Series.Add(lineSeries);
            plotView.Model.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat="HH:mm:ss" });

            Controls.Add(plotView);
            Text = "Random Walk Time Series";
            Width = 1200;
            Height = 1000;            

            LoadHistoricalData();
            StartDataCollection();
        }

        private void LoadHistoricalData()
        {
            var historicalData = _dbInstance.GetAllFromDb();
            dataPoints.AddRange(historicalData);
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
                        dataPoints.Add((DateTime.Now, value));

                        Invoke((MethodInvoker)delegate {
                            lineSeries.Points.Clear();
                            for (int i = 0; i < dataPoints.Count; i++)
                            {
                                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dataPoints[i].Item1), dataPoints[i].Item2 ));
                            }
                            plotView.Model.InvalidatePlot(true);
                        });
                    }
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
