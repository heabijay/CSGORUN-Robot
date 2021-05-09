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
    public class CSGORUN
    {
        public List<Account> Accounts { get; set; }

        public int? PrimaryAccountIndex { get; set; }

        public RegexPatterns RegexPatterns { get; set; }
    }
}
