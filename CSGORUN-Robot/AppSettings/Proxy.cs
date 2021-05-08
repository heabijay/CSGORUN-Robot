using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSGORUN_Robot.AppSettings
{
    public class Proxy : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));



        private ProxyType _Type;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProxyType Type
        {
            get => _Type;
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _Host;
        public string Host
        {
            get => _Host;
            set
            {
                if (value != _Host)
                {
                    _Host = value;
                    OnPropertyChanged();
                }
            }
        }



        private int _Port;
        public int Port
        {
            get => _Port;
            set
            {
                if (value != _Port)
                {
                    _Port = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _Username;
        public string Username
        {
            get => _Username;
            set
            {
                if (value != _Username)
                {
                    _Username = value;
                    OnPropertyChanged();
                }
            }
        }



        private string _Password;
        public string Password
        {
            get => _Password;
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
