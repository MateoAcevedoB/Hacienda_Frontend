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
    
    public class RepositorioVacunasEfCore : IRepositorioVacunas
    {
        private readonly HaciendaDbContext contexto;
        private readonly IValidadorVacuna validadorVacuna;

        public RepositorioVacunasEfCore(HaciendaDbContext contexto, IValidadorVacuna validadorVacuna)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            this.validadorVacuna = validadorVacuna ?? throw new ArgumentNullException(nameof(validadorVacuna));
        }

        public List<Vacuna> ObtenerDisponibles()
        {
            try
            {
                // ResId == null: ver comentario de clase. AsNoTracking:
                // ObtenerVacunasDisponibles/ObtenerEstadisticas/crear_vacuna
                // (comprobación de lote duplicado) solo leen, nunca vuelven a
                // guardar el resultado de esta consulta.
                return contexto.Vacunas
                    .AsNoTracking()
                    .Where(v => EF.Property<int?>(v, "ResId") == null)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar vacunas: {ex.Message}", ex);
            }
        }

        public void Agregar(Vacuna vacuna)
        {
            if (vacuna == null)
            {
                throw new ArgumentNullException(nameof(vacuna));
            }

            // Misma validación explícita que RepositorioVacunasTexto.Agregar,
            // con el mismo mensaje de error.
            if (!validadorVacuna.EsValido(vacuna))
            {
                throw new Exception("Error de validación en vacuna");
            }

            // La FK sombra "ResId" de una vacuna nueva queda sin fijar (null),
            // que es exactamente el estado "disponible en inventario" (ver
            // comentario de clase); no hace falta fijarla explícitamente.
            contexto.Vacunas.Add(vacuna);
            contexto.SaveChanges();
        }

        public void Remover(Vacuna vacuna)
        {
            if (vacuna == null)
            {
                throw new ArgumentNullException(nameof(vacuna));
            }

            
            var existente = contexto.Vacunas.Local
    .FirstOrDefault(v => v.Nombre == vacuna.Nombre && v.Lote == vacuna.Lote)
    ?? contexto.Vacunas
        .FirstOrDefault(v => v.Nombre == vacuna.Nombre
                           && v.Lote == vacuna.Lote
                           && EF.Property<int?>(v, "ResId") == null);

            if (existente != null)
            {
                contexto.Vacunas.Remove(existente);
                contexto.SaveChanges();
            }
        }

        
        public void MarcarComoAplicada(Vacuna vacuna, Res res)
{
    if (vacuna == null)
    {
        throw new ArgumentNullException(nameof(vacuna));
    }

    if (res == null)
    {
        throw new ArgumentNullException(nameof(res));
    }

    var entrada = contexto.ChangeTracker.Entries<Vacuna>()
        .FirstOrDefault(e => e.Entity.Nombre == vacuna.Nombre && e.Entity.Lote == vacuna.Lote);

    Vacuna existente;

    if (entrada != null)
    {
        existente = entrada.Entity;

        if (entrada.State == EntityState.Added)
        {
            
            entrada.State = EntityState.Modified;
        }
    }
    else
    {
        existente = contexto.Vacunas
            .FirstOrDefault(v => v.Nombre == vacuna.Nombre
                               && v.Lote == vacuna.Lote
                               && EF.Property<int?>(v, "ResId") == null);
    }

    if (existente != null)
    {
        contexto.Entry(existente).Property("ResId").CurrentValue = res.Id;
        contexto.SaveChanges();
    }
}
    }
}
