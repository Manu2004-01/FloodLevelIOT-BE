namespace WebAPI.Errors
{
    public class ApiException : BaseCommentResponse
    {
        private readonly string details;

        public ApiException(int statusCode, string message = null, string Details = null) : base(statusCode, message)
        {
            details = Details;
        }

        public string Details { get; set; }
    }
}
