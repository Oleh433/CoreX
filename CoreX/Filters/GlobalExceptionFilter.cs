using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreX.UI.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var (status, title) = context.Exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Operation not allowed"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            if (status >= 500)
                _logger.LogError(context.Exception, "Unhandled exception");
            else
                _logger.LogWarning(context.Exception, "{Title}", title);

            var detail = status >= 500 ? null : context.Exception.Message;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            };

            context.Result = new ObjectResult(problem) { StatusCode = status };
            context.ExceptionHandled = true;
        }
    }
}
