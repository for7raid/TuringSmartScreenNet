using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace TuringSmartScreenNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly HardwareInfoProvider monitor;

        private TcpListener listener;
        private List<TcpClient> clients = new List<TcpClient>(); // Keeps track of connected clients

        public ScreenDriver screen { get; }

        private readonly DispatcherTimer dispatcherTimer;

        private byte[] prevImage;
        private MainViewModel ViewModel { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            monitor = new HardwareInfoProvider();

            screen = new ScreenDriver("COM3");
            screen.Connect();
            //screen.SendCommand(Command.Reset);

            //Thread.Sleep(1000);

            screen.SendCommand(Command.ScreenOn);
            screen.SetOrientation(Orientation.PORTRAIT, 320, 480);
            screen.SendCommand(Command.Clear);
            screen.SetOrientation(Orientation.REVERSE_PORTRAIT, 320, 480);
            screen.SetBrightness(100);

            //dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            //dispatcherTimer.Interval = TimeSpan.FromMicroseconds(1000);

            StartTCP(10455);

            DataContext = ViewModel;


        }
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this, this);

            foreach (var element in RenderToImage.GetDiffs(prevImage, image.data))
            {
                screen.SendImage(element);
            }

            prevImage = image.data;


        }

        private void UpdateScreen()
        {
            while (true)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, () =>
                {
                    ViewModel.HardwareInfo = monitor.CollectInfo();
                    ViewModel.DateTimeNow = DateTime.Now;
                    var image = RenderToImage.SaveWpfElementAsBitmap(this, this);

                    foreach (var element in RenderToImage.GetDiffs(prevImage, image.data))
                    {
                        screen.SendImage(element);
                    }

                    prevImage = image.data;
                });

                Thread.Sleep(1000);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this, this);
            screen.SendImage(image);

            prevImage = image.data;

            //dispatcherTimer.Start();

            new Thread(UpdateScreen).Start();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //screen.SendCommand(Command.ScreenOff);
            //screen.Dispose();
        }

        public void StartTCP(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server started, waiting for connections...");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    clients.Add(client);
                    Console.WriteLine("Client connected.");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error accepting client: " + ex.Message);
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var jsonString = message.Split("\r\n\r\n")[1];

                    Debug.WriteLine(jsonString);

                    var payload = JsonSerializer.Deserialize<MultimediaMeta>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ViewModel.PlayerArtistName = payload.Artist;
                    ViewModel.PlayerSongName = payload.Title;
                    ViewModel.PlayBackState = payload.PlaybackState == "playing" ? "▶️" :
                            payload.PlaybackState == "paused" ? "⏸️" :
                            payload.PlaybackState == "stoped" ? "⏹️" :
                            payload.PlaybackState == "none" ? "" :
                            !string.IsNullOrWhiteSpace(ViewModel.PlayerSongName) ? "⏹️" :
                            string.Empty;
                    ViewModel.PlayerHostName = payload.Host?.Substring(0, 2) ?? string.Empty;

                    var response =
                        "HTTP/1.1 202 Accepted\r\n" +
                        "Access-Control-Allow-Origin: *\r\n" +
                        "Access-Control-Allow-Methods: *\r\n" +
                        "Access-Control-Allow-Headers: *\r\n" +
                        "Content-Length: 0\r\n" +
                        "Connection: close\r\n" +
                        "\r\n";

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error communicating with client: " + ex.Message);
            }
            finally
            {
                stream.Close();
                client.Close();
                clients.Remove(client);
                Console.WriteLine("Client disconnected.");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowState = WindowState.Minimized;
            e.Cancel = true;
        }
    }
}

public record class MultimediaMeta(string Artist, string Title, string PlaybackState, string Host);