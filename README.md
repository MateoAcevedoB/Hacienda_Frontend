# Hacienda Backend
Mateo Acevedo
Mateo Polanco

## 1. Descripción

Backend REST del sistema de gestión de hacienda ganadera (Potreros, Reses, Vacunas, Vacunación, Ventas, Usuarios), desarrollado en .NET 8 con arquitectura en capas y persistencia en PostgreSQL vía Entity Framework Core.

El proyecto incluye, en la misma solución:

- **`Api_Hacienda`**: Backend REST (JWT), objeto de este README.
- **`p_mvcHacienda`**: aplicación MVC.
- **`Bib_Hacienda`**: librería de dominio y aplicación.
- **`Bib_Hacienda.Tests`**: pruebas unitarias (xUnit + Moq) de `Bib_Hacienda`.

## 2. Arquitectura

```
Controller
    ↓
Servicio de aplicación (Bib_Hacienda.Aplicacion.Servicios)
    ↓
Interfaz de repositorio (Bib_Hacienda.Aplicacion.Repositorios)
    ↓
Repositorio EF Core (Bib_Hacienda.Infraestructura)
    ↓
HaciendaDbContext (Bib_Hacienda.Infraestructura.Persistencia)
    ↓
PostgreSQL
```

Reglas respetadas en todo `Api_Hacienda`:

- Los Controllers **no** contienen lógica de negocio: delegan en servicios de aplicación y solo deciden el código HTTP (404 por existencia de recurso, 400 por fallo de negocio, etc.).
- Los servicios de aplicación **no** acceden directamente a Entity Framework: dependen de interfaces de repositorio.
- `Api_Hacienda` es el **composition root**: solo ahí se conecta `HaciendaDbContext` con el proveedor concreto PostgreSQL/Npgsql (`Program.cs`).
- Manejo global de excepciones no controladas mediante `IExceptionHandler` nativo de ASP.NET Core 8 (`Middleware/ManejadorGlobalExcepciones.cs`), que nunca expone stack traces, connection strings ni secretos JWT en la respuesta HTTP.

## 3. Estructura de proyectos

```
Hacienda_TOBE_BACKEND/
├── Hacienda_TOBE.sln
├── Api_Hacienda/              # Backend REST (este README)
│   ├── Controllers/
│   ├── Models/                # DTOs de entrada/salida
│   ├── Middleware/            # Manejo global de excepciones
│   ├── Autenticacion/         # Generación de JWT
│   └── Program.cs             # Composition root
├── Bib_Hacienda/               # Dominio + Aplicación + Infraestructura
│   ├── Dominio/
│   ├── Aplicacion/
│   └── Infraestructura/
│       └── Persistencia/
│           ├── HaciendaDbContext.cs
│           └── Migrations/    # Pendiente de generar, ver sección 9
├── Bib_Hacienda.Tests/         # xUnit + Moq + Coverlet
├── p_mvcHacienda/              # Frontend MVC (cookies), no modificado
└── postman/
    ├── Hacienda_TOBE_Backend.postman_collection.json
    └── Hacienda_TOBE_Backend.postman_environment.json
```

## 4. Tecnologías

