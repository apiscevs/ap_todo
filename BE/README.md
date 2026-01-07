# Todo Backend (.NET 9 + Postgres)

A minimal .NET 9 Web API for Todo items backed by PostgreSQL via EF Core.

## Prerequisites

- .NET SDK 9.x
- Docker Desktop (or compatible Docker engine)

## Run Postgres

```bash
cd /Users/aleksejspiscevs/ap_todo/BE
docker compose up -d
```

Postgres runs on `localhost:5400`. Redis runs on `localhost:6379`.

## Run the API

```bash
cd /Users/aleksejspiscevs/ap_todo/BE
dotnet run
```

Default URL (dev): `http://localhost:5148`

## Run with .NET Aspire

```bash
cd /Users/aleksejspiscevs/ap_todo/AppHost
dotnet run
```

Aspire will start Postgres, Redis, the API, and the telemetry dashboard.

## Endpoints

- `GET /api/todos`
- `GET /api/todos/{id}`
- `POST /api/todos`
- `PUT /api/todos/{id}`
- `DELETE /api/todos`
- `DELETE /api/todos/completed`

### Request examples

```bash
curl -X POST http://localhost:5148/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Ship the backend","isCompleted":false}'
```

## Notes

- Connection string lives in `BE/appsettings.json`.
- Database schema is created automatically on startup.
