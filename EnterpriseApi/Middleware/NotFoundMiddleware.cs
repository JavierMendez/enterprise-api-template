using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApi.Middleware;

public class NotFoundMiddleware(RequestDelegate next, ILogger<NotFoundMiddleware> logger)
{    
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            logger.LogWarning("Recurso no encontrado: {Path} | TraceId: {TraceId}", 
                context.Request.Path, 
                context.TraceIdentifier);
            
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = "La ruta solicitada no existe.",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

public static class NotFoundMiddlewareExtensions
{
    public static IApplicationBuilder UseNotFoundMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NotFoundMiddleware>();
    }
}