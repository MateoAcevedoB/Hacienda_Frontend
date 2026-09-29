using Bib_Hacienda.Dominio;
using System;
using System.Collections.Generic;

namespace Bib_Hacienda.Interfaces
{
    public interface IHistoriaClinica
    {
        ResultadoOperacion RegistrarEvento(string potreroId, string nombreRes, DateTime fecha, string tipoEvento, string descripcion);

        List<EventoClinico> ConsultarHistoria(string potreroId, string nombreRes);
    }
}
