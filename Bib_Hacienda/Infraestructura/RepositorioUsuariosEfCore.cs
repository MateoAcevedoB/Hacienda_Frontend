using System;
using System.Collections.Generic;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Bib_Hacienda.Infraestructura
{
    // FASE 2B: implementación EF Core / PostgreSQL de IRepositorioUsuarios.
    //
    // Comportamiento equivalente al de RepositorioUsuariosTexto: las mismas
    // 3 operaciones del contrato (ObtenerTodos, BuscarPorNombre, Agregar).
    // Igual que el repositorio de texto (y que el UML, ver comentario de
    // RepositorioUsuariosTexto), esta clase NO recibe ningún IValidador*:
    // la validación de nombre/contraseña no vacíos ya la hace
    // ServicioAutenticacion.crear_usuario antes de llamar a Agregar.
    //
    // Usuario.ContrasenaHash: este repositorio únicamente persiste y
    // recupera el hash que ya llega calculado desde IServicioHash (inyectado
    // en ServicioAutenticacion, fuera de este bloque); no calcula, verifica
    // ni transforma contraseñas de ninguna forma.
    public class RepositorioUsuariosEfCore : IRepositorioUsuarios
    {
        private readonly HaciendaDbContext contexto;

        public RepositorioUsuariosEfCore(HaciendaDbContext contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public List<Usuario> ObtenerTodos()
        {
            try
            {
                // AsNoTracking: ServicioAutenticacion solo lee este resultado
                // (listar_usuarios, ValidarCredenciales, ObtenerEstadisticas,
                // comprobación de nombre duplicado en crear_usuario); nunca lo
                // vuelve a guardar a través de este repositorio.
                return contexto.Usuarios.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                // Misma degradación que RepositorioUsuariosTexto.ObtenerTodos
                // (y que PersistenciaService.CargarUsuarios en el AS-IS): un
                // fallo al leer usuarios no debe interrumpir la aplicación
                // (p. ej. al iniciar sesión); se registra y se devuelve una
                // lista vacía en vez de propagar la excepción.
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public Usuario BuscarPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            // Comparación case-insensitive: mismo comportamiento que
            // RepositorioUsuariosTexto (StringComparison.OrdinalIgnoreCase).
            string nombreNormalizado = nombre.ToLower();

            return contexto.Usuarios
                .AsNoTracking()
                .FirstOrDefault(u => u.Nombre.ToLower() == nombreNormalizado);
        }

        public void Agregar(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
        }
    }
}