- **.NET 8** (`net8.0`)
- **ASP.NET Core Web API** (`Api_Hacienda`)
- **Entity Framework Core 8.0.10** + **Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10**
- **PostgreSQL**
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer 8.0.10`)
- **Swagger / OpenAPI** (`Swashbuckle.AspNetCore 6.6.2`), con soporte de botón "Authorize" para pegar el JWT
- **xUnit 2.9.2** + **Moq 4.20.72** (pruebas unitarias, `Bib_Hacienda.Tests`)
- **Coverlet.collector 6.0.2** + **ReportGenerator** (medición de cobertura, ver sección 15)

## 5. Requisitos previos

- .NET 8 SDK instalado (`dotnet --version` debe reportar 8.x).
- PostgreSQL en ejecución (local o accesible por red) y una base de datos creada para el proyecto.
- (Opcional) Postman, para importar la colección de la sección 16.
- (Opcional) Newman, si se quiere ejecutar la colección de Postman por línea de comandos.

## 6. PostgreSQL

`HaciendaDbContext` (en `Bib_Hacienda.Infraestructura.Persistencia`) modela:

- `Potreros` (PK: `Identificacion`)
- `Reses` (TPH: Ternero/Cebon/Novillo, discriminador `TipoRes`, FK sombra `PotreroId`)
- `Vacunas` (TPH: Bacteriana/Viva, discriminador `TipoVacuna`, PK compuesta `Nombre`+`Lote`, FK sombra nullable `ResId`: `null` = disponible en inventario, no nulo = aplicada a esa Res)
- `Ventas` (PK sombra `Id`, FKs `PotreroId` y `ResId`)
- `Usuarios` (PK: `Nombre`)
- `EventosClinicos` (PK: `Id`, índice no único sobre `PotreroId`+`NombreRes`)

Se debe tener una instancia de PostgreSQL accesible y crear la base de datos indicada en la connection string (sección 8) antes de aplicar migraciones.

## 7. Configuración

Archivos relevantes en `Api_Hacienda/`:

- `appsettings.json`: connection string y sección `Jwt` (Issuer/Audience/ExpirationMinutes) por defecto.
- `appsettings.Development.json`: `Jwt:Key` de desarrollo local (clave aleatoria, **no es un secreto real ni debe usarse en producción**).

Para producción, `Jwt:Key` (y, si aplica, la connection string) deben provenir de **User Secrets, variables de entorno o un secret manager**, nunca quedar committeadas en el repositorio.

## 8. Connection String

Definida en `Api_Hacienda/appsettings.json`, clave `"HaciendaDatabase"`:

```json
"ConnectionStrings": {
  "HaciendaDatabase": "Host=localhost;Port=5432;Database=HaciendaDB;Username=postgres;Password=postgres"
}
```

Ajustar `Host`/`Port`/`Database`/`Username`/`Password` al entorno real antes de ejecutar.

## 9. Migraciones EF Core

**Estado actual: NO existen migraciones generadas.** La carpeta `Bib_Hacienda/Infraestructura/Persistencia/Migrations/` solo contiene `NOTA_MIGRACION.md`, documentando que en el entorno de desarrollo de este proyecto no hubo acceso de red a `api.nuget.org`, por lo que `dotnet restore`/`dotnet ef` no pudieron ejecutarse. **El entorno en el que se preparó esta FASE 5 tiene la misma limitación** (sin SDK de .NET instalado y sin salida de red hacia NuGet), así que tampoco fue posible generarlas aquí. No se fabricó ningún archivo de migración a mano.

Para generar y aplicar la migración inicial, ejecutar en un entorno con .NET 8 SDK y acceso a NuGet (por ejemplo, tu máquina Windows), desde la raíz de `Hacienda_TOBE_BACKEND/`:

```bash
dotnet tool install --global dotnet-ef   # si "dotnet ef" no está disponible aún
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate --project Bib_Hacienda --startup-project Api_Hacienda --output-dir Infraestructura/Persistencia/Migrations
dotnet ef database update --project Bib_Hacienda --startup-project Api_Hacienda
```

Estos comandos se verificaron contra la estructura real de la solución (nombres de proyecto `Bib_Hacienda`/`Api_Hacienda` tal como están declarados en `Hacienda_TOBE.sln`); no fueron ejecutados realmente en este entorno.

## 10. Ejecución

Desde la raíz de `Hacienda_TOBE_BACKEND/`:

```bash
dotnet restore
dotnet build
dotnet run --project Api_Hacienda
```

Perfiles disponibles (`Api_Hacienda/Properties/launchSettings.json`):

- `http`: `http://localhost:5175`
- `https`: `https://localhost:7092` (y `http://localhost:5175`)

