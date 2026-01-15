using BE.Data;
using BE.GraphQL;
using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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
