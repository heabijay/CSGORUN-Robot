namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class WithdrawItem
    {
        public int id { get; set; }
        public WithdrawStatus status { get; set; }
        public double amount { get; set; }
        public Item item { get; set; }
    }
}
