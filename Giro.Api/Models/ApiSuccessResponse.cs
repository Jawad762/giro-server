namespace Giro.Api.Models
{
    public class ApiSuccessResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiSuccessResponse() {}
    }
}
