using System.Text.Json.Serialization;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public partial class Result<T>
    {
        public string channel { get; set; }

        [JsonPropertyName("data.data")]
        public T data { get; set; }
    }
}
