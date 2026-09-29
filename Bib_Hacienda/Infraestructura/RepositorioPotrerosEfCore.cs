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
    // FASE 2A: implementación EF Core / PostgreSQL de IRepositorioPotreros.
    //
    // Comportamiento equivalente al de RepositorioPotrerosTexto: las mismas
    // 4 operaciones del contrato (ObtenerTodos, ObtenerPorId, Agregar,
    // Existe), con la misma validación explícita (H-11/H-12: validadorPotrero
    // + validadorRes) antes de persistir un Potrero nuevo. Usa
    // HaciendaDbContext en vez de Potreros.txt/Reses.txt.
    //
    // FASE 2C: la limitación documentada en fases anteriores (IRepositorioPotreros
    // no tenía ninguna operación para volver a persistir un Potrero ya
    // existente después de mutar su colección de reses) se resuelve
    // parcialmente en esta fase con el método Actualizar (ver su
    // implementación más abajo), usado por
    // ServicioPotreros.AnadirResAPotrero. vender_res sigue sin necesitar esta
    // operación (la Venta se persiste completa por RepositorioVentasEfCore.Agregar,
    // ver esa clase); aplicar_vacuna se resuelve aparte, en
    // RepositorioVacunasEfCore.MarcarComoAplicada.
    //
    // Tracking: ObtenerPorId NO usa AsNoTracking, a propósito. Es la única
    // consulta de este repositorio cuyo resultado puede participar después
    // en una escritura dentro del mismo caso de uso -por ejemplo,
    // ServicioVentas.vender_res llama primero a
    // IRepositorioPotreros.ObtenerPorId, y el Potrero devuelto (con la Res
    // que trae en L_reses) se usa para construir la Venta que luego
    // persiste RepositorioVentasEfCore.Agregar-. Si el Potrero llega
    // tracked (misma instancia de HaciendaDbContext, ver comentario en
    // RepositorioVentasEfCore), EF Core no necesita reinsertarlo. ObtenerTodos
    // y Existe, en cambio, solo se usan hoy para lectura/listado
    // (ObtenerTodosLosPotreros, ObtenerTodasLasReses, ObtenerEstadisticas,
    // y la comprobación de duplicados en ServicioPotreros.CrearPotrero), así
    // que sí usan AsNoTracking.
    public class RepositorioPotrerosEfCore : IRepositorioPotreros
    {
        private readonly HaciendaDbContext contexto;
        private readonly IValidadorPotrero validadorPotrero;
        private readonly IValidadorRes validadorRes;

        public RepositorioPotrerosEfCore(HaciendaDbContext contexto, IValidadorPotrero validadorPotrero, IValidadorRes validadorRes)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            this.validadorPotrero = validadorPotrero ?? throw new ArgumentNullException(nameof(validadorPotrero));
            this.validadorRes = validadorRes ?? throw new ArgumentNullException(nameof(validadorRes));
        }

        public List<Potrero> ObtenerTodos()
        {
            try
            {
                // Include(L_reses): misma forma en que RepositorioPotrerosTexto
                // carga cada Potrero con sus reses ya dentro de L_reses
                // (CargarResesDentroDePotreros). AsNoTracking: lectura pura,
                // ningún llamador actual de ObtenerTodos vuelve a guardar el
                // grafo devuelto.
                return contexto.Potreros
                    .Include(p => p.L_reses)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar potreros: {ex.Message}", ex);
            }
        }

        public Potrero ObtenerPorId(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return null;
            }

            // Comparación case-insensitive: mismo comportamiento que
            // RepositorioPotrerosTexto (StringComparison.OrdinalIgnoreCase).
            // ToLower() en ambos lados se traduce a SQL (LOWER(...) = LOWER(...)),
            // sin depender de una extensión específica de Npgsql.
            string idNormalizada = identificacion.ToLower();

            return contexto.Potreros
                .Include(p => p.L_reses)
                .FirstOrDefault(p => p.Identificacion.ToLower() == idNormalizada);
        }

        public bool Existe(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return false;
            }

            string idNormalizada = identificacion.ToLower();

            return contexto.Potreros
                .AsNoTracking()
                .Any(p => p.Identificacion.ToLower() == idNormalizada);
        }

        public void Agregar(Potrero potrero)
        {
            if (potrero == null)
            {
                throw new ArgumentNullException(nameof(potrero));
            }

            // Misma validación explícita que RepositorioPotrerosTexto.Agregar
            // (H-11/H-12), con los mismos mensajes de error.
            if (!validadorPotrero.EsValido(potrero))
            {
                throw new Exception("Error de validación en potrero");
            }

            foreach (var res in potrero.L_reses)
            {
                if (!validadorRes.EsValido(res))
                {
                    throw new Exception("Error de validación en res");
                }
            }

            // EF Core inserta el Potrero y, por la relación configurada en
            // HaciendaDbContext (potrero.HasMany(p => p.L_reses)...), en
            // cascada cualquier Res que ya traiga en L_reses (equivalente a
            // que RepositorioPotrerosTexto también escriba esas reses en
            // Reses.txt dentro de la misma llamada a Agregar). Hoy
            // ServicioPotreros.CrearPotrero siempre llama a Agregar con
            // L_reses vacía; este repositorio no asume eso y soporta ambos
            // casos igual que el repositorio de texto.
            contexto.Potreros.Add(potrero);
            contexto.SaveChanges();
        }

        // FASE 2C: implementación de IRepositorioPotreros.Actualizar.
        //
        // Llamador real (único, hoy): ServicioPotreros.AnadirResAPotrero,
        // después de Potrero.AgregarRes(res). El Potrero que llega aquí
        // proviene de ServicioPotreros.BuscarPotrero -> repositorioPotreros.ObtenerTodos(),
        // que usa AsNoTracking (ver ObtenerTodos arriba): es un grafo
        // DESCONECTADO de este contexto. Sus reses existentes ya tienen Id
        // (vinieron de la base de datos); la res recién añadida por
        // AgregarRes todavía tiene Id == 0 (default(int), ver Res.Id) porque
        // no ha sido insertada.
        //
        // No se usa Update(potrero) (que marcaría TODO el grafo, reses
        // existentes incluidas, como Modified) porque eso reescribiría filas
        // de Reses que no cambiaron. En su lugar: el propio Potrero se
        // adjunta como Unchanged (ya existe, su PK -Identificacion- ya tiene
        // valor), y solo las reses con Id == 0 (nuevas, sin persistir todavía)
        // se marcan explícitamente como Added, con la FK sombra "PotreroId"
        // fijada a mano (no hay navegación inversa Res -> Potrero, ver
        // HaciendaDbContext.ConfigurarPotrero). Las reses con Id != 0 no se
        // tocan: ya están persistidas y esta operación no las modifica.
        public void Actualizar(Potrero potrero)
        {
            if (potrero == null)
            {
                throw new ArgumentNullException(nameof(potrero));
            }

            if (!validadorPotrero.EsValido(potrero))
            {
                throw new Exception("Error de validación en potrero");
            }

            foreach (var res in potrero.L_reses)
            {
                if (!validadorRes.EsValido(res))
                {
                    throw new Exception("Error de validación en res");
                }
            }

            var entradaPotrero = contexto.ChangeTracker.Entries<Potrero>()
                .FirstOrDefault(e => e.Entity.Identificacion == potrero.Identificacion);

            if (entradaPotrero == null)
            {
                // No estaba tracked en este contexto todavía (caso típico:
                // llegó vía ObtenerTodos/AsNoTracking). Se adjunta como
                // Unchanged: ya existe en la base de datos (misma lógica que
                // RepositorioVentasEfCore.Agregar usa para Potrero/Res ya
                // existentes que llegan desconectados).
                contexto.Attach(potrero);
            }

            foreach (var res in potrero.L_reses)
            {
                if (res.Id == 0)
                {
                    // Res nueva (todavía sin PK asignada por la base de
                    // datos): se marca explícitamente para inserción,
                    // independientemente de cómo Attach() haya clasificado
                    // el grafo, y se fija la FK sombra "PotreroId" (sin
                    // navegación inversa en el dominio real, ver
                    // ConfigurarReses).
                    var entradaRes = contexto.Entry(res);
                    entradaRes.State = EntityState.Added;
                    entradaRes.Property("PotreroId").CurrentValue = potrero.Identificacion;
                }
            }

            contexto.SaveChanges();
        }
    }
}
