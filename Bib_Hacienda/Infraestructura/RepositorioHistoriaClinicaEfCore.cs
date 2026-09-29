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
    // FASE 2B: implementación EF Core / PostgreSQL de IRepositorioHistoriaClinica.
    //
    // Comportamiento equivalente al de RepositorioHistoriaClinicaTexto: las
    // mismas 2 operaciones del contrato (ObtenerPorRes, Agregar), con la
    // misma validación explícita (IValidadorEventoClinico) antes de
    // persistir. Igual que el repositorio de texto, no existe operación de
    // "Remover" (SC-3 solo pide asociar, registrar y consultar).
    //
    // PotreroId + NombreRes: EventoClinico no tiene, en el dominio real, una
    // referencia de objeto a Res ni a Potrero (ver comentario de
    // EventoClinico.cs), y HaciendaDbContext.ConfigurarEventosClinicos
    // mapea PotreroId/NombreRes como columnas de texto simples, SIN ninguna
    // FK/navegación hacia Potrero o Res (solo un índice no-único sobre
    // ambas). Este repositorio respeta exactamente ese modelo: sigue
    // filtrando por la pareja de strings PotreroId+NombreRes, igual que
    // ObtenerPorRes en el repositorio de texto; no se inventa ninguna
    // relación nueva para "facilitar" la consulta.
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
                // Comparación case-insensitive y orden cronológico: mismo
                // comportamiento que RepositorioHistoriaClinicaTexto.ObtenerPorRes
                // (StringComparison.OrdinalIgnoreCase + OrderBy(Fecha)).
                // ToLower() en ambos lados se traduce a SQL, sin depender de
                // una extensión específica de Npgsql.
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

            // Misma validación explícita que RepositorioHistoriaClinicaTexto.Agregar,
            // con el mismo mensaje de error.
            if (!validadorEventoClinico.EsValido(evento))
            {
                throw new Exception("Error de validación en evento clínico");
            }

            contexto.EventosClinicos.Add(evento);
            contexto.SaveChanges();
        }
    }
}
