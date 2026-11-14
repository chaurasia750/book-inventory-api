using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BookInventory.BuildingBlocks.Configuration.Filters
{
    /// <summary>
    /// Runs each write request (POST/PUT/PATCH/DELETE) in one database transaction: committed when the action
    /// succeeds, rolled back on an exception or an error status (4xx/5xx). Read requests do not open a transaction.
    /// An action marked <see cref="PersistOnFailureAttribute"/> is committed even when it returns an error status.
    /// </summary>
    public class TransactionFilter(IUnitOfWork unitOfWork) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
            {
                await next();
                return;
            }

            var persistOnFailure = context.ActionDescriptor.EndpointMetadata.OfType<PersistOnFailureAttribute>().Any();

            await unitOfWork.StartTransactionAsync(context.HttpContext.RequestAborted);

            ActionExecutedContext executed;
            try
            {
                executed = await next();
            }
            catch
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
                throw;
            }

            var threw = executed.Exception is not null && !executed.ExceptionHandled;
            var errorStatus = executed.Result is IStatusCodeActionResult { StatusCode: >= 400 };

            if (threw || (errorStatus && !persistOnFailure))
                await unitOfWork.RollbackAsync(CancellationToken.None);
            else
                await unitOfWork.CommitAsync(CancellationToken.None);
        }
    }
}
