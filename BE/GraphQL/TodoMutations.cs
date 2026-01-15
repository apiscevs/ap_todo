using BE.Data;
using BE.Models;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BE.GraphQL;

public class TodoMutations
{
    public async Task<TodoItem> CreateTodo(
        TodoCreateInput input,
        [Service] TodoDbContext db,
        [Service] IDistributedCache cache)
    {
        var todo = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            IsCompleted = input.IsCompleted ?? false,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = input.IsCompleted == true ? DateTime.UtcNow : null
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.Todos);

        return todo;
    }

    public async Task<TodoItem> UpdateTodo(
        [GraphQLType(typeof(UuidType))] Guid id,
        TodoUpdateInput input,
        [Service] TodoDbContext db,
        [Service] IDistributedCache cache)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(todo => todo.Id == id);
        if (todo is null)
        {
            throw CreateNotFoundError(id);
        }

        if (!string.IsNullOrWhiteSpace(input.Title))
        {
            todo.Title = input.Title.Trim();
        }

        if (input.IsCompleted.HasValue)
        {
            todo.IsCompleted = input.IsCompleted.Value;
            todo.CompletedAt = input.IsCompleted.Value ? DateTime.UtcNow : null;
        }

        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.Todos);
        return todo;
    }

    public async Task<bool> DeleteTodo(
        [GraphQLType(typeof(UuidType))] Guid id,
        [Service] TodoDbContext db,
        [Service] IDistributedCache cache)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(todo => todo.Id == id);
        if (todo is null)
        {
            throw CreateNotFoundError(id);
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.Todos);
        return true;
    }

    public async Task<int> DeleteCompletedTodos(
        [Service] TodoDbContext db,
        [Service] IDistributedCache cache)
    {
        var completed = await db.Todos.Where(todo => todo.IsCompleted).ToListAsync();
        if (completed.Count == 0)
        {
            return 0;
        }

        db.Todos.RemoveRange(completed);
        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.Todos);
        return completed.Count;
    }

    private static GraphQLException CreateNotFoundError(Guid id) =>
        new(ErrorBuilder.New()
            .SetMessage($"Todo '{id}' was not found.")
            .SetCode("TODO_NOT_FOUND")
            .Build());
}
