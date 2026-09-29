using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Validacion
{
    public class ValidadorRes : IValidadorRes
    {
        public bool EsValido(Res res)
        {
            if (res == null || string.IsNullOrWhiteSpace(res.Nombre) || res.Peso <= 0 || res.Edad <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
