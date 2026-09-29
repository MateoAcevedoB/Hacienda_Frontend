using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public class ValidadorPotrero : IValidadorPotrero
    {
        public bool EsValido(Potrero potrero)
        {
            if (potrero == null || string.IsNullOrWhiteSpace(potrero.Identificacion))
            {
                return false;
            }
            return true;
        }
    }
}
