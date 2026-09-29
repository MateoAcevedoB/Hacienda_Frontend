using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Validacion;

namespace Bib_Hacienda.Infraestructura
{
    public class RepositorioPotrerosTexto : IRepositorioPotreros
    {
        private const string ArchivoPotreros = "Potreros.txt";
        private const string ArchivoReses = "Reses.txt";

        private readonly ISerializador<Potrero> serializadorPotrero;
        private readonly ISerializador<Res> serializadorRes;
        private readonly IValidadorPotrero validadorPotrero;
        private readonly IValidadorRes validadorRes;

        public RepositorioPotrerosTexto(ISerializador<Potrero> serializadorPotrero, ISerializador<Res> serializadorRes, IValidadorPotrero validadorPotrero, IValidadorRes validadorRes)
        {
            this.serializadorPotrero = serializadorPotrero;
            this.serializadorRes = serializadorRes;
            this.validadorPotrero = validadorPotrero;
            this.validadorRes = validadorRes;
        }

        public List<Potrero> ObtenerTodos()
        {
            try
            {
                var potreros = CargarPotrerosDesdeArchivo();
                CargarResesDentroDePotreros(potreros);
                return potreros;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar potreros: {ex.Message}", ex);
            }
        }

        public Potrero ObtenerPorId(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return null;
            }

            return ObtenerTodos()
                .FirstOrDefault(p => string.Equals(p.Identificacion, identificacion, StringComparison.OrdinalIgnoreCase));
        }

        public bool Existe(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return false;
            }

            return ObtenerTodos()
                .Any(p => string.Equals(p.Identificacion, identificacion, StringComparison.OrdinalIgnoreCase));
        }

        public void Agregar(Potrero potrero)
        {
            if (potrero == null)
            {
                throw new ArgumentNullException(nameof(potrero));
            }

            if (!validadorPotrero.EsValido(potrero))
            {
                throw new Exception("Error de validación en potrero");
            }

            foreach (var res in potrero.L_reses)
            {
                if (!validadorRes.EsValido(res))
                {
                    throw new Exception("Error de validación en res");
                }
            }

            string rutaPotreros = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoPotreros);
            File.AppendAllLines(rutaPotreros, new[] { serializadorPotrero.Serializar(potrero) });

            if (potrero.L_reses.Count > 0)
            {
                string rutaReses = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);
                var lineasReses = potrero.L_reses
                    .Select(res => $"{potrero.Identificacion}|{serializadorRes.Serializar(res)}");
                File.AppendAllLines(rutaReses, lineasReses);
            }
        }

        public void Actualizar(Potrero potrero)
        {
            if (potrero == null)
            {
                throw new ArgumentNullException(nameof(potrero));
            }

            if (!validadorPotrero.EsValido(potrero))
            {
                throw new Exception("Error de validación en potrero");
            }

            foreach (var res in potrero.L_reses)
            {
                if (!validadorRes.EsValido(res))
                {
                    throw new Exception("Error de validación en res");
                }
            }

            string rutaReses = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);

            var lineasDeOtrosPotreros = new List<string>();
            if (File.Exists(rutaReses))
            {
                foreach (var linea in File.ReadAllLines(rutaReses))
                {
                    if (string.IsNullOrWhiteSpace(linea))
                    {
                        continue;
                    }

                    int separador = linea.IndexOf('|');
                    string potreroIdDeLaLinea = separador >= 0 ? linea.Substring(0, separador).Trim() : null;

                    if (!string.Equals(potreroIdDeLaLinea, potrero.Identificacion, StringComparison.OrdinalIgnoreCase))
                    {
                        lineasDeOtrosPotreros.Add(linea);
                    }
                }
            }

            var lineasDelPotrero = potrero.L_reses
                .Select(res => $"{potrero.Identificacion}|{serializadorRes.Serializar(res)}");

            File.WriteAllLines(rutaReses, lineasDeOtrosPotreros.Concat(lineasDelPotrero));
        }

        private List<Potrero> CargarPotrerosDesdeArchivo()
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoPotreros);
            var potreros = new List<Potrero>();

            if (!File.Exists(ruta))
            {
                return potreros;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                var potrero = serializadorPotrero.Deserializar(linea);

                if (!potreros.Any(p => string.Equals(p.Identificacion, potrero.Identificacion, StringComparison.OrdinalIgnoreCase)))
                {
                    potreros.Add(potrero);
                }
            }

            return potreros;
        }

        private void CargarResesDentroDePotreros(List<Potrero> potreros)
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoReses);

            if (!File.Exists(ruta))
            {
                return;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                int separador = linea.IndexOf('|');
                if (separador < 0)
                {
                    continue;
                }

                string potreroId = linea.Substring(0, separador).Trim();
                string lineaRes = linea.Substring(separador + 1);

                var potrero = potreros.FirstOrDefault(p => string.Equals(p.Identificacion, potreroId, StringComparison.OrdinalIgnoreCase));
                if (potrero == null)
                {
                    continue;
                }

                var res = serializadorRes.Deserializar(lineaRes);
                potrero.L_reses.Add(res);
            }
        }
    }
}
