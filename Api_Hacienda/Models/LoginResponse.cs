namespace Api_Hacienda.Models
{
    
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}
