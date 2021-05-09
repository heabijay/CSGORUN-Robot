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
    public class ClientUnit
    {
        public Account Account { get; set; }

        public ClientHttpService HttpService { get; set; }

        public ClientUnit(Account account)
        {
            Account = account;
            HttpService = new ClientHttpService(account.AuthToken, account.Proxy?.ToWebProxy());
            account.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(account.AuthToken))
                    HttpService.UpdateToken(account.AuthToken);
            };
        }
    }
}
