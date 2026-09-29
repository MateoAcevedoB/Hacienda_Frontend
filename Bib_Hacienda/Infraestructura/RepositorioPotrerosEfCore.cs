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

            contexto.Potreros.Add(potrero);
            contexto.SaveChanges();
        }

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
                contexto.Attach(potrero);
            }

            foreach (var res in potrero.L_reses)
            {
                if (res.Id == 0)
                {
                    var entradaRes = contexto.Entry(res);
                    entradaRes.State = EntityState.Added;
                    entradaRes.Property("PotreroId").CurrentValue = potrero.Identificacion;
                }
            }

            contexto.SaveChanges();
        }
    }
}
