using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.Dtos
{
    public class Deposit
    {
        public double amount { get; set; }
        public List<Item> items { get; set; }
    }
}
