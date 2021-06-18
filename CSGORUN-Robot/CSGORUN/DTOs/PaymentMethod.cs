using System;

namespace CSGORUN_Robot.CSGORUN.DTOs
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
