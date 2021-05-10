using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class ErrorResponse
    {
        public string error { get; set; }
        public List<object> errors { get; set; }
    }
}
