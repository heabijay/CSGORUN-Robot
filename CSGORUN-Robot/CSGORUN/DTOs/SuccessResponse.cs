using System;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class SuccessResponse<T> where T : new()
    {
        public bool success { get; set; }
        public DateTime date { get; set; }
        public T data { get; set; }
    }
}
