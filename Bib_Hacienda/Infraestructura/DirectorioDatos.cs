using System;
using System.IO;

namespace Bib_Hacienda.Infraestructura
{
    internal static class DirectorioDatos
    {
        private const string NombreCarpeta = "Datos";

        public static string ObtenerRuta()
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NombreCarpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            return ruta;
        }
    }
}
