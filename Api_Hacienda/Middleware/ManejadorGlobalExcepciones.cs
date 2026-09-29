using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api_Hacienda.Middleware
{
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
            logger.LogError(
                exception,
                "Excepción no controlada procesando {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado al procesar la solicitud. " +
                          "Intente nuevamente más tarde o contacte al administrador.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
