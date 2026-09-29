using Bib_Hacienda.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib_Hacienda.Dominio.Reglas;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherPotreroMitad
    {
        public delegate void delegado_potrero_mitad(string mensaje);
        public event delegado_potrero_mitad evt_potrero_mitad;

        public void Informar_Potrero_Mitad(ushort cantidad_reses, Potrero potrero)
        {
            try
            {
                ushort capacidad_mitad = (ushort)(ReglaPotrero.max_reses_potrero / 2);

                if (cantidad_reses == capacidad_mitad)
                {
                    string mensaje = $"[Evento] El potrero '{potrero.Identificacion}' ha alcanzado la mitad de su capacidad máxima de reses.";

                    if (evt_potrero_mitad != null)
                    {
                        evt_potrero_mitad(mensaje);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception er)
            {
                throw new Exception("[Evento] Error inesperado en el metodo Informar_Potrero_Mitad: " + er.Message);
            }
        }
    }
}
