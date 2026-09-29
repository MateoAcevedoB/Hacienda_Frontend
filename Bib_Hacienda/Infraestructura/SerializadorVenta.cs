using System;
using System.Globalization;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;

namespace Bib_Hacienda.Infraestructura
{
    public class SerializadorVenta : ISerializador<Venta>
    {
        public string Serializar(Venta entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad));
            }

            if (!(entidad.Activo is Res res))
            {
                throw new NotSupportedException(
                    "SerializadorVenta solo admite ventas cuyo Activo sea una Res (Ternero/Novillo/Cebon); " +
                    "otros IActivoVendible quedan fuera de este bloque (SC-1).");
            }

            string fecha = entidad.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string tipoRes = res.GetType().Name;

            return $"{entidad.Potrero.Identificacion}|{fecha}|{res.Nombre}|{res.Peso}|{res.Edad}|{tipoRes}|{entidad.Monto}";
        }

        public Venta Deserializar(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                throw new ArgumentException("La línea de venta a deserializar no puede estar vacía.", nameof(linea));
            }

            var partes = linea.Split('|');
            if (partes.Length < 7)
            {
                throw new FormatException($"Línea de venta con formato inválido: '{linea}'");
            }

            string potreroId = partes[0].Trim();

            if (!DateTime.TryParseExact(partes[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                throw new FormatException($"Fecha de venta con formato inválido: '{partes[1]}'");
            }

            string nombreRes = partes[2];
            uint peso = uint.Parse(partes[3], CultureInfo.InvariantCulture);
            ushort edad = ushort.Parse(partes[4], CultureInfo.InvariantCulture);
            string tipoRes = partes[5].Trim();
            uint monto = uint.Parse(partes[6], CultureInfo.InvariantCulture);

            Res res = tipoRes switch
            {
                "Ternero" => new Ternero(nombreRes, peso, edad),
                "Novillo" => new Novillo(nombreRes, peso, edad),
                "Cebon" => new Cebon(nombreRes, peso, edad),
                _ => new Ternero(nombreRes, peso, edad)
            };

            var potrero = new Potrero(potreroId, string.Empty, new PublisherPotreroMitad(), new PublisherPotreroLleno());

            return new Venta(potrero, fecha, res, monto);
        }
    }
}
