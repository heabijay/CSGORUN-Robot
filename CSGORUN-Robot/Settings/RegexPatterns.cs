using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public class RegexPatterns : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));


        private string _Default;
        public string Default
        {
            get => _Default;
            set
            {
                if (value != _Default)
                {
                    _Default = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _EN_Admins;
        public string EN_Admins
        {
            get => _EN_Admins;
            set
            {
                if (value != _EN_Admins)
                {
                    _EN_Admins = value;
                    OnPropertyChanged();
                }
            }
        }



        private string _RU_Admins;
        public string RU_Admins
        {
            get => _RU_Admins;
            set
            {
                if (value != _RU_Admins)
                {
                    _RU_Admins = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
