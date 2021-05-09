using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));


        private CSGORUN _CSGORUN;
        public CSGORUN CSGORUN
        {
            get => _CSGORUN;
            set
            {
                if (value != _CSGORUN)
                {
                    _CSGORUN = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
