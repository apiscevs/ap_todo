# Todo Backend (.NET 9 + Postgres + GraphQL)

A minimal .NET 9 Web API for Todo items backed by PostgreSQL via EF Core. REST endpoints remain available for legacy clients; GraphQL is the primary API going forward.

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

## GraphQL (preferred)

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

query SingleTodo($id: ID!) {
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

mutation UpdateTodo($id: ID!, $input: TodoUpdateInput!) {
  updateTodo(id: $id, input: $input) {
    id
    title
    isCompleted
  }
}

mutation DeleteTodo($id: ID!) {
  deleteTodo(id: $id)
}

mutation DeleteCompleted {
  deleteCompletedTodos
}
```

## REST (legacy)

- `GET /api/todos`
- `GET /api/todos/{id}`
- `POST /api/todos`
- `PUT /api/todos/{id}`
- `DELETE /api/todos`
- `DELETE /api/todos/completed`

### Request example

```bash
curl -X POST http://localhost:5148/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Ship the backend","isCompleted":false}'
```

## Notes

- Connection string lives in `BE/appsettings.json`.
- Database schema is created automatically on startup.
