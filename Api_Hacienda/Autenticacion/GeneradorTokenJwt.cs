using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bib_Hacienda.Dominio;
using Microsoft.IdentityModel.Tokens;

namespace Api_Hacienda.Autenticacion
{
    
    public class GeneradorTokenJwt : IGeneradorTokenJwt
    {
        private readonly IConfiguration configuracion;

        public GeneradorTokenJwt(IConfiguration configuracion)
        {
            this.configuracion = configuracion;
        }

        public TokenJwtGenerado GenerarToken(Usuario usuario, string rol)
        {
            string clave = configuracion["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(clave))
            {
                
                throw new InvalidOperationException(
                    "Falta configurar Jwt:Key. En desarrollo, agréguela a appsettings.Development.json; " +
                    "en producción debe provenir de User Secrets, variables de entorno o un secret manager.");
            }

            string issuer = configuracion["Jwt:Issuer"];
            string audience = configuracion["Jwt:Audience"];
            int expiracionMinutos = configuracion.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                // Mismo valor que ServicioAutenticacion usa como "rol" hoy
                // (usuario.Nombre); ver comentario en IGeneradorTokenJwt.
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                SecurityAlgorithms.HmacSha256);

            DateTime expiraEn = DateTime.UtcNow.AddMinutes(expiracionMinutos);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiraEn,
                signingCredentials: credenciales);

            string tokenSerializado = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenJwtGenerado(tokenSerializado, expiraEn);
        }
    }
}
