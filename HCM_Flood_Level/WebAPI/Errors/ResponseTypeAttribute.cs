using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Errors
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        public ResponseTypeAttribute(int statusCode) : base(statusCode)
        {
        }

        public ResponseTypeAttribute(Type type, int statusCode) : base(type, statusCode)
        {
        }
    }
}
