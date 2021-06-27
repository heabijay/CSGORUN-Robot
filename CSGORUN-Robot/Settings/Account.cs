using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSGORUN_Robot.Settings
{
    public class Account : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));


        private string _authToken;
        public string AuthToken
        {
            get => _authToken;
            set
            {
                if (value != _authToken)
                {
                    _authToken = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36";
        public string UserAgent
        {
            get => _userAgent;
            set
            {
                if (value != _userAgent)
                {
                    _userAgent = value;
                    OnPropertyChanged();
                }
            }
        }



        private Proxy _proxy;
        public Proxy Proxy
        {
            get => _proxy;
            set
            {
                if (value != _proxy)
                {
                    _proxy = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
