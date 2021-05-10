using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;

namespace CSGORUN_Robot.Client
{
    public class ClientWorker
    {
        public Account Account { get; set; }

        public ClientHttpService HttpService { get; set; }

        public ClientWorker(Account account)
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