Al iniciar en modo `Development`, el navegador se abre automáticamente en `/swagger`.

## 11. Autenticación JWT

- `POST /api/auth/login` (único endpoint `[AllowAnonymous]`) recibe `{ "nombre": "...", "contrasena": "..." }` y, si son válidas, responde un JWT (`LoginResponse`: `token`, `tokenType`, `expiresAt`, `nombre`, `rol`).
- El resto de los Controllers exige `[Authorize]`; `UsuariosController` exige además `[Authorize(Roles = "admin")]`.
- El proyecto no modela un `Rol` separado del nombre de usuario: el claim de rol emitido es el propio `usuario.Nombre` (mismo criterio que `ProveedorPermisosPorRol` ya usaba antes de esta fase).
- En Swagger UI: pegar únicamente el JWT (sin el prefijo `Bearer `) en el botón **Authorize**.
- Para llamadas HTTP directas: header `Authorization: Bearer <token>`.

## 12. Endpoints

14 endpoints reales, agrupados por Controller:

**AuthController** (`api/auth`)
| Verbo | Ruta | Auth |
|---|---|---|
| POST | `/api/auth/login` | Anónimo |

**PotrerosController** (`api/potreros`) — `[Authorize]`
| Verbo | Ruta |
|---|---|
| GET | `/api/potreros` |
| GET | `/api/potreros/{id}` |
| POST | `/api/potreros` |
| GET | `/api/potreros/{potreroId}/reses` |
| POST | `/api/potreros/{potreroId}/reses` |
| GET | `/api/potreros/{potreroId}/reses/{nombre}` |

**VacunasController** (`api/vacunas`) — `[Authorize]`
| Verbo | Ruta |
|---|---|
| GET | `/api/vacunas` |
| POST | `/api/vacunas` |

**VacunacionController** (`api/vacunacion`) — `[Authorize]`
| Verbo | Ruta |
|---|---|
| POST | `/api/vacunacion/aplicar` |

**VentasController** (`api`) — `[Authorize]`
| Verbo | Ruta |
|---|---|
| GET | `/api/ventas` (filtros opcionales `potreroId` **o** `desde`+`hasta`, mutuamente excluyentes) |
| POST | `/api/potreros/{potreroId}/reses/{nombre}/vender` |

**UsuariosController** (`api/usuarios`) — `[Authorize(Roles = "admin")]`
| Verbo | Ruta |
|---|---|
| GET | `/api/usuarios` |
| POST | `/api/usuarios` |

> **Historia Clínica**: `IRepositorioHistoriaClinica`, `RepositorioHistoriaClinicaEfCore` y `ServicioHistoriaClinica` existen en `Bib_Hacienda`, pero **no** están expuestos por ningún Controller ni registrados en el DI de `Api_Hacienda`. No se agregó un endpoint en esta fase (ver sección 17, Limitaciones conocidas).

## 13. Códigos HTTP

- `200 OK`: consultas exitosas y acciones que no crean un recurso nuevo (ej. `POST /api/vacunacion/aplicar`).
- `201 Created`: alta de un recurso nuevo (`POST /api/potreros`, `POST /api/potreros/{id}/reses`, `POST /api/vacunas`, `POST /api/usuarios`, `POST .../vender`).
- `400 Bad Request`: modelo de entrada estructuralmente inválido (DataAnnotations, sección 14) o fallo de una regla de negocio (`ResultadoOperacion.Fallo`).
- `401 Unauthorized`: sin token, token inválido/expirado, o credenciales incorrectas en login.
- `403 Forbidden`: token válido pero sin el rol requerido (`UsuariosController`).
- `404 Not Found`: recurso inexistente (potrero, res, vacuna por lote).
- `500 Internal Server Error`: excepción no controlada, capturada por `ManejadorGlobalExcepciones` y devuelta como `ProblemDetails` genérico (sin detalles internos).

