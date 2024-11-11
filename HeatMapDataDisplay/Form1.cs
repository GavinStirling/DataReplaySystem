using MathNet.Numerics.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System.Net.Sockets;
using System.Text;

namespace HeatMapDataDisplay
{
    public partial class Form1 : Form
    {

        private List<TcpClient> clients = new();
        private List<List<double>> dataStreams = new List<List<double>>();
        private PlotView plotView;

        public Form1()
        {
            InitializeComponent();
            Text = "Correlation Heatmap";
            Width = 1200;
            Height = 800;

            plotView = new PlotView 
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(plotView);

            SetupStreams();
        }

        private void SetupStreams()
        {
            List<(string ip, int port)> streamSources = new List<(string, int)>
            {
                ("127.0.0.1", 5000),
                ("127.0.0.1", 6000),

            };

            foreach (var source in streamSources)
            {
                TcpClient client = new TcpClient(source.ip, source.port);
                clients.Add(client);
                dataStreams.Add(new List<double>());

                Thread receiveThread = new Thread(() => ReceiveDate(client, dataStreams.Count - 1));
                receiveThread.Start();
            }

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000;
            timer.Tick += (s, e) => UpdateHeatmap();
            timer.Start();
        }

        private void ReceiveDate(TcpClient client, int streamIndex)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[256];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string receivedValue = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (double.TryParse(receivedValue, out double value))
                {
                    lock (dataStreams)
                    {
                        dataStreams[streamIndex].Add(value);
                    }
                }
            }
        }

        private void UpdateHeatmap()
        {
            List<List<double>> alignedData = AlignDataStream();
            double[,] correlationMatrix = CalculateCorrelationMatrix(alignedData);
            DisplayHeatMap(correlationMatrix);
        }

        private List<List<double>> AlignDataStream() {
            int minLength = int.MaxValue;
            foreach(var stream in dataStreams)
            {
                if(stream.Count < minLength)
                {
                    minLength = stream.Count;
                }
            }

            List<List<double>> alignedData = new List<List<double>>();
            foreach(var stream in dataStreams)
            {
                alignedData.Add(stream.GetRange(0, minLength));
            }
            return alignedData;
        }

        private double[,] CalculateCorrelationMatrix(List<List<double>> alignedData)
        {
            int numStreams = alignedData.Count;
            double[,] matrix = new double[numStreams, numStreams];

            for (int i = 0; i < numStreams; i++)
            {
                for (int j = 0; j < numStreams; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 1.0;
                    }
                    else
                    {
                        matrix[i, j] = Correlation.Pearson(alignedData[i], alignedData[j]);
                    }
                }
            }
            return matrix;
        }

        private void DisplayHeatMap(double[,] correlationMatrix)
        {
            int size = correlationMatrix.GetLength(0);
            var plotModel = new PlotModel { Title = "Correlation Matrix" };

            var heatMap = new HeatMapSeries
            {
                X0 = 0,
                X1 = size,
                Y0 = 0,
                Y1 = size,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Data = correlationMatrix
            };
            plotModel.Series.Add(heatMap);

            plotModel.Axes.Add(new LinearColorAxis { Palette = OxyPalettes.Jet(200), Position = AxisPosition.Right });
            plotView.Model = plotModel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
