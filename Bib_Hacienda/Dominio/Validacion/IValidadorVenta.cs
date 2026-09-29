using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public interface IValidadorVenta
    {
        bool EsValido(Venta venta);
    }
}
