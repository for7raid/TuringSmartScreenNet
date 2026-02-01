using System.Management;
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

        private TcpListener? listener;

        public ScreenDriver screen { get; }

        private readonly CancellationTokenSource _Cts;
        private readonly CancellationToken _cancellationToken;
        private byte[]? prevImage;

        private MainViewModel ViewModel { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            monitor = new HardwareInfoProvider();

            screen = new ScreenDriver("COM3");
            screen.Connect();
            screen.SendCommand(Command.ScreenOn);
            screen.SendCommand(Command.Clear);
            screen.SetOrientation(Orientation.REVERSE_PORTRAIT, 320, 480);
            screen.SetBrightness(100);

            StartTCP();

            DataContext = ViewModel;

            _Cts = new CancellationTokenSource();
            _cancellationToken = _Cts.Token;



        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this);
            screen.SendImage(image);

            prevImage = image.Data;

            new Thread(UpdateScreen).Start();

        }
        private void UpdateScreen()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, () =>
                {
                    ViewModel.HardwareInfo = monitor.CollectInfo();
                    ViewModel.DateTimeNow = DateTime.Now;

                    if (ViewModel.DateTimeNow.Second % 10 == 0)
                    {
                        ViewModel.BluetoothStatus = GetBluetoothBatteryStatus();
                    }

                    var image = RenderToImage.SaveWpfElementAsBitmap(this);

                    foreach (var element in RenderToImage.GetDiffs(prevImage, image.Data))
                    {
                        screen.SendImage(element);
                    }

                    prevImage = image.Data;
                });

                Thread.Sleep(1000);
            }
        }
        private string GetBluetoothBatteryStatus()
        {
            bool isConnected = false;
            byte status = 0;

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%Studio Wireless%'");
            var list = searcher.Get();
            foreach (ManagementObject obj in list)
            {
                //string devId = obj["DeviceID"]?.ToString() ?? "";
                //string caption = obj["Caption"]?.ToString() ?? "";
                //string PNPClass = obj["PNPClass"]?.ToString() ?? "";
                //var pp = obj.Properties;

                var batteryStatusProperty = "{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2";
                var connectionStatusProperty = "{83DA6326-97A6-4088-9453-A1923F573B29} 15";

                var batteryStatus = GetDeviceProperty<byte?>(obj, batteryStatusProperty);
                if (batteryStatus.HasValue)
                {
                    status = batteryStatus.Value;
                }

                var isConnectedValue = GetDeviceProperty<bool?>(obj, connectionStatusProperty);
                if (isConnectedValue.HasValue && isConnectedValue.Value)
                {
                    isConnected = isConnectedValue.Value;
                }

            }
            if (isConnected)
            {
                return $"ᛒ {status}%";
            }
            else
            {
                return string.Empty;
            }
        }

        private T? GetDeviceProperty<T>(ManagementObject obj, string propName)
        {
            var args = new object[] { new string[] { propName }, null! };
            try
            {
                obj.InvokeMethod("GetDeviceProperties", args);
                using ManagementBaseObject? ss = (args[1] as ManagementBaseObject[])?[0];
                var data = ss?.Properties
                       .Cast<PropertyData>()
                       .FirstOrDefault(x => x.Name == "Data")?.Value;
                return (T?)data;

            }
            catch
            {
                return default;
            }
        }

        public void StartTCP(int port = 10455)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server started, waiting for connections...");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private async void AcceptClients()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(_cancellationToken);

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
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var firstLine = request.Split('\n')[0].Trim();
                    var parts = firstLine.Split(' ');
                    var method = parts[0]?.ToUpper();
                    var url = parts[1];

                    if (method == "POST" && url == "/")
                    {
                        var jsonString = request.Split("\r\n\r\n")[1];
                        UpdatePlayerStatus(stream, jsonString);
                    }
                    else if (method == "GET" && url == "/")
                    {
                        GetHTMLStatusPage(stream);
                    }
                    else if (method == "POST" && url.StartsWith("/player/"))
                    {
                        SendMediaKey(url);
                        Thread.Sleep(1000);
                        GetHTMLStatusPage(stream);
                    }
                    else
                    {
                        GetHTMLStatusPage(stream);
                        //var response =
                        //    "HTTP/1.1 404 Not Found\r\n" +
                        //    "Content-Length: 0\r\n" +
                        //    "Connection: close\r\n" +
                        //    "\r\n";

                        //byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        //stream.Write(responseBytes, 0, response.Length);
                    }

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
                Console.WriteLine("Client disconnected.");
            }
        }

        private void SendMediaKey(string url)
        {
            var parts = url.Split("/");
            var button = parts[^1]?.ToUpper();
            switch (button)
            {
                case "PLAY-PAUSE":
                    MediaKeys.PlayPause();
                    break;
                case "PREV":
                    MediaKeys.Previous();
                    break;
                case "NEXT":
                    MediaKeys.Next();
                    break;
                case "VOLUME-UP":
                    MediaKeys.VolumeUp();
                    break;
                case "VOLUME-DOWN":
                    MediaKeys.VolumeDown();
                    break;
            }
        }

        private void GetHTMLStatusPage(NetworkStream stream)
        {
            string html = string.Format(htmlPageTemplate, ViewModel.PlayerArtistName, ViewModel.PlayerSongName);

            byte[] body = Encoding.UTF8.GetBytes(html);

            // (3) HTTP-ответ
            string headers =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: text/html; charset=utf-8\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "Connection: close\r\n" +
                "\r\n";

            byte[] headerBytes = Encoding.ASCII.GetBytes(headers);

            // (4) Отдаём ответ
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
        }

        private void UpdatePlayerStatus(NetworkStream stream, string jsonString)
        {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _Cts.Cancel();
            screen.Dispose();
            Application.Current.Shutdown();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            Activate();
        }

        string htmlPageTemplate = $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""UTF-8"">
    <title>Плеер</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body>

