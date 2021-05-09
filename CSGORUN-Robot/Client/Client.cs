using CSGORUN_Robot.Settings;
using CSGORUN_Robot.Extensions;
using CSGORUN_Robot.Services;
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

        public ClientHttpService httpService { get; set; }

        public Client(Account account)
        {
            Account = account;
            httpService = new ClientHttpService(account.AuthToken, account.Proxy?.ToWebProxy());

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Account))
                {
                    Account.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(Account.AuthToken))
                            httpService.UpdateToken(Account.AuthToken);
                        else if (e.PropertyName == nameof(Account.Proxy))
                            httpService.UpdateProxy(Account.Proxy?.ToWebProxy());
                    };

                    httpService.UpdateToken(Account.AuthToken);
                    httpService.UpdateProxy(Account.Proxy?.ToWebProxy());
                }
            };
        }
    }
}
