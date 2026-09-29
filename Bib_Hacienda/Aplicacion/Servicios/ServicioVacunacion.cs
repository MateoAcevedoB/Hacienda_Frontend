using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bib_Hacienda.Aplicacion
{
    public class ServicioVacunacion : IVacunacion
    {
        private IRepositorioPotreros repositorioPotreros;
        private IRepositorioVacunas repositorioVacunas;
        private PublisherVacunaVencida publisherVacunaVencida;
        private PublisherVacunacionCompletada publisherVacunacionCompletada;

        public ServicioVacunacion(IRepositorioPotreros repositorioPotreros, IRepositorioVacunas repositorioVacunas, PublisherVacunaVencida publisherVacunaVencida, PublisherVacunacionCompletada publisherVacunacionCompletada)
        {
            this.repositorioPotreros = repositorioPotreros;
            this.repositorioVacunas = repositorioVacunas;
            this.publisherVacunaVencida = publisherVacunaVencida;
            this.publisherVacunacionCompletada = publisherVacunacionCompletada;
        }

        public ResultadoOperacion aplicar_vacuna(Vacuna vacuna, string nombre, string id_potrero)
        {
            try
            {
                string mensaje_vacuna = "";
                string mensaje_vacunacion = "";

                Potrero potrero = repositorioPotreros.ObtenerPorId(id_potrero);
                if (potrero == null)
                {
                    return ResultadoOperacion.Fallo($"No se encontró el potrero '{id_potrero}'");
                }

                Res res = potrero.BuscarRes(nombre);

                if (vacuna == null)
                {
                    return ResultadoOperacion.Fallo("La vacuna no puede ser nula");
                }

                byte contador_bacterianas = 0;
                byte contador_vivas = 0;

                if (res.L_vacunas_aplicadas.Any(v => v.Nombre == vacuna.Nombre || v.Lote == vacuna.Lote))
                {
                    return ResultadoOperacion.Fallo($"La vacuna '{vacuna.Nombre}' ya fue aplicada a la res '{res.Nombre}'.");
                }

                foreach (Vacuna vac in res.L_vacunas_aplicadas)
                {
                    if (vac is Bacteriana)
                    {
                        contador_bacterianas++;
                    }
                    else if (vac is Viva)
                    {
                        contador_vivas++;
                    }
                }

                byte max_bac = res.MaxVacunasBacterianas;
                byte max_viv = res.MaxVacunasVivas;

                if (vacuna is Bacteriana && contador_bacterianas >= max_bac)
                {
                    return ResultadoOperacion.Fallo($"No se puede aplicar más vacunas bacterianas a la res '{res.Nombre}'. Ya tiene las {max_bac} permitidas.");
                }

                if (vacuna is Viva && contador_vivas >= max_viv)
                {
                    return ResultadoOperacion.Fallo($"No se puede aplicar más vacunas vivas a la res '{res.Nombre}'. Ya tiene las {max_viv} permitidas.");
                }

                publisherVacunaVencida.evt_vacuna_vencida += (mensaje) =>
                {
                    mensaje_vacuna = mensaje;
                };

                bool vacuna_vencida = publisherVacunaVencida.Informar_Vacuna_Vencida(vacuna);

                if (vacuna_vencida)
                {
                    return ResultadoOperacion.Fallo(mensaje_vacuna);
                }

                res.L_vacunas_aplicadas.Add(vacuna);

                
                repositorioVacunas.MarcarComoAplicada(vacuna, res);

                if (vacuna is Bacteriana)
                {
                    contador_bacterianas++;
                }
                else if (vacuna is Viva)
                {
                    contador_vivas++;
                }

                publisherVacunacionCompletada.evt_vacunacion_completada += (mensaje) =>
                {
                    mensaje_vacunacion = mensaje;
                };

                publisherVacunacionCompletada.Informar_Vacunacion_Completada(res, contador_bacterianas, contador_vivas);

                return ResultadoOperacion.Ok($"Vacuna aplicada correctamente a la res {res.Nombre}. {mensaje_vacunacion}");
            }
            catch (Exception err)
            {
                return ResultadoOperacion.Fallo("Error inesperado en el metodo aplicar_vacuna: " + err.Message);
            }
        }

        public List<Vacuna> ObtenerVacunasAplicadas(string potreroId, string nombreRes)
        {
            try
            {
                var potrero = repositorioPotreros.ObtenerPorId(potreroId);
                var res = potrero.BuscarRes(nombreRes);
                return res.L_vacunas_aplicadas;
            }
            catch
            {
                return new List<Vacuna>();
            }
        }
    }
}
