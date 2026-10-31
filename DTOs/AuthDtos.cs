namespace PR3.DTOs
{
    public class AuthDtos
    {
        public record RegisterRequest(string Login, string Password, string Phone, string Address) { }
        public record LoginRequest(string Login, string Password) { }
        public record AuthResponse(string Token, string TokenType) { }
    }
}
