using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Hacienda.Controllers
{
   
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(Roles = "admin")]
    public class UsuariosController : ControllerBase
    {
        private readonly ServicioAutenticacion servicioAutenticacion;

        public UsuariosController(ServicioAutenticacion servicioAutenticacion)
        {
            this.servicioAutenticacion = servicioAutenticacion;
        }

        [HttpGet]
        public ActionResult<List<UsuarioResponse>> ObtenerTodos()
        {
            List<Usuario> usuarios = servicioAutenticacion.listar_usuarios();
            List<UsuarioResponse> respuesta = usuarios.Select(UsuarioResponse.DesdeUsuario).ToList();
            return Ok(respuesta);
        }

        [HttpPost]
        public ActionResult<ResultadoOperacion> Crear([FromBody] CrearUsuarioRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Contrasena))
            {
                return BadRequest(new { mensaje = "Nombre y contraseña son obligatorios." });
            }

            ResultadoOperacion resultado = servicioAutenticacion.crear_usuario(request.Nombre, request.Contrasena);

            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            return StatusCode(StatusCodes.Status201Created, resultado);
        }
    }
}
