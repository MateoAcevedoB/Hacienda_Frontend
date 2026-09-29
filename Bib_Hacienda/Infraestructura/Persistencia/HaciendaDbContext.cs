using Bib_Hacienda.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Bib_Hacienda.Infraestructura.Persistencia
{
    
    public class HaciendaDbContext : DbContext
    {
        private static readonly ValueConverter<DateTime, DateTime> ConversorUtc =
            new(
                fecha => fecha.Kind == DateTimeKind.Utc
                    ? fecha
                    : fecha.Kind == DateTimeKind.Local
                        ? fecha.ToUniversalTime()
                        : DateTime.SpecifyKind(fecha, DateTimeKind.Utc),
                fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

        public HaciendaDbContext(DbContextOptions<HaciendaDbContext> options) : base(options)
        {
        }

        
        public DbSet<Potrero> Potreros { get; set; }

        
        public DbSet<Res> Reses { get; set; }

        
        public DbSet<Vacuna> Vacunas { get; set; }

        public DbSet<Venta> Ventas { get; set; }

        
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<EventoClinico> EventosClinicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarPotrero(modelBuilder);
            ConfigurarReses(modelBuilder);
            ConfigurarVacunas(modelBuilder);
            ConfigurarVentas(modelBuilder);
            ConfigurarUsuarios(modelBuilder);
            ConfigurarEventosClinicos(modelBuilder);
        }

        private static void ConfigurarPotrero(ModelBuilder modelBuilder)
        {
            var potrero = modelBuilder.Entity<Potrero>();

            potrero.ToTable("Potreros");

            
            potrero.HasKey(p => p.Identificacion);
            potrero.Property(p => p.Identificacion).HasMaxLength(100).IsRequired();

            potrero.Property(p => p.Tipo_potrero).HasMaxLength(100).IsRequired();

            
            potrero.HasMany(p => p.L_reses)
                   .WithOne()
                   .HasForeignKey("PotreroId")
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            
        }

        private static void ConfigurarReses(ModelBuilder modelBuilder)
        {
            var res = modelBuilder.Entity<Res>();

            res.ToTable("Reses");

            
            res.HasKey(r => r.Id);
            res.Property(r => r.Id).ValueGeneratedOnAdd();

            res.Property(r => r.Nombre).HasMaxLength(200).IsRequired();

            
            res.Property(r => r.Peso).HasConversion<long>().IsRequired();
            res.Property(r => r.Edad).HasConversion<int>().IsRequired();

            
            res.Ignore(r => r.Identificador);

           
            res.HasDiscriminator<string>("TipoRes")
               .HasValue<Ternero>("Ternero")
               .HasValue<Cebon>("Cebon")
               .HasValue<Novillo>("Novillo");

            
            res.HasMany(r => r.L_vacunas_aplicadas)
               .WithOne()
               .HasForeignKey("ResId")
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigurarVacunas(ModelBuilder modelBuilder)
        {
            var vacuna = modelBuilder.Entity<Vacuna>();

            vacuna.ToTable("Vacunas");

            
            vacuna.HasKey(v => new { v.Nombre, v.Lote });
            vacuna.Property(v => v.Nombre).HasMaxLength(200);
            vacuna.Property(v => v.Lote).HasMaxLength(100);

            vacuna.Property(v => v.Fecha_vencimiento)
                  .HasConversion(ConversorUtc)
                  .IsRequired();
            vacuna.Property(v => v.Fecha_aplicacion)
                  .HasConversion(ConversorUtc)
                  .IsRequired();

            vacuna.HasDiscriminator<string>("TipoVacuna")
                  .HasValue<Bacteriana>("Bacteriana")
                  .HasValue<Viva>("Viva");

            
            modelBuilder.Entity<Viva>()
                .Property<Viva.enum_l_atenuaciones>("periodo_atenuacion")
                .HasField("periodo_atenuacion")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("PeriodoAtenuacion")
                .IsRequired();

            
            modelBuilder.Entity<Bacteriana>()
                .Property(b => b.Periodo_aplicacion)
                .HasConversion<long>()
                .IsRequired();
        }

        private static void ConfigurarVentas(ModelBuilder modelBuilder)
        {
            var venta = modelBuilder.Entity<Venta>();

            venta.ToTable("Ventas");

            
            venta.Property<int>("Id").ValueGeneratedOnAdd();
            venta.HasKey("Id");

            venta.Property(v => v.Fecha)
                 .HasConversion(ConversorUtc)
                 .IsRequired();

            venta.Property(v => v.Monto).HasConversion<long>().IsRequired();

            
            venta.Ignore(v => v.Activo);

            venta.HasOne<Res>()
                 .WithMany()
                 .HasForeignKey("ResId")
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);

            
            venta.HasOne(v => v.Potrero)
                 .WithMany()
                 .HasForeignKey("PotreroId")
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigurarUsuarios(ModelBuilder modelBuilder)
        {
            var usuario = modelBuilder.Entity<Usuario>();

            usuario.ToTable("Usuarios");

            
            usuario.HasKey(u => u.Nombre);
            usuario.Property(u => u.Nombre).HasMaxLength(100);

            
            usuario.Property(u => u.ContrasenaHash).IsRequired();
        }

        private static void ConfigurarEventosClinicos(ModelBuilder modelBuilder)
        {
            var evento = modelBuilder.Entity<EventoClinico>();

            evento.ToTable("EventosClinicos");

            
            evento.HasKey(e => e.Id);
            evento.Property(e => e.Id).ValueGeneratedOnAdd();

            evento.Property(e => e.PotreroId).HasMaxLength(100).IsRequired();
            evento.Property(e => e.NombreRes).HasMaxLength(200).IsRequired();
            evento.Property(e => e.TipoEvento).HasMaxLength(100).IsRequired();
            evento.Property(e => e.Fecha).IsRequired();

            
            evento.HasIndex(e => new { e.PotreroId, e.NombreRes });
        }

    }
}
