using Bib_Hacienda.Dominio;

namespace Api_Hacienda.Autenticacion
{
    
    public interface IGeneradorTokenJwt
    {
        TokenJwtGenerado GenerarToken(Usuario usuario, string rol);
    }

    
    public record TokenJwtGenerado(string Token, DateTime ExpiraEn);
}
