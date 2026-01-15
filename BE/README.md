# Todo Backend (.NET 9 + Postgres + GraphQL)

A minimal .NET 9 Web API for Todo items backed by PostgreSQL via EF Core.

## Prerequisites

- .NET SDK 9.x
- Docker Desktop (or compatible Docker engine)

## Run with Docker-compose

```bash
cd /Users/aleksejspiscevs/ap_todo/BE
docker compose up -d
dotnet run
```

Postgres runs on `localhost:5400`. Redis runs on `localhost:6379`.

Default API URL (dev): `http://localhost:5148`

## Run with .NET Aspire

```bash
cd /Users/aleksejspiscevs/ap_todo/AppHost
dotnet run
```

Aspire starts Postgres, Redis, the API, and the telemetry dashboard.

## GraphQL

- Endpoint: `http://localhost:5148/graphql`
- Banana Cake Pop IDE (Development): open `http://localhost:5148/graphql` in a browser (the tool is served at the same path)

### Sample queries

```graphql
query AllTodos($done: Boolean, $search: String) {
  todos(isCompleted: $done, search: $search) {
    id
    title
    isCompleted
    createdAt
    completedAt
  }
}

query SingleTodo($id: UUID!) {
  todo(id: $id) {
    id
    title
    isCompleted
  }
}
```

### Sample mutations

```graphql
mutation CreateTodo($input: TodoCreateInput!) {
  createTodo(input: $input) {
    id
    title
    isCompleted
  }
}

mutation UpdateTodo($id: UUID!, $input: TodoUpdateInput!) {
  updateTodo(id: $id, input: $input) {
    id
    title
    isCompleted
  }
}

mutation DeleteTodo($id: UUID!) {
  deleteTodo(id: $id)
}

mutation DeleteCompleted {
  deleteCompletedTodos
}
```

## Notes

- Connection string lives in `BE/appsettings.json`.
- Database schema is created automatically on startup.
