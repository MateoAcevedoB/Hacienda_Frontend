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
    
    public class RepositorioVentasEfCore : IRepositorioVentas
    {
        private readonly HaciendaDbContext contexto;
        private readonly IValidadorVenta validadorVenta;

        public RepositorioVentasEfCore(HaciendaDbContext contexto, IValidadorVenta validadorVenta)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            this.validadorVenta = validadorVenta ?? throw new ArgumentNullException(nameof(validadorVenta));
        }

        public List<Venta> ObtenerTodas()
        {
            try
            {
                
                var filas = contexto.Ventas
                    .Include(v => v.Potrero)
                    .AsNoTracking()
                    .Select(v => new
                    {
                        Venta = v,
                        ResId = EF.Property<int>(v, "ResId")
                    })
                    .ToList();

                var idsReses = filas.Select(f => f.ResId).Distinct().ToList();

                var reses = contexto.Reses
                    .AsNoTracking()
                    .Where(r => idsReses.Contains(r.Id))
                    .ToDictionary(r => r.Id);

                
                foreach (var fila in filas)
                {
                    fila.Venta.Activo = reses.TryGetValue(fila.ResId, out var res) ? res : null;
                }

                return filas.Select(f => f.Venta).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar ventas: {ex.Message}", ex);
            }
        }

        public void Agregar(Venta venta)
        {
            if (venta == null)
            {
                throw new ArgumentNullException(nameof(venta));
            }

            
            if (!validadorVenta.EsValido(venta))
            {
                throw new Exception("Error de validación en venta");
            }

            if (!(venta.Activo is Res resVendida))
            {
                
                throw new InvalidOperationException(
                    "RepositorioVentasEfCore solo puede persistir ventas cuyo Activo sea una Res " +
                    "(única implementación de IActivoVendible mapeada en HaciendaDbContext).");
            }

           
            if (venta.Potrero != null)
            {
                contexto.Entry(venta.Potrero);
            }
            contexto.Entry(resVendida);

            contexto.Ventas.Add(venta);

           
            contexto.Entry(venta).Property("ResId").CurrentValue = resVendida.Id;

            contexto.SaveChanges();
        }
    }
}
