using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bib_Hacienda.Aplicacion;

namespace p_mvcHacienda.Controllers
{
    public class VentaController : Controller
    {
        private readonly ServicioVentas _servicioVentas;

        public VentaController(ServicioVentas servicioVentas)
        {
            _servicioVentas = servicioVentas;
        }

        public ActionResult Index()
        {
            var ventas = _servicioVentas.ObtenerTodasLasVentas();
            var estadisticas = _servicioVentas.ObtenerEstadisticas();

            ViewBag.Estadisticas = estadisticas;

            return View(ventas);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
