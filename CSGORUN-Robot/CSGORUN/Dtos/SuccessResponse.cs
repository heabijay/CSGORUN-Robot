using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.Dtos
{
    public class SuccessResponse<T> where T : new()
    {
        public bool success { get; set; }
        public DateTime date { get; set; }
        public T data { get; set; }
    }
}
