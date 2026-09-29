using System;
using System.Globalization;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    public class SerializadorRes : ISerializador<Res>
    {
        public string Serializar(Res entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            string tipo = entidad.GetType().Name;
            return $"{entidad.Nombre}|{entidad.Peso}|{entidad.Edad}|{tipo}";
        }

        public Res Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de res a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 4)
            {
                throw new FormatException($"Línea de res con formato inválido: '{linea}'");
            }

            string nombre = partes[0];
            uint peso = uint.Parse(partes[1], CultureInfo.InvariantCulture);
            ushort edad = ushort.Parse(partes[2], CultureInfo.InvariantCulture);
            string tipo = partes[3].Trim();

            return tipo switch
            {
                "Ternero" => new Ternero(nombre, peso, edad),
                "Novillo" => new Novillo(nombre, peso, edad),
                "Cebon" => new Cebon(nombre, peso, edad),
                _ => new Ternero(nombre, peso, edad)
            };
        }
    }
}