<h2 style=""font-size:clamp(18px, 4vw, 28px);"">
    {{0}} — {{1}}
</h2>

<div>
    <form method=""post"" action=""/player/prev"" style=""display:inline-block;"">
        <button
            type=""submit""
            style=""
                width:clamp(56px, 18vw, 80px);
                height:clamp(56px, 18vw, 80px);
                font-size:clamp(22px, 8vw, 32px);
            "">
            ⏮
        </button>
    </form>

    <form method=""post"" action=""/player/play-pause"" style=""display:inline-block;"">
        <button
            type=""submit""
            style=""
                width:clamp(56px, 18vw, 80px);
                height:clamp(56px, 18vw, 80px);
                font-size:clamp(22px, 8vw, 32px);
            "">
            ⏯
        </button>
    </form>

    <form method=""post"" action=""/player/next"" style=""display:inline-block;"">
        <button
            type=""submit""
            style=""
                width:clamp(56px, 18vw, 80px);
                height:clamp(56px, 18vw, 80px);
                font-size:clamp(22px, 8vw, 32px);
            "">
            ⏭
        </button>
    </form>
</div>

<br>

<div>
    <form method=""post"" action=""/player/volume-down"" style=""display:inline-block;"">
        <button style=""width:clamp(56px,18vw,80px);height:clamp(56px,18vw,80px);font-size:clamp(22px,8vw,32px);"">
            🔉
        </button>
    </form>


    <form method=""post"" action=""/player/volume-up"" style=""display:inline-block;"">
        <button style=""width:clamp(56px,18vw,80px);height:clamp(56px,18vw,80px);font-size:clamp(22px,8vw,32px);"">
            🔊
        </button>
    </form>
</div>

</body>
</html>



";
    }
}

public record class MultimediaMeta(string Artist, string Title, string PlaybackState, string Host);