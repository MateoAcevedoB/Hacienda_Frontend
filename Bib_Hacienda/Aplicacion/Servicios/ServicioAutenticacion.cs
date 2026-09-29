using Bib_Hacienda.Dominio;
using Bib_Hacienda.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib_Hacienda.Aplicacion
{
   
    
    public class ServicioAutenticacion : IAutenticacion
    {
        private IRepositorioUsuarios repositorioUsuarios;
        private IServicioHash servicioHash;
        private IProveedorPermisos proveedorPermisos;

        public ServicioAutenticacion(IRepositorioUsuarios repositorioUsuarios, IServicioHash servicioHash, IProveedorPermisos proveedorPermisos)
        {
            this.repositorioUsuarios = repositorioUsuarios;
            this.servicioHash = servicioHash;
            this.proveedorPermisos = proveedorPermisos;
        }

        public ResultadoOperacion crear_usuario(string nombre, string contrasena)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return ResultadoOperacion.Fallo("El nombre del usuario no puede estar vacío");
                }

                if (string.IsNullOrWhiteSpace(contrasena))
                {
                    return ResultadoOperacion.Fallo("La contraseña no puede estar vacía");
                }

                if (repositorioUsuarios.ObtenerTodos().Any(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResultadoOperacion.Fallo($"Ya existe un usuario con el nombre '{nombre}'.");
                }

                string contrasenaHash = servicioHash.Hash(contrasena);
                Usuario nuevo_usuario = new Usuario(nombre, contrasenaHash);
                repositorioUsuarios.Agregar(nuevo_usuario);

                return ResultadoOperacion.Ok($"Usuario '{nombre}' creado exitosamente en el sistema.");
            }
            catch (Exception er)
            {
                return ResultadoOperacion.Fallo("Error inesperado en el método crear_usuario: " + er.Message);
            }
        }

        public List<Usuario> listar_usuarios()
        {
            return new List<Usuario>(repositorioUsuarios.ObtenerTodos());
        }

        public Dictionary<string, object> ObtenerEstadisticas()
        {
            return new Dictionary<string, object>
            {
                { "TotalUsuarios", repositorioUsuarios.ObtenerTodos().Count }
            };
        }

        public bool ValidarCredenciales(string nombre, string contrasena)
        {
            Usuario usuario = repositorioUsuarios.ObtenerTodos()
                .FirstOrDefault(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                return false;
            }

            if (servicioHash.Verificar(contrasena, usuario.ContrasenaHash))
            {
                return true;
            }

            return string.Equals(contrasena, usuario.ContrasenaHash, StringComparison.Ordinal);
        }

        public Usuario buscar_usuario(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new ArgumentException("El nombre de búsqueda no puede estar vacío.");
                }

                Usuario usuario = repositorioUsuarios.BuscarPorNombre(nombre);

                if (usuario == null)
                {
                    throw new Exception($"No se encontró el usuario '{nombre}'.");
                }

                return usuario;
            }
            catch (Exception er)
            {
                throw new Exception("Error inesperado en el método buscar_usuario: " + er.Message);
            }
        }

        public ResultadoOperacion AutorizarOperacion(Usuario usuario, string operacion)
        {
            if (usuario == null)
            {
                return ResultadoOperacion.Fallo("✗ Usuario no autenticado. Debe iniciar sesión para realizar operaciones");
            }

            Usuario usuarioRegistrado = repositorioUsuarios.BuscarPorNombre(usuario.Nombre);

            if (usuarioRegistrado == null)
            {
                return ResultadoOperacion.Fallo($"✗ Usuario '{usuario.Nombre}' no está registrado en el sistema");
            }

            bool tienePermiso = proveedorPermisos.TienePermiso(usuario.Nombre, operacion);

            if (tienePermiso)
            {
                return ResultadoOperacion.Ok($"✓ Usuario '{usuario.Nombre}' autorizado para ejecutar: {operacion}");
            }
            else
            {
                return ResultadoOperacion.Fallo($"✗ Acceso DENEGADO. Usuario '{usuario.Nombre}' NO tiene permisos para: {operacion}");
            }
        }
    }
}
