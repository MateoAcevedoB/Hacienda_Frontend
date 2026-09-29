# Migración `InitialCreate` — NO generada en este entorno

Esta carpeta está preparada como destino de la migración inicial de EF Core
(`dotnet ef migrations add InitialCreate --project Bib_Hacienda --startup-project Api_Hacienda`),
pero **la migración no pudo generarse en el entorno donde se ejecutó esta
fase**, por la siguiente razón concreta y verificable:

## Causa exacta

El entorno de ejecución solo tiene salida de red permitida hacia una lista
blanca de dominios (paquetes de npm, PyPI, crates.io, GitHub, repositorios
`apt` de Ubuntu, etc.). **`api.nuget.org` no está en esa lista.**

Se comprobó directamente:

```
$ dotnet restore Api_Hacienda/Api_Hacienda.csproj
...
error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json.
error : Response status code does not indicate success: 403 (Forbidden).
```

Como consecuencia:

1. `dotnet restore` **no puede** descargar `Microsoft.EntityFrameworkCore`,
   `Npgsql.EntityFrameworkCore.PostgreSQL` ni `Microsoft.EntityFrameworkCore.Design`
   (los `<PackageReference>` ya están declarados en los `.csproj`, ver
   reporte final, pero sus `.dll` no pudieron descargarse).
2. Sin esos paquetes restaurados, `dotnet build` de `Bib_Hacienda` (que
   ahora referencia `Microsoft.EntityFrameworkCore` para `HaciendaDbContext`)
   y de `Api_Hacienda` **no pudo ejecutarse hasta el final** para esta parte
   del código. (`Bib_Hacienda` y `p_mvcHacienda` SÍ se compilaron
   correctamente en su estado previo a estos paquetes, ver reporte final).
3. Sin build correcto, la herramienta `dotnet ef` (que además es en sí
   misma un paquete NuGet, `dotnet-ef`, que tampoco pudo instalarse) no
   pudo ejecutar `migrations add`.

## Qué SÍ se hizo

Todo el modelo de EF Core (`HaciendaDbContext.cs`) fue escrito completo y
revisado manualmente contra la API pública de EF Core 8 / Npgsql 8, con
mapeo del dominio real (ver `HaciendaDbContext.cs` y el reporte final de la
fase). No se inventó ningún resultado de compilación ni de migración.

## Qué falta (para quien continúe, con acceso a NuGet)

Con acceso a `api.nuget.org` (o un feed NuGet interno equivalente), desde la
raíz del repositorio:

```bash
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate --project Bib_Hacienda --startup-project Api_Hacienda --output-dir Infraestructura/Persistencia/Migrations
```

**IMPORTANTE**: no ejecutar `dotnet ef database update` (la fase 1B
explícitamente prohíbe aplicar la migración a PostgreSQL todavía).

Si al generar la migración aparece algún error de mapeo (por ejemplo, en el
"constructor binding" de `Potrero`/`Venta`/`Viva`, o en el mapeo de campo
privado de `Viva.periodo_atenuacion`), son exactamente los puntos que este
reporte marca como pendientes de verificación real (ver sección
"Advertencias para compilar" del reporte final de la fase), porque no
pudieron probarse contra el compilador y las librerías reales de EF Core en
este entorno.
