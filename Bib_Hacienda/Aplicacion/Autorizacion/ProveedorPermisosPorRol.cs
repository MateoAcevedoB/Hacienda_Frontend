namespace Bib_Hacienda.Aplicacion
{
    
    public class ProveedorPermisosPorRol : IProveedorPermisos
    {
        public bool TienePermiso(string rol, string operacion)
        {
            if (rol == "admin")
            {
                //Admin tiene todos los permisos
                return true;
            }
            else if (rol == "empleado")
            {
                //Empleado puede hacer todo excepto eliminar usuarios
                return !operacion.Contains("Eliminar");
            }
            else if (rol == "visitante")
            {
                //Visitante solo puede consultar
                return operacion.Contains("Consultar") || operacion.Contains("Listar");
            }

            //Rol desconocido: sin permisos (comportamiento no cubierto por el
            //AS-IS, que solo contemplaba estos 3 roles; se deniega por defecto).
            return false;
        }
    }
}
