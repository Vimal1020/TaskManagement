using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using TaskManagement.Core.Models.Responses;

namespace TaskManagement.API.Middleware
{
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception has occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var response = CreateErrorResponse(exception, statusCode);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ValidationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private ApiErrorResponse CreateErrorResponse(Exception exception, int statusCode)
        {
            var response = new ApiErrorResponse
            {
                Status = statusCode,
                Title = GetErrorTitle(statusCode),
                Detail = exception.Message
            };

            if (env.IsDevelopment())
            {
                response.Detail = exception.ToString();
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            return response;
        }

        private static string GetErrorTitle(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                500 => "Internal Server Error",
                _ => "An error occurred"
            };
        }
    }
}
