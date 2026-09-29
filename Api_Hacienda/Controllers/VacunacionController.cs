using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Api_Hacienda.Controllers
{
    
    [ApiController]
    [Route("api/vacunacion")]
    [Authorize]
    public class VacunacionController : ControllerBase
    {
        private readonly ServicioPotreros servicioPotreros;
        private readonly ServicioInventarioVacunas servicioInventarioVacunas;
        private readonly ServicioVacunacion servicioVacunacion;

        public VacunacionController(
            ServicioPotreros servicioPotreros,
            ServicioInventarioVacunas servicioInventarioVacunas,
            ServicioVacunacion servicioVacunacion)
        {
            this.servicioPotreros = servicioPotreros;
            this.servicioInventarioVacunas = servicioInventarioVacunas;
            this.servicioVacunacion = servicioVacunacion;
        }

        
        [HttpPost("aplicar")]
        public ActionResult<ResultadoOperacion> Aplicar([FromBody] AplicarVacunaRequest request)
        {
            Potrero? potrero = servicioPotreros.ObtenerPotreroPorIdentificacion(request.PotreroId);
            if (potrero == null)
            {
                return NotFound(new { mensaje = $"No se encontró el potrero '{request.PotreroId}'." });
            }

            Res? res = servicioPotreros.BuscarRes(request.PotreroId, request.NombreRes);
            if (res == null)
            {
                return NotFound(new { mensaje = $"No se encontró la res '{request.NombreRes}' en el potrero '{request.PotreroId}'." });
            }

    
            Vacuna? vacuna = servicioInventarioVacunas
                .ObtenerVacunasDisponibles()
                .FirstOrDefault(v => v.Lote == request.LoteVacuna);

            if (vacuna == null)
            {
                return NotFound(new { mensaje = $"No se encontró en el inventario disponible ninguna vacuna con el lote '{request.LoteVacuna}'." });
            }

            ResultadoOperacion resultado = servicioVacunacion.aplicar_vacuna(vacuna, request.NombreRes, request.PotreroId);

            if (!resultado.Exito)
            {
                
                return BadRequest(resultado);
            }

            
            return Ok(resultado);
        }
    }
}
