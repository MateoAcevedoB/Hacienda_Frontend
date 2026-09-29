using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio.Eventos
{
    public class PublisherVacunacionCompletada
    {
        public delegate void dele_vacunacion_completada(string mensaje);
        public event dele_vacunacion_completada evt_vacunacion_completada;

        public bool Informar_Vacunacion_Completada(Res res, ushort contador_bacterianas, ushort contador_vivas)
        {
            try
            {
                if (res == null)
                {
                    throw new ArgumentNullException(nameof(res), "La res no puede ser null");
                }

                bool esquema_completo = contador_bacterianas >= res.MaxVacunasBacterianas && contador_vivas >= res.MaxVacunasVivas;

                if (evt_vacunacion_completada != null)
                {
                    string mensaje;
                    if (esquema_completo)
                    {
                        mensaje = $"[Evento] La res '{res.Nombre}' ha completado su esquema de vacunación.";
                    }
                    else
                    {
                        mensaje = $"[Evento] La res '{res.Nombre}' aún no ha completado su esquema de vacunación. Bacterianas: {contador_bacterianas}, Vivas: {contador_vivas}";
                    }
                    evt_vacunacion_completada(mensaje);
                }

                return esquema_completo;
            }
            catch (Exception er)
            {
                throw new Exception("[evento] Error inesperado en el metodo Informar_Vacunacion_Completada: " + er.Message);
            }
        }
    }
}
