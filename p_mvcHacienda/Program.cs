using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Validacion;
using Bib_Hacienda.Infraestructura;

namespace p_mvcHacienda
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.Cookie.Name = "HaciendaSoft.Auth";
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            builder.Services.AddHttpContextAccessor();


            builder.Services.AddScoped<ISerializador<Potrero>, SerializadorPotrero>();
            builder.Services.AddScoped<ISerializador<Res>, SerializadorRes>();
            builder.Services.AddScoped<ISerializador<Usuario>, SerializadorUsuario>();
            builder.Services.AddScoped<ISerializador<Vacuna>, SerializadorVacuna>();
            builder.Services.AddScoped<ISerializador<Venta>, SerializadorVenta>();
            builder.Services.AddScoped<ISerializador<EventoClinico>, SerializadorEventoClinico>();

            builder.Services.AddScoped<IValidadorPotrero, ValidadorPotrero>();
            builder.Services.AddScoped<IValidadorRes, ValidadorRes>();
            builder.Services.AddScoped<IValidadorVacuna, ValidadorVacuna>();
            builder.Services.AddScoped<IValidadorVenta, ValidadorVenta>();
            builder.Services.AddScoped<IValidadorEventoClinico, ValidadorEventoClinico>();

            builder.Services.AddScoped<IRepositorioPotreros, RepositorioPotrerosTexto>();
            builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosTexto>();
            builder.Services.AddScoped<IRepositorioVacunas, RepositorioVacunasTexto>();
            builder.Services.AddScoped<IRepositorioVentas, RepositorioVentasTexto>();
            builder.Services.AddScoped<IRepositorioHistoriaClinica, RepositorioHistoriaClinicaTexto>();

            builder.Services.AddScoped<IFabricaVacunaBacteriana, FabricaVacunaBacteriana>();
            builder.Services.AddScoped<IFabricaVacunaViva, FabricaVacunaViva>();

            builder.Services.AddScoped<IRegistroFabricasRes>(sp =>
            {
                var fabricas = new Dictionary<string, IFabricaRes>
                {
                    { "ternero", new FabricaTernero() },
                    { "cebon", new FabricaCebon() },
                    { "novillo", new FabricaNovillo() }
                };
                return new RegistroFabricasRes(fabricas);
            });

            builder.Services.AddScoped<IServicioHash, ServicioHashSha256>();
            builder.Services.AddScoped<IProveedorPermisos, ProveedorPermisosPorRol>();

            builder.Services.AddScoped<PublisherPesoMin>();
            builder.Services.AddScoped<PublisherPesoVenta>();
            builder.Services.AddScoped<PublisherVacunaVencida>();
            builder.Services.AddScoped<PublisherVacunacionCompletada>();

            builder.Services.AddScoped<ServicioAutenticacion>();
            builder.Services.AddScoped<ServicioPotreros>();
            builder.Services.AddScoped<ServicioAlimentacion>();
            builder.Services.AddScoped<ServicioVentas>();
            builder.Services.AddScoped<ServicioInventarioVacunas>();
            builder.Services.AddScoped<ServicioVacunacion>();
            builder.Services.AddScoped<ServicioHistoriaClinica>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
