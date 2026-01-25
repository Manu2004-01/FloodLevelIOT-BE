namespace WebAPI.Errors
{
    public class ApiValidationErrorResponse : BaseCommentResponse
    {
        public ApiValidationErrorResponse() : base(400)
        {
        }

        public IEnumerable<string> Errors { get; set; }
    }
}
