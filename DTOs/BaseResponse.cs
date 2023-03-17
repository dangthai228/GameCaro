namespace Caro.Game.DTOs
{
    public class BaseResponse
    {
        public BaseResponse()
        {
        }
        public BaseResponse(long status, dynamic data)
        {
            this.status = status;
            this.data = data;
        }
        public long status { get; set; }
        public dynamic data { get; set; }
    }
}
