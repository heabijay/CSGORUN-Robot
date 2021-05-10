using System.Text.Json.Serialization;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class ChatData
    {
        public string a { get; set; }

        [JsonPropertyName("p")]
        public ChatPayload payload { get; set; }
    }
}
