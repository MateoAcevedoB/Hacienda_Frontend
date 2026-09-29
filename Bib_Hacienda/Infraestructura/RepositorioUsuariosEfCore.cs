using System;
using System.Collections.Generic;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Bib_Hacienda.Infraestructura
{
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
                return contexto.Usuarios.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
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
