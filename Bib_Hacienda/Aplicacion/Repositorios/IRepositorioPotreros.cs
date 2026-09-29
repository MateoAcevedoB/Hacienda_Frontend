using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public interface IRepositorioPotreros
    {
        List<Potrero> ObtenerTodos();
        Potrero ObtenerPorId(string identificacion);
        void Agregar(Potrero potrero);
        bool Existe(string identificacion);

        void Actualizar(Potrero potrero);
    }
}
