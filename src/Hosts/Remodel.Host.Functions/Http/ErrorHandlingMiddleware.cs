using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Remodel.Host.Functions.Http;

/// <summary>
/// The one place where exceptions from HTTP functions become responses.
/// Exceptions from queue functions are left alone: the Functions runtime retries the message
/// and finally moves it to the poison queue.
/// </summary>
internal sealed class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (context.GetHttpContext() is not null)
        {
            var problem = Problems.FromException(exception);
            if (problem.Status >= 500)
            {
                logger.LogError(exception, "{Function} failed", context.FunctionDefinition.Name);
            }

            // The function's result becomes the problem response, like a returned IActionResult.
            context.GetInvocationResult().Value = Problems.AsActionResult(problem);
        }
    }
}
