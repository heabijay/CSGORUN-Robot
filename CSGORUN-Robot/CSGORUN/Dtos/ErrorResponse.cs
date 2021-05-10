using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class ErrorResponse
    {
        public string error { get; set; }
        public List<object> errors { get; set; }
    }
}
