using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.AppSettings
{
    public class Proxy : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));



        private ProxyType _Type;
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


        private string _Address;
        public string Address
        {
            get => _Address;
            set
            {
                if (value != _Address)
                {
                    _Address = value;
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


        private string _Login;
        public string Login
        {
            get => _Login;
            set
            {
                if (value != _Login)
                {
                    _Login = value;
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
