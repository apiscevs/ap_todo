using BE.Data;
using BE.GraphQL;
using BE.Models;
using HotChocolate.AspNetCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net.Sockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var todoDbConnection = builder.Configuration.GetConnectionString("TodoDb")
    ?? builder.Configuration.GetConnectionString("todo_db");
if (string.IsNullOrWhiteSpace(todoDbConnection))
{
    throw new InvalidOperationException("TodoDb connection string is missing.");
}

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(todoDbConnection));

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? builder.Configuration.GetConnectionString("redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddGraphQLServer()
    .AddQueryType<TodoQueries>()
    .AddMutationType<TodoMutations>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseCors("ui");

app.MapDefaultEndpoints();
var graphQlEndpoint = app.MapGraphQL("/graphql");
graphQlEndpoint.WithOptions(new GraphQLServerOptions
{
    Tool =
    {
        Enable = app.Environment.IsDevelopment(),
        ServeMode = GraphQLToolServeMode.Embedded,
        GraphQLEndpoint = "/graphql"
    }
});

await EnsureDatabaseReadyAsync(app.Services, app.Logger, app.Lifetime.ApplicationStopping);

var todos = app.MapGroup("/api/todos");

var cacheOptions = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
};
var cacheJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
const string TodosCacheKey = CacheKeys.Todos;

todos.MapGet("/", async (TodoDbContext db, IDistributedCache cache) =>
{
    var cached = await cache.GetStringAsync(TodosCacheKey);
    if (!string.IsNullOrWhiteSpace(cached))
    {
        var cachedTodos = JsonSerializer.Deserialize<List<TodoItem>>(cached, cacheJsonOptions);
        if (cachedTodos is not null)
        {
            return Results.Ok(cachedTodos);
        }
    }

    var items = await db.Todos.OrderByDescending(todo => todo.CreatedAt).ToListAsync();
    await cache.SetStringAsync(TodosCacheKey, JsonSerializer.Serialize(items, cacheJsonOptions), cacheOptions);
    return Results.Ok(items);
});

todos.MapGet("/{id:guid}", async (Guid id, TodoDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

todos.MapPost("/", async (TodoCreateRequest request, TodoDbContext db, IDistributedCache cache) =>
{
    var todo = new TodoItem
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        IsCompleted = request.IsCompleted,
        CreatedAt = DateTime.UtcNow,
        CompletedAt = request.IsCompleted ? DateTime.UtcNow : null
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    await cache.RemoveAsync(TodosCacheKey);
    return Results.Created($"/api/todos/{todo.Id}", todo);
});

todos.MapPut("/{id:guid}", async (Guid id, TodoUpdateRequest request, TodoDbContext db, IDistributedCache cache) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    if (!string.IsNullOrWhiteSpace(request.Title))
    {
        todo.Title = request.Title.Trim();
    }

    if (request.IsCompleted.HasValue)
    {
        todo.IsCompleted = request.IsCompleted.Value;
        todo.CompletedAt = request.IsCompleted.Value ? DateTime.UtcNow : null;
    }

    await db.SaveChangesAsync();
    await cache.RemoveAsync(TodosCacheKey);
    return Results.Ok(todo);
});

todos.MapDelete("/{id:guid}", async (Guid id, TodoDbContext db, IDistributedCache cache) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    await cache.RemoveAsync(TodosCacheKey);
    return Results.NoContent();
});

todos.MapDelete("/completed", async (TodoDbContext db, IDistributedCache cache) =>
{
    var completed = await db.Todos.Where(todo => todo.IsCompleted).ToListAsync();
    if (completed.Count == 0)
    {
        return Results.NoContent();
    }

    db.Todos.RemoveRange(completed);
    await db.SaveChangesAsync();
    await cache.RemoveAsync(TodosCacheKey);
    return Results.NoContent();
});

app.Run();

static async Task EnsureDatabaseReadyAsync(
    IServiceProvider services,
    ILogger logger,
    CancellationToken cancellationToken)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(1);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }
        catch (Exception ex) when (IsTransientDatabaseException(ex))
        {
            logger.LogWarning(
                ex,
                "Database not ready yet (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}s.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            if (attempt == maxAttempts)
            {
                logger.LogWarning("Database initialization skipped after retries.");
                return;
            }

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
        }
    }
}

static bool IsTransientDatabaseException(Exception ex)
{
    if (ex is NpgsqlException)
    {
        return true;
    }

    var baseException = ex.GetBaseException();
    return baseException is SocketException;
}

record TodoCreateRequest(string Title, bool IsCompleted);

record TodoUpdateRequest(string? Title, bool? IsCompleted);
