using System;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;

namespace Bib_Hacienda.Infraestructura
{
    public class SerializadorPotrero : ISerializador<Potrero>
    {
        public string Serializar(Potrero entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            return $"{entidad.Identificacion}|{entidad.Tipo_potrero}";
        }

        public Potrero Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de potrero a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 2)
            {
                throw new FormatException($"Línea de potrero con formato inválido: '{linea}'");
            }

            string identificacion = partes[0].Trim();
            string tipoPotrero = partes[1].Trim();

            return new Potrero(identificacion, tipoPotrero, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }
    }
}
