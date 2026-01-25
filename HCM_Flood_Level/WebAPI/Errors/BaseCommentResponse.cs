namespace WebAPI.Errors
{
    public class BaseCommentResponse
    {
        public int Statuscodes { get; set; }
        public string Message { get; set; }

        public BaseCommentResponse(int statusCode)
        {
            this.Statuscodes = statusCode;
            this.Message = DefaultMessageForStatuscodes(statusCode);
        }

        public BaseCommentResponse(int statusCode, string message)
        {
            this.Statuscodes = statusCode;
            this.Message = message ?? DefaultMessageForStatuscodes(statusCode);
        }

        private string DefaultMessageForStatuscodes(int statuscodes)
        {
            return statuscodes switch
            {
                400 => "Bad Request",
                401 => "Not Athorize",
                404 => "Resource Not Found",
                500 => "Server Error",
                _ => null
            };
        }
    }
}
