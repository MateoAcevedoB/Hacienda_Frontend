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

        // ------------------------------------------------------------
        // POST /api/auth/login  (sin [Authorize]: es el único punto de
        // entrada para obtener el token).
        // ------------------------------------------------------------
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
                // ValidarCredenciales ya confirmó que el usuario existe;
                // buscar_usuario reutiliza la misma fuente (IRepositorioUsuarios)
                // para obtener el objeto Usuario y armar los claims del JWT.
                usuario = servicioAutenticacion.buscar_usuario(request.Nombre);
            }
            catch (Exception)
            {
                // Caso extremadamente improbable (usuario borrado entre
                // ValidarCredenciales y buscar_usuario); se trata como
                // credenciales inválidas en vez de exponer un 500.
                return Unauthorized(new { mensaje = "Nombre de usuario o contraseña incorrectos." });
            }

            // "rol": ver comentario en IGeneradorTokenJwt — el proyecto no
            // modela un Rol separado del Nombre de usuario (admin/empleado/
            // visitante SON, a la vez, nombres de usuario y roles, igual que
            // en ProveedorPermisosPorRol.TienePermiso). No se inventa un
            // campo Rol nuevo en Usuario.
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
