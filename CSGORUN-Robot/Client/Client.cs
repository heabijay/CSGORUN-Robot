using CSGORUN_Robot.AppSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Client
{
    public class Client : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));


        private Account _Account;
        public Account Account
        {
            get => _Account;
            set
            {
                if (value != _Account)
                {
                    _Account = value;
                    OnPropertyChanged();
                }
            }
        }


        public Client(Account account)
        {
            Account = account;
        }
    }
}
