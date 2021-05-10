using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class User
    {
        public int id { get; set; }
        public string steamId { get; set; }
        public string name { get; set; }
        public double deposit { get; set; }
        public bool hasDeposit { get; set; }
        public int depositCount { get; set; }
        public int steamLevel { get; set; }
        public string avatar { get; set; }
        public double balance { get; set; }
        public object mutedAt { get; set; }
        public int lang { get; set; }
        public int role { get; set; }
        public List<Item> items { get; set; }
        public List<Sticker> stickers { get; set; }
        public bool blm { get; set; }
    }
}