`ResultadoOperacion` no distingue el tipo de fallo de negocio con un código propio (p. ej. "lote duplicado" vs. "parámetros inválidos" no son distinguibles sin analizar el texto del mensaje); por eso, de forma consistente en toda la API, todo fallo de negocio se traduce a `400 Bad Request` en vez de intentar inferir `409 Conflict` u otro código a partir del mensaje.

## 14. Validación de requests (FASE 5)

Se agregaron `DataAnnotations` **estrictamente estructurales** (`[Required]`, `[StringLength]`) a los DTOs de entrada (`LoginRequest`, `CrearUsuarioRequest`, `PotreroRequest`, `ResRequest`, `VacunaRequest`, `AplicarVacunaRequest`), alineadas con los límites de columna ya definidos en `HaciendaDbContext` (`HasMaxLength`). Gracias a `[ApiController]`, un modelo inválido produce automáticamente `400 Bad Request` antes de llegar al Controller.

**No** se duplicaron reglas de negocio ya implementadas en los validadores de dominio (`Bib_Hacienda/Dominio/Validacion/`):

- `Peso > 0` / `Edad > 0` (`ValidadorRes`) — no se agregó `[Range]` en `ResRequest`.
- `Monto > 0` (`ValidadorVenta`) — no se agregó `[Range]` en `VenderResRequest`.
- Grados de atenuación válidos (10/20/30, no un rango continuo) — se mantiene la verificación explícita con `Enum.IsDefined` en `VacunasController.Crear`, más precisa que un `[Range(10,30)]`.

## 15. Pruebas

```bash
dotnet test Bib_Hacienda.Tests/Bib_Hacienda.Tests.csproj
```

**Resultado confirmado (FASE 4C, sobre Windows, entorno con SDK/NuGet disponibles):**

- **203 tests, 203 exitosos, 0 fallidos, 0 omitidos.**

Esta fase (5) no agregó ni modificó tests: los cambios de código (middleware de excepciones, DataAnnotations) no alteran el comportamiento evaluado por la suite existente. **No se pudo re-ejecutar `dotnet test` en el entorno donde se preparó esta fase** (sin SDK .NET ni acceso a NuGet, ver sección 9); el resultado 203/203 corresponde a la última ejecución real confirmada en Windows (FASE 4C), no a una nueva corrida.

## 16. Cobertura

Medida con Coverlet + ReportGenerator (`medir_cobertura.sh`), sobre lógica de negocio (excluye infraestructura vía `coverlet.runsettings`):

- **Line coverage: 93.2%** (858/920 líneas)
- **Branch coverage: 96.4%** (245/254 ramas)

Supera el mínimo requerido del 80%. No se persiguió cobertura adicional en esta fase (instrucción explícita).

## 17. Postman

Archivos en `postman/`:

- `Hacienda_TOBE_Backend.postman_collection.json`
- `Hacienda_TOBE_Backend.postman_environment.json` ("Hacienda TO-BE Backend - Local")

Contienen únicamente los 14 endpoints reales listados en la sección 12, organizados en 7 carpetas (`01 - Autenticación` a `07 - Usuarios`), con casos positivos y negativos (credenciales incorrectas, datos inválidos, recurso inexistente, sin token, sin rol admin).

**Cómo usarla:**

1. Importar ambos archivos en Postman.
2. Seleccionar el environment "Hacienda TO-BE Backend - Local".
3. Ajustar `baseUrl` si la API corre en un puerto distinto a `https://localhost:7092`.
4. Ejecutar primero `01 - Autenticación > Login - Credenciales válidas` (reemplazando `nombre`/`contrasena` por un usuario real existente): el script de test guarda el JWT automáticamente en la variable `{{token}}`.
5. Ejecutar el resto de las carpetas; reutilizan `{{token}}`, `{{potreroId}}`, `{{nombreRes}}`, `{{loteVacuna}}`.



