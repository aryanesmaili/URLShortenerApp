using FluentValidation;
using SharedDataModels.Responses;
using System.Net;
using URLShortener.Application.Utility.Exceptions;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Middlewares;

public sealed class ExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");

            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ErrorType.Validation, errors: errors);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ErrorType.Argument, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, ErrorType.NotFound, ex.Message);
        }
        catch (InsufficientBalanceException ex)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ErrorType.InsufficientBalance, ex.Message);
        }
        catch (Exception ex)
        {
            // TODO: Add logger to detect problems. use "ex" param
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ErrorType.InternalError);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        ErrorType errorType,
        string? error = null,
        List<string>? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        APIResponse response = (errors is { Count: > 0 })
            ? CreateResult.Failure(errors)
            : CreateResult.Failure(errorType, error);

        await context.Response.WriteAsJsonAsync(response);
    }
}
