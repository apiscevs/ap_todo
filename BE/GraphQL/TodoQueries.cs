using BE.Data;
using BE.Models;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

namespace BE.GraphQL;

public class TodoQueries
{
    public async Task<IReadOnlyList<TodoItem>> GetTodos(
        [Service] TodoDbContext db,
        bool? isCompleted,
        string? search)
    {
        var query = db.Todos.AsNoTracking().AsQueryable();

        if (isCompleted.HasValue)
        {
            query = query.Where(todo => todo.IsCompleted == isCompleted.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(todo => EF.Functions.ILike(todo.Title, $"%{search.Trim()}%"));
        }

        return await query
            .OrderByDescending(todo => todo.CreatedAt)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetTodo(
        Guid id,
        [Service] TodoDbContext db)
    {
        return await db.Todos.AsNoTracking().FirstOrDefaultAsync(todo => todo.Id == id);
    }
}
