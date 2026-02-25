
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace TuringSmartScreenNet
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private HardwareInfo _hardwareInfo = new();
        public HardwareInfo HardwareInfo
        {
            get => _hardwareInfo;
            set
            {
                if (_hardwareInfo == value) return;
                _hardwareInfo = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeNow = DateTime.Now;

        public DateTime DateTimeNow
        {
            get { return _dateTimeNow; }
            set { _dateTimeNow = value; OnPropertyChanged(); }
        }

        private string _playerSongName;

        public string PlayerSongName
        {
            get { return _playerSongName; }
            set { if (_playerSongName == value) return; _playerSongName = value; OnPropertyChanged(); }
        }

        private string _playerArtistName;
        public string PlayerArtistName
        {
            get { return _playerArtistName; }
            set { if (_playerArtistName == value) return; _playerArtistName = value; OnPropertyChanged(); }
        }

        private string _playBackState;

        public string PlayBackState
        {
            get { return _playBackState; }
            set { if (_playBackState == value) return;  _playBackState = value; OnPropertyChanged(); }
        }

        private string _playerHostName;

        public string PlayerHostName
        {
            get { return _playerHostName; }
            set { if (_playerHostName == value) return; _playerHostName = value; OnPropertyChanged(); }
        }

        private string _bluetoothStatus;
        public string BluetoothStatus
        {
            get { return _bluetoothStatus; }
            set { if (_bluetoothStatus == value) return; _bluetoothStatus = value; OnPropertyChanged(); }
        }
        public Func<double, string> CPUUsageLabelFormatting { get; } = (x) => $"{x}%";


        private ImageSource? _conway;
        public ImageSource? Conway
        {
            get { return _conway; }
            set { _conway = value; OnPropertyChanged(); }
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
