using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public interface IRepositorioHistoriaClinica
    {
        List<EventoClinico> ObtenerPorRes(string potreroId, string nombreRes);

        void Agregar(EventoClinico evento);
    }
}
