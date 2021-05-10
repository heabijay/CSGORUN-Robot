using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class DataWrapper<T> where T: class
    {
        public T data { get; set; }
    }
}
