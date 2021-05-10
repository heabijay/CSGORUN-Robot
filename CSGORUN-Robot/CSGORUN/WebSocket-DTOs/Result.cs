using System.Text.Json.Serialization;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class Result<T> where T: class
    {
        public string channel { get; set; }

        public DataWrapper<T> data { get; set; }
    }
}
