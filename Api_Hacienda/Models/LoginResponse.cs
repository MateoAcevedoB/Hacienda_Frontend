namespace Api_Hacienda.Models
{
    
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Mismo valor que se usa como "rol" en ServicioAutenticacion /
        // ProveedorPermisosPorRol (ver comentario en IGeneradorTokenJwt):
        // el proyecto no tiene un concepto de Rol separado del Nombre.
        public string Rol { get; set; } = string.Empty;
    }
}
