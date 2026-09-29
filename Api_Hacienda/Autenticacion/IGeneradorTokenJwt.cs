using Bib_Hacienda.Dominio;

namespace Api_Hacienda.Autenticacion
{
    
    public interface IGeneradorTokenJwt
    {
        // "rol" recibe el mismo valor que ServicioAutenticacion.AutorizarOperacion
        // pasa hoy a IProveedorPermisos.TienePermiso (usuario.Nombre, ver
        // ProveedorPermisosPorRol.cs): el proyecto no modela un Rol separado
        // del Nombre de usuario, así que no se inventa uno nuevo aquí.
        TokenJwtGenerado GenerarToken(Usuario usuario, string rol);
    }

    
    public record TokenJwtGenerado(string Token, DateTime ExpiraEn);
}
