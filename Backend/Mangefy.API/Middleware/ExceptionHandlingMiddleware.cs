using FluentValidation;
using Mangefy.Application.Common.Exceptions;
using Mangefy.Domain.Common;
using System.Net;
using System.Text.Json;

namespace Mangefy.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Dados inválidos.",
                (object)ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })),

            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, (object?)null),
            ForbiddenException fe => (HttpStatusCode.Forbidden, fe.Message, (object?)null),
            ConflictException ce => (HttpStatusCode.Conflict, ce.Message, (object?)null),
            DomainException de => (HttpStatusCode.UnprocessableEntity, de.Message, (object?)null),

            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno.", (object?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { message, errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
