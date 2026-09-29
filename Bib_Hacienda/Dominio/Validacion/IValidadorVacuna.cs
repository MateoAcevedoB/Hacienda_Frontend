using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public interface IValidadorVacuna
    {
        bool EsValido(Vacuna vacuna);
    }
}
