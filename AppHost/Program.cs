using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", port: 5400)
    .WithDataVolume("ap_todo_postgres");
var todoDb = postgres.AddDatabase("TodoDb");
var redis = builder.AddRedis("redis");

builder.AddProject<Projects.BE>("be")
    .WithReference(todoDb)
    .WithReference(redis)
    ;

builder.Build().Run();
