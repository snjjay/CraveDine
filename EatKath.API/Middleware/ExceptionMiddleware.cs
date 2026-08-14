using Azure;
using EatKath.API.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using System.Text.Json;

namespace EatKath.API.Middleware;


//This file is global error handler for EatKath
//Don't read it line by line. Think of it as one job:
//If something goes wrong anywhere in the API, catch the error, log it, and send a clean response to React
//How does it work at a high level? Request → try → error → catch → log → return proper response.
// ==========================================================
// Exception Middleware
// ==========================================================
//
// 🚨 Think: "The application's emergency error handler."
//
// Every request passes through here.
// If something goes wrong anywhere in the API:
//
//     Error occurs
//          ↓
//     Middleware catches it
//          ↓
//     Log the error 📝
//          ↓
//     Identify the error type 🔍
//          ↓
//     Choose HTTP status code
//          ↓
//     Send clean JSON response to React
//
// Examples:
// Validation error      → 400 Bad Request
// Duplicate entity      → 409 Conflict
// Business rule error   → 400 Bad Request
// Unknown error         → 500 Internal Server Error
//
// Goal:
// Don't let ugly technical exceptions reach the frontend.
// ==========================================================


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