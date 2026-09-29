namespace Api_Hacienda.Models
{
    
    public class UsuarioResponse
    {
        public string Nombre { get; set; } = string.Empty;

        public static UsuarioResponse DesdeUsuario(Bib_Hacienda.Dominio.Usuario usuario)
        {
            return new UsuarioResponse { Nombre = usuario.Nombre };
        }
    }
}
