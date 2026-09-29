using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_Hacienda.Controllers
{
    
    [ApiController]
    [Route("api")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly ServicioVentas servicioVentas;
        private readonly ServicioPotreros servicioPotreros;

        public VentasController(ServicioVentas servicioVentas, ServicioPotreros servicioPotreros)
        {
            this.servicioVentas = servicioVentas;
            this.servicioPotreros = servicioPotreros;
        }

        // ------------------------------------------------------------
        // GET /api/ventas
        // GET /api/ventas?potreroId=...
        // GET /api/ventas?desde=...&hasta=...
        // ------------------------------------------------------------
        
        [HttpGet("ventas")]
        public ActionResult<List<VentaResponse>> ObtenerVentas([FromQuery] string? potreroId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            bool tieneFiltroPotrero = !string.IsNullOrWhiteSpace(potreroId);
            bool tieneFiltroFechas = desde.HasValue || hasta.HasValue;

            if (tieneFiltroPotrero && tieneFiltroFechas)
            {
                return BadRequest(new { mensaje = "No se soporta combinar el filtro potreroId con el filtro de fechas: ServicioVentas no expone un método que combine ambos criterios." });
            }

            if (tieneFiltroFechas && (!desde.HasValue || !hasta.HasValue))
            {
                return BadRequest(new { mensaje = "Para filtrar por fechas se requieren ambos parámetros: 'desde' y 'hasta'." });
            }

            List<Venta> ventas;

            if (tieneFiltroPotrero)
            {
                ventas = servicioVentas.ObtenerVentasPorPotrero(potreroId!);
            }
            else if (tieneFiltroFechas)
            {
                ventas = servicioVentas.ObtenerVentasPorFechas(desde!.Value, hasta!.Value);
            }
            else
            {
                ventas = servicioVentas.ObtenerTodasLasVentas();
            }

            return Ok(ventas.Select(AConVentaResponse).ToList());
        }

        // ------------------------------------------------------------
        // POST /api/potreros/{potreroId}/reses/{nombre}/vender
        // ------------------------------------------------------------
        
        [HttpPost("potreros/{potreroId}/reses/{nombre}/vender")]
        public ActionResult<ResultadoOperacion> Vender(string potreroId, string nombre, [FromBody] VenderResRequest request)
        {
            Potrero? potrero = servicioPotreros.ObtenerPotreroPorIdentificacion(potreroId);
            if (potrero == null)
            {
                return NotFound(new { mensaje = $"No se encontró el potrero '{potreroId}'." });
            }

            Res? res = servicioPotreros.BuscarRes(potreroId, nombre);
            if (res == null)
            {
                return NotFound(new { mensaje = $"No se encontró la res '{nombre}' en el potrero '{potreroId}'." });
            }

            ResultadoOperacion resultado = servicioVentas.vender_res(potreroId, nombre, request.Monto);

            if (!resultado.Exito)
            {
                
                return BadRequest(resultado);
            }

            return StatusCode(201, resultado);
        }

        private static VentaResponse AConVentaResponse(Venta venta)
        {
            return new VentaResponse
            {
                PotreroId = venta.Potrero?.Identificacion ?? string.Empty,
               
                NombreRes = venta.Activo?.Identificador ?? string.Empty,
                Fecha = venta.Fecha,
                Monto = venta.Monto
            };
        }
    }
}
