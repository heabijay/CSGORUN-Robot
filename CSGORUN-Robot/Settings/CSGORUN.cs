using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public class CSGORUN : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private ObservableCollection<Account> _Accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set
            {
                if (value != _Accounts)
                {
                    _Accounts = value;
                    OnPropertyChanged();
                }
            }
        }


        private int? _PrimaryAccountId = 0;
        public int? PrimaryAccountId
        {
            get => _PrimaryAccountId;
            set
            {
                if (value != _PrimaryAccountId)
                {
                    _PrimaryAccountId = value;
                    OnPropertyChanged();
                }
            }
        }


        private RegexPatterns _RegexPatterns;
        public RegexPatterns RegexPatterns
        {
            get => _RegexPatterns;
            set
            {
                if (value != _RegexPatterns)
                {
                    _RegexPatterns = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
