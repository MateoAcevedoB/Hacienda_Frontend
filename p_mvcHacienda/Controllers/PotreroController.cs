using Microsoft.AspNetCore.Mvc;
using Bib_Hacienda.Aplicacion;

namespace p_mvcHacienda.Controllers
{
    public class PotreroController : Controller
    {
        private readonly ServicioPotreros _servicioPotreros;

        public PotreroController(ServicioPotreros servicioPotreros)
        {
            _servicioPotreros = servicioPotreros;
        }

        [HttpGet]

        public ActionResult Index()
        {
            var potreros = _servicioPotreros.ObtenerTodosLosPotreros();
            var estadisticas = _servicioPotreros.ObtenerEstadisticas();
      
            ViewBag.Estadisticas = estadisticas;

            return View(potreros);
        }

        
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Details(string id)
        {
            var potrero = _servicioPotreros.ObtenerPotreroPorIdentificacion(id);

            if (potrero == null)
            {
                TempData["Mensaje"] = "Potrero no encontrado";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            return View(potrero);
        }

        [HttpPost]

        public ActionResult Create(string identificacion, string tipo)
        {
            try
            { 
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    ViewBag.Mensaje = "La identificación no puede estar vacía";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                var resultado = _servicioPotreros.CrearPotrero(identificacion, tipo);

                if (!resultado.Exito)
                {
                    ViewBag.Mensaje = resultado.Mensaje;
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }

                TempData["Mensaje"] = resultado.Mensaje;
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"{ex.Message}";
                ViewBag.TipoMensaje = "danger";
            }
  
            return View();
        }
    }
}
