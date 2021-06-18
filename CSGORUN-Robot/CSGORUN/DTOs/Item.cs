namespace CSGORUN_Robot.CSGORUN.DTOs
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
