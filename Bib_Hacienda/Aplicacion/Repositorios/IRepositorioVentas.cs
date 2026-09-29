using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public interface IRepositorioVentas
    {
        List<Venta> ObtenerTodas();
        void Agregar(Venta venta);
    }
}
