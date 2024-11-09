namespace Giro.Api.Models
{
    public class ApiErrorResponse<T>
    {
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public T? ErrorDetails { get; set; }
        public ApiErrorResponse() { }
    }
}
