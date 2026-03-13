using System.Net;
using System.Text.Json;
using DataSneeq.Application.DTOs;
using Npgsql;

namespace DataSneeq.Api.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error: {SqlState}", ex.SqlState);
            var (message, code) = TranslatePostgresError(ex);
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, message, code, new { table = ex.TableName, column = ex.ColumnName });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation/operation error");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message, "INVALID_OPERATION");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message, "INVALID_ARGUMENT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", "INTERNAL_ERROR");
        }
    }

    private static (string Message, string Code) TranslatePostgresError(PostgresException ex)
    {
        var table = ex.TableName ?? "unknown";
        var column = ex.ColumnName ?? "unknown";

        return ex.SqlState switch
        {
            "23502" => ($"Column \"{column}\" in table \"{table}\" is required but was not provided.", "NOT_NULL_VIOLATION"),
            "23503" => ("A referenced record does not exist. Check that the value exists in the related table.", "FOREIGN_KEY_VIOLATION"),
            "23505" => ($"A record with this value already exists. Duplicate key in table \"{table}\".", "UNIQUE_VIOLATION"),
            "23514" => ("The value does not meet the check constraint.", "CHECK_VIOLATION"),
            "42P01" => ("Table does not exist.", "UNDEFINED_TABLE"),
            "3D000" => ("Database does not exist.", "INVALID_CATALOG_NAME"),
            "28P01" => ("Invalid connection: username or password incorrect.", "INVALID_PASSWORD"),
            "08006" or "08000" or "08003" or "08004" or "08007" => ("Could not connect to the database. Check host, port, and network.", "CONNECTION_FAILURE"),
            _ => (ex.Message, "DATABASE_ERROR")
        };
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message, string? code = null, object? details = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Error = message,
            Code = code,
            Details = details
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = response.Error, code = response.Code, details = response.Details }));
    }
}
