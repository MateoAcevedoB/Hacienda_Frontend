using System;
using System.Globalization;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Infraestructura
{
    public class SerializadorVacuna : ISerializador<Vacuna>
    {
        public string Serializar(Vacuna entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            string fechaVenc = entidad.Fecha_vencimiento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string fechaAplic = entidad.Fecha_aplicacion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string tipo = entidad.GetType().Name;
            uint periodo = entidad is Bacteriana bacteriana ? bacteriana.Periodo_aplicacion : 0;

            return $"{entidad.Nombre}|{entidad.Lote}|{fechaVenc}|{fechaAplic}|{tipo}|{periodo}";
        }

        public Vacuna Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de vacuna a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 6)
            {
                throw new FormatException($"Línea de vacuna con formato inválido: '{linea}'");
            }

            string nombre = partes[0];
            string lote = partes[1];

            if (!DateTime.TryParseExact(partes[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaVenc))
            {
                throw new FormatException($"Fecha de vencimiento con formato inválido: '{partes[2]}'");
            }

            if (!DateTime.TryParseExact(partes[3].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaAplic))
            {
                throw new FormatException($"Fecha de aplicación con formato inválido: '{partes[3]}'");
            }

            string tipo = partes[4].Trim();
            uint periodo = uint.TryParse(partes[5].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var per) ? per : 0u;

            if (tipo.Equals("Bacteriana", StringComparison.OrdinalIgnoreCase))
            {
                return new Bacteriana(nombre, lote, fechaVenc, fechaAplic, periodo);
            }

            return new Viva(nombre, lote, fechaVenc, fechaAplic, Viva.enum_l_atenuaciones.Atenuacion10);
        }
    }
}
