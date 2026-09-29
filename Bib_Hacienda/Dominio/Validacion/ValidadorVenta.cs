using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public class ValidadorVenta : IValidadorVenta
    {
        public bool EsValido(Venta venta)
        {
            if (venta == null || venta.Potrero == null || venta.Activo == null || venta.Monto <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
