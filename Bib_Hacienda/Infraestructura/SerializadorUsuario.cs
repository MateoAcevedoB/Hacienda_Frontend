using System;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    public class SerializadorUsuario : ISerializador<Usuario>
    {
        public string Serializar(Usuario entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            return $"{entidad.Nombre}|{entidad.ContrasenaHash}";
        }

        public Usuario Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de usuario a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 2)
            {
                throw new FormatException($"Línea de usuario con formato inválido: '{linea}'");
            }

            string nombre = partes[0];
            string contrasenaHash = partes[1];

            return new Usuario(nombre, contrasenaHash);
        }
    }
}
