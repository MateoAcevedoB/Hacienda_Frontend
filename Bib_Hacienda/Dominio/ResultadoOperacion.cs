using System;

namespace Bib_Hacienda.Dominio
{
    public class ResultadoOperacion
    {
        public bool Exito { get; }
        public string Mensaje { get; }

        public ResultadoOperacion(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        public static ResultadoOperacion Ok(string mensaje)
        {
            return new ResultadoOperacion(true, mensaje);
        }

        public static ResultadoOperacion Fallo(string mensaje)
        {
            return new ResultadoOperacion(false, mensaje);
        }
    }
}
