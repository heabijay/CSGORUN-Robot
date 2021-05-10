using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public partial class SuccessResponse<T> where T: class
    {
        public Result<T> result { get; set; }
    }
}
