using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Twitch.DTOs
{
    public class UserMessage
    {
        public string Channel { get; set; }
        public string Nickname { get; set; }
        public string Message { get; set; }
    }
}
