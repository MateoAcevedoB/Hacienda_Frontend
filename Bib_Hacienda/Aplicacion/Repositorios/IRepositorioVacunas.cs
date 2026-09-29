using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public interface IRepositorioVacunas
    {
        List<Vacuna> ObtenerDisponibles();
        void Agregar(Vacuna vacuna);
        void Remover(Vacuna vacuna);

        
        void MarcarComoAplicada(Vacuna vacuna, Res res);
    }
}
