using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Api_Hacienda.Controllers
{
    
    [ApiController]
    [Route("api/potreros")]
    [Authorize]
    public class PotrerosController : ControllerBase
    {
        private readonly ServicioPotreros servicioPotreros;

        public PotrerosController(ServicioPotreros servicioPotreros)
        {
            this.servicioPotreros = servicioPotreros;
        }

        // ------------------------------------------------------------
        // ENDPOINT 1 — GET /api/potreros
        // ------------------------------------------------------------
        [HttpGet]
        public ActionResult<List<PotreroResponse>> ObtenerTodos()
        {
            List<Potrero> potreros = servicioPotreros.ObtenerTodosLosPotreros();
            return Ok(potreros.Select(AConPotreroResponse).ToList());
        }

        // ------------------------------------------------------------
        // ENDPOINT 2 — GET /api/potreros/{id}
        // ------------------------------------------------------------
       
        [HttpGet("{id}")]
        public ActionResult<PotreroResponse> ObtenerPorId(string id)
        {
            Potrero? potrero = servicioPotreros.ObtenerPotreroPorIdentificacion(id);

            if (potrero == null)
            {
                return NotFound(new { mensaje = $"No se encontró el potrero '{id}'." });
            }

            return Ok(AConPotreroResponse(potrero));
        }

        // ------------------------------------------------------------
        // ENDPOINT 3 — POST /api/potreros
        // ------------------------------------------------------------
        [HttpPost]
        public ActionResult<ResultadoOperacion> Crear([FromBody] PotreroRequest request)
        {
            ResultadoOperacion resultado = servicioPotreros.CrearPotrero(request.Identificacion, request.TipoPotrero);

            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            return CreatedAtAction(nameof(ObtenerPorId), new { id = request.Identificacion }, resultado);
        }

        // ------------------------------------------------------------
        // ENDPOINT 4 — GET /api/potreros/{potreroId}/reses
        // ------------------------------------------------------------
       
        [HttpGet("{potreroId}/reses")]
        public ActionResult<List<ResResponse>> ObtenerResesDePotrero(string potreroId)
        {
            Potrero? potrero = servicioPotreros.ObtenerPotreroPorIdentificacion(potreroId);

            if (potrero == null)
            {
                return NotFound(new { mensaje = $"No se encontró el potrero '{potreroId}'." });
            }

            return Ok(potrero.L_reses.Select(AConResResponse).ToList());
        }

        // ------------------------------------------------------------
        // ENDPOINT 5 — POST /api/potreros/{potreroId}/reses
        // ------------------------------------------------------------

        [HttpPost("{potreroId}/reses")]
        public ActionResult<ResultadoOperacion> AgregarRes(string potreroId, [FromBody] ResRequest request)
        {
            Potrero? potrero = servicioPotreros.ObtenerPotreroPorIdentificacion(potreroId);

            if (potrero == null)
            {
                return NotFound(new { mensaje = $"No se encontró el potrero '{potreroId}'." });
            }

            ResultadoOperacion resultado = servicioPotreros.AnadirResAPotrero(potreroId, request.Nombre, request.Edad, request.Peso);

            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            return CreatedAtAction(nameof(ObtenerRes), new { potreroId, nombre = request.Nombre }, resultado);
        }

        // ------------------------------------------------------------
        // ENDPOINT 6 — GET /api/potreros/{potreroId}/reses/{nombre}
        // ------------------------------------------------------------
        //
        
        [HttpGet("{potreroId}/reses/{nombre}")]
        public ActionResult<ResResponse> ObtenerRes(string potreroId, string nombre)
        {
            Res? res = servicioPotreros.BuscarRes(potreroId, nombre);

            if (res == null)
            {
                return NotFound(new { mensaje = $"No se encontró la res '{nombre}' en el potrero '{potreroId}'." });
            }

            return Ok(AConResResponse(res));
        }

        // ------------------------------------------------------------
        // Mapeo a DTO (no es lógica de negocio: solo traslada datos ya
        // calculados por el dominio/servicio a la forma de salida HTTP).
        // ------------------------------------------------------------
        private static PotreroResponse AConPotreroResponse(Potrero potrero)
        {
            return new PotreroResponse
            {
                Identificacion = potrero.Identificacion,
                TipoPotrero = potrero.Tipo_potrero,
                CantidadReses = potrero.L_reses.Count
            };
        }

        private static ResResponse AConResResponse(Res res)
        {
            return new ResResponse
            {
                Nombre = res.Nombre,
                Peso = res.Peso,
                Edad = res.Edad,
                Tipo = res.GetType().Name
            };
        }
    }
}
