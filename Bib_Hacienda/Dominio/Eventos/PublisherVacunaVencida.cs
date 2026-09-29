using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherVacunaVencida
    {
        public delegate void dele_vacuna_vencida(string mensaje);
        public event dele_vacuna_vencida evt_vacuna_vencida;

        public bool Informar_Vacuna_Vencida(Vacuna vacuna)
        {
            try
            {
                if (vacuna == null)
                {
                    throw new ArgumentNullException(nameof(vacuna), "La vacuna no puede ser null");
                }

                DateTime fechaAlerta = DateTime.Now.AddMonths(1);
                bool esta_vencida = vacuna.Fecha_vencimiento <= DateTime.Now;
                bool alerta_vencimiento = vacuna.Fecha_vencimiento <= fechaAlerta && !esta_vencida;

                if (evt_vacuna_vencida != null)
                {
                    string mensaje;
                    if (esta_vencida)
                    {
                        mensaje = $"[Evento] La vacuna '{vacuna.Nombre}' del lote '{vacuna.Lote}' está vencida desde {vacuna.Fecha_vencimiento.ToShortDateString()}";
                    }
                    else if (alerta_vencimiento)
                    {
                        int diasRestantes = (vacuna.Fecha_vencimiento - DateTime.Now).Days;
                        mensaje = $"[Evento] ⚠ ALERTA: La vacuna '{vacuna.Nombre}' del lote '{vacuna.Lote}' vencerá en {diasRestantes} días ({vacuna.Fecha_vencimiento.ToShortDateString()})";
                    }
                    else
                    {
                        mensaje = $"[Evento] La vacuna '{vacuna.Nombre}' del lote '{vacuna.Lote}' es válida (vence el {vacuna.Fecha_vencimiento.ToShortDateString()})";
                    }
                    evt_vacuna_vencida(mensaje);
                }

                return esta_vencida;
            }
            catch (Exception er)
            {
                throw new Exception("[evento] Error inesperado en el metodo Informar_Vacuna_Vencida: " + er.Message);
            }
        }
    }
}
