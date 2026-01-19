using LibreHardwareMonitor.Hardware;
using RAMSPDToolkit.Windows.Driver.Interfaces;
using System.Diagnostics;
using System.Management;
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

        public ScreenDriver screen { get; }

        private readonly DispatcherTimer dispatcherTimer;

        private byte[] prevImage;
        private MainViewModel ViewModel { get; set; } = new() { PlayerArtistName = "The Prodigy", PlayerSongName = "Smack My Bitch Up" };

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

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMicroseconds(1000);


            DataContext = ViewModel;


        }
        static Dictionary<string, (int x, int y, int width, int height)> elements = new()
        {
            {"date", (10, 15, 130, 20)},
            {"time", (205, 15, 115, 20)},
            {"CPUUsage", (38, 73, 95, 97)},
            {"CPUTempCap", (160, 70, 100, 20)},
            {"CPUTempGuage", (260, 52, 55, 48)},
            {"CPUClockCap", (160, 135, 95, 20)},
            {"CPUClockGuage", (260, 115, 55, 48)},
            {"RAM", (38, 200, 95, 97)},
            {"GPU", (193, 200, 95, 97)},
            {"FANControl", (30, 322, 105, 95)},
            {"FANRPM", (170, 363, 150, 30)},
        };
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            //dispatcherTimer.Stop();
            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this, this);


            //foreach (var element in elements.Values)
            //{
            //    var elImage = RenderToImage.Crop(image.data, element.x, element.y, element.width, element.height);
            //    screen.SendImage(elImage);
            //}

            foreach(var element in RenderToImage.GetDiffs(prevImage, image.data))
            {
                screen.SendImage(element);
            }

            prevImage = image.data;


        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this, this);
            screen.SendImage(image);

            prevImage = image.data;



            dispatcherTimer.Start();

            RenderToImage.SaveWpfElementAsImage(this, "c:\\temp\\window.png");
            //RenderToImage.SaveWpfElementAsImage(grid, "c:\\temp\\grid.png");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            screen.SendCommand(Command.ScreenOff);
            screen.Dispose();
        }
    }
}
