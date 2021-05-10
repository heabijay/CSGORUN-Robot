namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class SuccessResponse<T> where T : class
    {
        public Result<T> result { get; set; }
    }
}
