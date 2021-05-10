using System.Text.Json.Serialization;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class ChatPayload
    {
        [JsonPropertyName("i")]
        public int id { get; set; }

        [JsonPropertyName("c")]
        public string message { get; set; }

        [JsonPropertyName("t")]
        public string time { get; set; }

        [JsonPropertyName("u")]
        public User user { get; set; }
        public int type { get; set; }
    }
}
