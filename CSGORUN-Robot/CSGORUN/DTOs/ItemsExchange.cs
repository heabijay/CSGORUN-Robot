using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class ItemsExchange
    {
        public double balance { get; set; }
        public double spent { get; set; }
        public UserItems userItems { get; set; }
    }
}
