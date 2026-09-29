using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api_Hacienda.Middleware
{
    // FASE 5, OBJETIVO 1: manejo global de excepciones.
    //
    // Usa el mecanismo NATIVO de ASP.NET Core 8 (IExceptionHandler +
    // builder.Services.AddExceptionHandler<T>() + app.UseExceptionHandler(),
    // ver Program.cs), en vez de un middleware manual escrito a mano, tal
    // como pide el encargo ("preferiblemente utilizar el mecanismo nativo").
    //
    // Alcance explícito de esta clase: SOLO captura excepciones NO
    // controladas (errores inesperados: fallo de conexión a PostgreSQL,
    // NullReferenceException no prevista, etc.). Los 400/404/401/403 que ya
    // devuelven los Controllers explícitamente (BadRequest/NotFound/
    // Unauthorized, ver AuthController/PotrerosController/etc.) NUNCA pasan
    // por aquí: esos son ActionResult normales, no excepciones, así que este
    // manejador no los toca ni los reinterpreta (el encargo prohíbe
    // convertir 400/404 existentes en 500).
    //
    // Todas las excepciones no controladas se traducen a 500 con un
    // ProblemDetails genérico: no se expone stack trace, connection string,
    // clave JWT ni ningún detalle interno de PostgreSQL/EF Core en la
    // respuesta HTTP. El detalle real de la excepción solo se escribe en el
    // log del servidor (ILogger), nunca en el body de la respuesta.
    public class ManejadorGlobalExcepciones : IExceptionHandler
    {
        private readonly ILogger<ManejadorGlobalExcepciones> logger;

        public ManejadorGlobalExcepciones(ILogger<ManejadorGlobalExcepciones> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // El detalle completo (incluye stack trace) solo va al log del
            // servidor, nunca a la respuesta HTTP.
            logger.LogError(
                exception,
                "Excepción no controlada procesando {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                // Mensaje genérico e intencionalmente sin detalle técnico:
                // NO se usa exception.Message aquí porque mensajes de EF
                // Core/Npgsql (p. ej. errores de conexión) pueden incluir
                // fragmentos de la connection string o del esquema real de
                // la base de datos.
                Detail = "Ocurrió un error inesperado al procesar la solicitud. " +
                          "Intente nuevamente más tarde o contacte al administrador.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // true = "ya manejé esta excepción, no la vuelvas a propagar".
            return true;
        }
    }
}
