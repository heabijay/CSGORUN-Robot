using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace CSGORUN_Robot.AppSettings
{
    public class Account : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));


        private string _AuthToken;
        public string AuthToken
        {
            get => _AuthToken;
            set
            {
                if (value != _AuthToken)
                {
                    _AuthToken = value;
                    OnPropertyChanged();
                }
            }
        }



        private Proxy _Proxy;
        public Proxy Proxy
        {
            get => _Proxy;
            set
            {
                if (value != _Proxy)
                {
                    _Proxy = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
