namespace Work360.Domain.DTO
{
    public class LoginResponseDTO
    {
        public required string Token { get; set; }
        public int ExpiresIn { get; set; }
    }
}
