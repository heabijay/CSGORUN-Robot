using System.Text.Json.Serialization;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class User
    {
        [JsonPropertyName("i")]
        public int id { get; set; }

        [JsonPropertyName("n")]
        public string nickname { get; set; }

        [JsonPropertyName("s")]
        public string steamId { get; set; }

        [JsonPropertyName("a")]
        public string avatarUrl { get; set; }

        [JsonPropertyName("r")]
        public int role { get; set; }
    }
}
