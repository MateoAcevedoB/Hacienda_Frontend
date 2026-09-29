using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bib_Hacienda.Aplicacion;
using static Bib_Hacienda.Dominio.Viva;
using System.Globalization;

namespace p_mvcHacienda.Controllers
{
    public class VacunaController : Controller
    {
        private readonly ServicioInventarioVacunas _servicioInventarioVacunas;
        private readonly ServicioVacunacion _servicioVacunacion;
        private readonly ServicioPotreros _servicioPotreros;

        public VacunaController(ServicioInventarioVacunas servicioInventarioVacunas, ServicioVacunacion servicioVacunacion, ServicioPotreros servicioPotreros)
        {
            _servicioInventarioVacunas = servicioInventarioVacunas;
            _servicioVacunacion = servicioVacunacion;
            _servicioPotreros = servicioPotreros;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var vacunas = _servicioInventarioVacunas.ObtenerVacunasDisponibles();
            var estadisticas = _servicioInventarioVacunas.ObtenerEstadisticas();

            ViewBag.Estadisticas = estadisticas;

            return View(vacunas);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Aplicar()
        {
            ViewBag.Potreros = _servicioPotreros.ObtenerTodosLosPotreros();
            ViewBag.Reses = _servicioPotreros.ObtenerTodasLasReses();
            ViewBag.Vacunas = _servicioInventarioVacunas.ObtenerVacunasDisponibles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string tipoVacuna, string nombre, string lote,
            string fechaVencimiento, string fechaAplicacion,    
            uint? periodoAplicacion, enum_l_atenuaciones? atenuacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(lote))
                {
                    ViewBag.Mensaje = "El nombre y lote son requeridos";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                if (!DateTime.TryParseExact(fechaVencimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaVenc))
                {
                    ViewBag.Mensaje = "Fecha de vencimiento inválida";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
                if (!DateTime.TryParseExact(fechaAplicacion, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaAplic))
                {
                    ViewBag.Mensaje = "Fecha de aplicación inválida";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                if (fechaAplic > fechaVenc)
                {
                    ViewBag.Mensaje = "La fecha de aplicación no puede ser posterior a la fecha de vencimiento";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                if (tipoVacuna == "Bacteriana" && !periodoAplicacion.HasValue)
                {
                    ViewBag.Mensaje = "El período de aplicación es requerido para vacunas bacterianas";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                if (tipoVacuna != "Bacteriana" && !atenuacion.HasValue)
                {
                    ViewBag.Mensaje = "La atenuación es requerida para vacunas vivas";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                var resultado = _servicioInventarioVacunas.CrearVacuna(nombre, lote, fechaVenc, fechaAplic, periodoAplicacion, atenuacion);

                if (resultado.Exito)
                {
                    TempData["Mensaje"] = resultado.Mensaje;
                    TempData["TipoMensaje"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Mensaje = resultado.Mensaje;
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $" Error: {ex.Message}";
                ViewBag.TipoMensaje = "danger";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aplicar(string potreroId, string nombreRes, string loteVacuna)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(potreroId) || string.IsNullOrWhiteSpace(nombreRes) || string.IsNullOrWhiteSpace(loteVacuna))
                {
                    ViewBag.Mensaje = " Todos los campos son requeridos";
                    ViewBag.TipoMensaje = "danger";
                    ViewBag.Potreros = _servicioPotreros.ObtenerTodosLosPotreros();
                    ViewBag.Reses = _servicioPotreros.ObtenerTodasLasReses();
                    ViewBag.Vacunas = _servicioInventarioVacunas.ObtenerVacunasDisponibles();
                    return View();
                }

                var vacuna = _servicioInventarioVacunas.ObtenerVacunasDisponibles()
                    .FirstOrDefault(v => v.Lote == loteVacuna);

                if (vacuna == null)
                {
                    TempData["Mensaje"] = $"No se encontró una vacuna con el lote '{loteVacuna}'";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction(nameof(Index));
                }

                var resultado = _servicioVacunacion.aplicar_vacuna(vacuna, nombreRes, potreroId);

                TempData["Mensaje"] = resultado.Mensaje;
                TempData["TipoMensaje"] = resultado.Exito ? "success" : "danger";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $" Error: {ex.Message}";
                ViewBag.TipoMensaje = "danger";
                ViewBag.Potreros = _servicioPotreros.ObtenerTodosLosPotreros();
                ViewBag.Reses = _servicioPotreros.ObtenerTodasLasReses();
                ViewBag.Vacunas = _servicioInventarioVacunas.ObtenerVacunasDisponibles();
                return View();
            }
        }
    }
}
