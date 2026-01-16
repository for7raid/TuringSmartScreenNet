
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        private string _playerSongName = "My heard is go on";

        public string PlayerSongName
        {
            get { return _playerSongName; }
            set { if (_playerSongName == value) return; _playerSongName = value; OnPropertyChanged(); }
        }

        private string _playerArtistName = "Moby";

        public string PlayerArtistName
        {
            get { return _playerArtistName; }
            set { if (_playerArtistName == value) return; _playerArtistName = value; OnPropertyChanged(); }
        }

        public Func<double, string> CPUUsageLabelFormatting { get; } = (x) => $"{x}%";
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
