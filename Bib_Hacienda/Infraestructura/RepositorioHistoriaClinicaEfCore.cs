using System;
using System.Collections.Generic;
using System.Linq;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Validacion;
using Bib_Hacienda.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Bib_Hacienda.Infraestructura
{
    public class RepositorioHistoriaClinicaEfCore : IRepositorioHistoriaClinica
    {
        private readonly HaciendaDbContext contexto;
        private readonly IValidadorEventoClinico validadorEventoClinico;

        public RepositorioHistoriaClinicaEfCore(HaciendaDbContext contexto, IValidadorEventoClinico validadorEventoClinico)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            this.validadorEventoClinico = validadorEventoClinico ?? throw new ArgumentNullException(nameof(validadorEventoClinico));
        }

        public List<EventoClinico> ObtenerPorRes(string potreroId, string nombreRes)
        {
            try
            {
                string potreroIdNormalizado = potreroId?.ToLower();
                string nombreResNormalizado = nombreRes?.ToLower();

                return contexto.EventosClinicos
                    .AsNoTracking()
                    .Where(e => e.PotreroId.ToLower() == potreroIdNormalizado
                             && e.NombreRes.ToLower() == nombreResNormalizado)
                    .OrderBy(e => e.Fecha)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar historia clínica: {ex.Message}", ex);
            }
        }

        public void Agregar(EventoClinico evento)
        {
            if (evento == null)
            {
                throw new ArgumentNullException(nameof(evento));
            }

            if (!validadorEventoClinico.EsValido(evento))
            {
                throw new Exception("Error de validación en evento clínico");
            }

            contexto.EventosClinicos.Add(evento);
            contexto.SaveChanges();
        }
    }
}
