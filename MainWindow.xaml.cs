using LibreHardwareMonitor.Hardware;
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
        private readonly DispatcherTimer dispatcherTimer;

        private MainViewModel ViewModel { get; set; } = new() { PlayerArtistName = "The Prodigy", PlayerSongName = "Smack My Bitch Up" };

        public MainWindow()
        {
            InitializeComponent();

            monitor = new HardwareInfoProvider();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

            DataContext = ViewModel;
        }
        public Func<double, string> CPUUsageLabelFormatting { get; } = (x) => $"{x}%";
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            ViewModel.HardwareInfo = monitor.CollectInfo();
            ViewModel.DateTimeNow = DateTime.Now;
        }
    }
}
