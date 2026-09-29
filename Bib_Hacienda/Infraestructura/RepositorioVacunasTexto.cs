using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Validacion;

namespace Bib_Hacienda.Infraestructura
{
    public class RepositorioVacunasTexto : IRepositorioVacunas
    {
        private const string ArchivoVacunas = "Vacunas.txt";

        private readonly ISerializador<Vacuna> serializadorVacuna;
        private readonly IValidadorVacuna validadorVacuna;

        public RepositorioVacunasTexto(ISerializador<Vacuna> serializadorVacuna, IValidadorVacuna validadorVacuna)
        {
            this.serializadorVacuna = serializadorVacuna;
            this.validadorVacuna = validadorVacuna;
        }

        public List<Vacuna> ObtenerDisponibles()
        {
            try
            {
                return CargarVacunasDesdeArchivo();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar vacunas: {ex.Message}", ex);
            }
        }

        public void Agregar(Vacuna vacuna)
        {
            if (vacuna == null)
            {
                throw new ArgumentNullException(nameof(vacuna));
            }

            if (!validadorVacuna.EsValido(vacuna))
            {
                throw new Exception("Error de validación en vacuna");
            }

            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoVacunas);
            File.AppendAllLines(ruta, new[] { serializadorVacuna.Serializar(vacuna) });
        }

        public void Remover(Vacuna vacuna)
        {
            if (vacuna == null)
            {
                throw new ArgumentNullException(nameof(vacuna));
            }

            var restantes = CargarVacunasDesdeArchivo()
                .Where(v => !(string.Equals(v.Nombre, vacuna.Nombre, StringComparison.Ordinal) && string.Equals(v.Lote, vacuna.Lote, StringComparison.Ordinal)))
                .ToList();

            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoVacunas);
            var lineas = restantes.Select(v => serializadorVacuna.Serializar(v));
            File.WriteAllLines(ruta, lineas);
        }

        public void MarcarComoAplicada(Vacuna vacuna, Res res)
        {
            Remover(vacuna);
        }

        private List<Vacuna> CargarVacunasDesdeArchivo()
        {
            string ruta = Path.Combine(DirectorioDatos.ObtenerRuta(), ArchivoVacunas);
            var vacunas = new List<Vacuna>();

            if (!File.Exists(ruta))
            {
                return vacunas;
            }

            foreach (var linea in File.ReadAllLines(ruta))
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                vacunas.Add(serializadorVacuna.Deserializar(linea));
            }

            return vacunas;
        }
    }
}
