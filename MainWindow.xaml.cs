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

        private MainViewModel ViewModel { get; set; } = new() { PlayerArtistName = "The Prodigy", PlayerSongName = "Smack My Bitch Up" };

        public MainWindow()
        {
            InitializeComponent();

            monitor = new HardwareInfoProvider();

            screen = new ScreenDriver("COM3");
            screen.Connect();
            //screen.SendCommand(Command.Reset);
            screen.SendCommand(Command.ScreenOn);
            screen.SetOrientation(Orientation.PORTRAIT, 320, 480);
            screen.SendCommand(Command.Clear);
            screen.SetOrientation(Orientation.REVERSE_PORTRAIT, 320, 480);

            //screen.SendCommand(Command.ScreenOff);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMicroseconds(500);
            dispatcherTimer.Start();

            DataContext = ViewModel;

            //var image = 
            //screen.SendImage(image);

           
        }
        public Func<double, string> CPUUsageLabelFormatting { get; } = (x) => $"{x}%";
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            //dispatcherTimer.Stop();
            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
            var image = RenderToImage.SaveWpfElementAsBitmap(this);
            screen.SendImage(image);  
        }
    }
}
