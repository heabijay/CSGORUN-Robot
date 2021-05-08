using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.Dtos
{
    public class PaymentMethod
    {
        public int id { get; set; }
        public string name { get; set; }
        public string titleRu { get; set; }
        public string titleEn { get; set; }
        public string type { get; set; }
        public bool isActive { get; set; }
        public int order { get; set; }
        public double minAmount { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
