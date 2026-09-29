using Api_Hacienda.Models;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static Bib_Hacienda.Dominio.Viva;

namespace Api_Hacienda.Controllers
{
    
    [ApiController]
    [Route("api/vacunas")]
    [Authorize]
    public class VacunasController : ControllerBase
    {
        private readonly ServicioInventarioVacunas servicioInventarioVacunas;

        public VacunasController(ServicioInventarioVacunas servicioInventarioVacunas)
        {
            this.servicioInventarioVacunas = servicioInventarioVacunas;
        }

        
        [HttpGet]
        public ActionResult<List<VacunaResponse>> ObtenerDisponibles()
        {
            List<Vacuna> vacunas = servicioInventarioVacunas.ObtenerVacunasDisponibles();
            return Ok(vacunas.Select(AConVacunaResponse).ToList());
        }

        
        [HttpPost]
        public ActionResult<ResultadoOperacion> Crear([FromBody] VacunaRequest request)
        {
            
            if (request.AtenuacionGrado.HasValue && !Enum.IsDefined(typeof(enum_l_atenuaciones), request.AtenuacionGrado.Value))
            {
                return BadRequest(new { mensaje = $"El grado de atenuación {request.AtenuacionGrado.Value} no es válido. Valores permitidos: 10, 20, 30." });
            }

            enum_l_atenuaciones? atenuacion = request.AtenuacionGrado.HasValue
                ? (enum_l_atenuaciones)request.AtenuacionGrado.Value
                : (enum_l_atenuaciones?)null;

            ResultadoOperacion resultado = servicioInventarioVacunas.CrearVacuna(
                request.Nombre,
                request.Lote,
                request.FechaVencimiento,
                request.FechaAplicacion,
                request.PeriodoAplicacion,
                atenuacion);

            if (!resultado.Exito)
            {
                
                return BadRequest(resultado);
            }

            return StatusCode(201, resultado);
        }

        private static VacunaResponse AConVacunaResponse(Vacuna vacuna)
        {
            return new VacunaResponse
            {
                Nombre = vacuna.Nombre,
                Lote = vacuna.Lote,
                FechaVencimiento = vacuna.Fecha_vencimiento,
                FechaAplicacion = vacuna.Fecha_aplicacion,
                Tipo = vacuna.GetType().Name,
                
                PeriodoAplicacion = vacuna is Bacteriana bacteriana ? bacteriana.Periodo_aplicacion : (uint?)null
            };
        }
    }
}
