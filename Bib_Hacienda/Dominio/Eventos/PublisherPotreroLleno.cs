using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherPotreroLleno
    {
        public delegate void delegado_potrero_lleno(string mensaje);
        public event delegado_potrero_lleno evt_potrero_lleno;

        public void Informar_Potrero_Lleno(ushort cantidad_reses, Potrero potrero)
        {
            try
            {
                if (cantidad_reses == ReglaPotrero.max_reses_potrero)
                {
                    string mensaje = $"[Evento] El potrero '{potrero.Identificacion}' ha alcanzado su capacidad máxima de reses ({ReglaPotrero.max_reses_potrero}). No se pueden agregar más reses.";

                    if (evt_potrero_lleno != null)
                    {
                        evt_potrero_lleno(mensaje);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception er)
            {
                throw new Exception("[Evento] Error inesperado en el metodo Informar_Potrero_Lleno: " + er.Message);
            }
        }
    }
}
