using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.Dtos
{
    public class Item
    {
        public int id { get; set; }
        public double price { get; set; }
        public int? itemId { get; set; }
        public string name { get; set; }
        public string entity { get; set; }
        public string description { get; set; }
        public int? qualityId { get; set; }
        public int? colorId { get; set; }
        public bool? isSlowWithdraw { get; set; }
    }
}
