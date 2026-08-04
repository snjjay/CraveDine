using System.Net;
using System.Text.Json;
using EatKath.API.Exceptions;
using FluentValidation;

namespace EatKath.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";

            switch (ex)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = string.Join(" | ",
                        validationException.Errors.Select(e => e.ErrorMessage));
                    break;

                case DuplicateEntityException:
                    statusCode = HttpStatusCode.Conflict;
                    message = ex.Message;
                    break;

                case BusinessRuleException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}