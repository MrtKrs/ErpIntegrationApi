using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Todos.Update;

internal sealed class UpdateTodoCommandHandler(
    IDbConnectionFactory connectionFactory,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<UpdateTodoCommand>
{
    public async Task<Result> Handle(UpdateTodoCommand command, CancellationToken cancellationToken)
    {
        _ = connectionFactory;
        _ = cache;
        _ = userContext;
        throw new NotImplementedException();
        // TodoItem? todoItem = await context.TodoItems
        //     .SingleOrDefaultAsync(
        //         t => t.Id == command.TodoItemId && t.UserId == userContext.UserId,
        //         cancellationToken);

        // if (todoItem is null)
        // {
        //     return Result.Failure(TodoItemErrors.NotFound(command.TodoItemId));
        // }

        // todoItem.Description = command.Description;

        // todoItem.Raise(new TodoItemUpdatedDomainEvent(todoItem.Id));

        // await context.SaveChangesAsync(cancellationToken);

        // await cache.RemoveAsync(TodoCacheKeys.ById(todoItem.UserId, todoItem.Id), cancellationToken);

        // return Result.Success();
    }
}
