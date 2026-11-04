using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;
using Dapper;

namespace Application.Todos.GetById;

internal sealed class GetTodoByIdQueryHandler(
    IDbConnectionFactory connectionFactory,
    IUserContext userContext,
    HybridCache cache)
    : IQueryHandler<GetTodoByIdQuery, TodoResponse>
{
    public async Task<Result<TodoResponse>> Handle(GetTodoByIdQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        TodoResponse? todo = await cache.GetOrCreateAsync(
            TodoCacheKeys.ById(userId, query.TodoItemId),
            async cancellation =>
            {
                using var connection = connectionFactory.CreateConnection();
                const string sql = @"SELECT Id, .... FROM X WHERE Id = @TodoItemId AND UserId = @UserId";

                return await connection.QuerySingleOrDefaultAsync<TodoResponse>
                (new CommandDefinition(
                    sql,
                    new {query.TodoItemId, UserId = userId},
                    cancellationToken: cancellation)
                );
            },
            cancellationToken: cancellationToken);

        if (todo is null)
        {
            return Result.Failure<TodoResponse>(TodoItemErrors.NotFound(query.TodoItemId));
        }

        return todo;
    }
}
