using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    public class RepositorioUsuariosTexto : IRepositorioUsuarios
    {
        private const string ArchivoUsuarios = "Usuarios.txt";

        private readonly ISerializador<Usuario> serializadorUsuario;

        public RepositorioUsuariosTexto(ISerializador<Usuario> serializadorUsuario)
        {
            this.serializadorUsuario = serializadorUsuario;
        }

        public List<Usuario> ObtenerTodos()
        {
            try
            {
                string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoUsuarios);
                var usuarios = new List<Usuario>();

                if (!File.Exists(ruta))
                {
                    return usuarios;
                }

                foreach (var linea in File.ReadAllLines(ruta))
                {
                    if (string.IsNullOrWhiteSpace(linea))
                    {
                        continue;
                    }

                    usuarios.Add(serializadorUsuario.Deserializar(linea));
                }

                return usuarios;
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

            return ObtenerTodos()
                .FirstOrDefault(u => string.Equals(u.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
        }

        public void Agregar(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoUsuarios);
            File.AppendAllLines(ruta, new[] { serializadorUsuario.Serializar(usuario) });
        }
    }
}
