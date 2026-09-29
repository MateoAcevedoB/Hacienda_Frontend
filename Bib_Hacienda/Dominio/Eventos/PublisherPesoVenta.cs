using Bib_Hacienda.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherPesoVenta
    {
        public delegate void dele_peso_venta(string peso_venta);
        public event dele_peso_venta evt_peso_venta;

        public void Informar_Peso_Venta(Res res)
        {
            try
            {
                if (res.Peso >= res.PesoRecomendadoVenta)
                {
                    string mensaje = $"[Evento] La res '{res.Nombre}' tiene un peso {res.Peso}, apta para venta.";

                    if (evt_peso_venta != null)
                    {
                        evt_peso_venta(mensaje);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception er)
            {
                throw new Exception("Error inesperado en el metodo Informar_Peso_Venta: " + er.Message);
            }
        }
    }
}
