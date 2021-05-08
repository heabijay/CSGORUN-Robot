using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.Dtos
{
    public class Bet
    {
        public int id { get; set; }
        public int status { get; set; }
        public double coefficientAuto { get; set; }
        public double? coefficient { get; set; }
        public Deposit deposit { get; set; }
        public Withdraw withdraw { get; set; }
        public User user { get; set; }
    }
}
