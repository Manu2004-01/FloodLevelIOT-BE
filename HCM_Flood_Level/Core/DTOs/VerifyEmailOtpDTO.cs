namespace Core.DTOs
{
    public class VerifyEmailOtpDTO
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
