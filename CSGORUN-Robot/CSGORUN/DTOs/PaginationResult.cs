using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class PaginationResult<T> where T: class
    {
        public int total { get; set; }
        public int pages { get; set; }
        public List<T> items { get; set; }
    }
}
