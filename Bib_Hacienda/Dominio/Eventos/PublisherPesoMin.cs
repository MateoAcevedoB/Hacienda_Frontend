using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherPesoMin
    {
        public delegate void dele_peso_min(string peso_min);
        public event dele_peso_min evt_peso_min;

        public void Informar_Peso_Min(Res res)
        {
            try
            {
                if (res.Peso < res.PesoMinimo)
                {
                    string mensaje = $"[Evento] La res '{res.Nombre}' tiene un peso {res.Peso}, está en desnutrición.";

                    if (evt_peso_min != null)
                    {
                        evt_peso_min(mensaje);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception er)
            {
                throw new Exception("[Evento] Error inesperado en el metodo Informar_Peso_Min: " + er.Message);
            }
        }
    }
}
