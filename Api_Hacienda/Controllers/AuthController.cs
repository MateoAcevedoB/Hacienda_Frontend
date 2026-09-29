using Api_Hacienda.Autenticacion;
using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Hacienda.Controllers
{
    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ServicioAutenticacion servicioAutenticacion;
        private readonly IGeneradorTokenJwt generadorTokenJwt;

        public AuthController(ServicioAutenticacion servicioAutenticacion, IGeneradorTokenJwt generadorTokenJwt)
        {
            this.servicioAutenticacion = servicioAutenticacion;
            this.generadorTokenJwt = generadorTokenJwt;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Contrasena))
            {
                return BadRequest(new { mensaje = "Nombre y contraseña son obligatorios." });
            }

            bool credencialesValidas = servicioAutenticacion.ValidarCredenciales(request.Nombre, request.Contrasena);

            if (!credencialesValidas)
            {
                return Unauthorized(new { mensaje = "Nombre de usuario o contraseña incorrectos." });
            }

            Usuario usuario;
            try
            {
                usuario = servicioAutenticacion.buscar_usuario(request.Nombre);
            }
            catch (Exception)
            {
                return Unauthorized(new { mensaje = "Nombre de usuario o contraseña incorrectos." });
            }

            string rol = usuario.Nombre;

            TokenJwtGenerado tokenGenerado = generadorTokenJwt.GenerarToken(usuario, rol);

            return Ok(new LoginResponse
            {
                Token = tokenGenerado.Token,
                TokenType = "Bearer",
                ExpiresAt = tokenGenerado.ExpiraEn,
                Nombre = usuario.Nombre,
                Rol = rol
            });
        }
    }
}
