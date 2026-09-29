using Api_Hacienda.Autenticacion;
using Api_Hacienda.Middleware;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Validacion;
using Bib_Hacienda.Infraestructura;
using Bib_Hacienda.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

const string PoliticaCorsFrontend = "FrontendAngular";

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsFrontend, policy =>
        policy.WithOrigins("http://localhost:4200")
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type"));
});

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddExceptionHandler<ManejadorGlobalExcepciones>();
builder.Services.AddProblemDetails();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresar únicamente el JWT (sin el prefijo 'Bearer '), Swagger lo agrega automáticamente."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<HaciendaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HaciendaDatabase")));



builder.Services.AddScoped<IValidadorPotrero, ValidadorPotrero>();
builder.Services.AddScoped<IValidadorRes, ValidadorRes>();
builder.Services.AddScoped<IValidadorVacuna, ValidadorVacuna>();
builder.Services.AddScoped<IValidadorVenta, ValidadorVenta>();
builder.Services.AddScoped<IValidadorEventoClinico, ValidadorEventoClinico>();

builder.Services.AddScoped<IRepositorioPotreros, RepositorioPotrerosEfCore>();
builder.Services.AddScoped<IRepositorioVentas, RepositorioVentasEfCore>();
builder.Services.AddScoped<IRepositorioVacunas, RepositorioVacunasEfCore>();
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosEfCore>();
builder.Services.AddScoped<IRepositorioHistoriaClinica, RepositorioHistoriaClinicaEfCore>();


builder.Services.AddScoped<IServicioHash, ServicioHashSha256>();
builder.Services.AddScoped<IProveedorPermisos, ProveedorPermisosPorRol>();
builder.Services.AddScoped<ServicioAutenticacion>();


builder.Services.AddScoped<IGeneradorTokenJwt, GeneradorTokenJwt>();


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

builder.Services.AddScoped<ServicioPotreros>();


builder.Services.AddScoped<IFabricaVacunaBacteriana, FabricaVacunaBacteriana>();
builder.Services.AddScoped<IFabricaVacunaViva, FabricaVacunaViva>();

builder.Services.AddScoped<PublisherVacunaVencida>();
builder.Services.AddScoped<PublisherVacunacionCompletada>();

builder.Services.AddScoped<ServicioInventarioVacunas>();
builder.Services.AddScoped<ServicioVacunacion>();
builder.Services.AddScoped<ServicioVentas>();



string jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? string.Empty;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
            
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(PoliticaCorsFrontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
