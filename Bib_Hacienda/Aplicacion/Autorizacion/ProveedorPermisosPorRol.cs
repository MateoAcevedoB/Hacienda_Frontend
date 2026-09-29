namespace Bib_Hacienda.Aplicacion
{
    
    public class ProveedorPermisosPorRol : IProveedorPermisos
    {
        public bool TienePermiso(string rol, string operacion)
        {
            if (rol == "admin")
            {
                return true;
            }
            else if (rol == "empleado")
            {
                return !operacion.Contains("Eliminar");
            }
            else if (rol == "visitante")
            {
                return operacion.Contains("Consultar") || operacion.Contains("Listar");
            }

            return false;
        }
    }
}
