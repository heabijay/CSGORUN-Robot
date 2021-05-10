using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class Withdraw
    {
        public double? amount { get; set; }
        public List<object> items { get; set; }
    }
}
