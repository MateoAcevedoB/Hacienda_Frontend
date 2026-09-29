using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public class ValidadorVacuna : IValidadorVacuna
    {
        public bool EsValido(Vacuna vacuna)
        {
            if (vacuna == null || string.IsNullOrWhiteSpace(vacuna.Nombre) || string.IsNullOrWhiteSpace(vacuna.Lote))
            {
                return false;
            }
            return true;
        }
    }
}
