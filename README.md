# ap_todo

Full-stack Todo app with a .NET 10 backend (Postgres, GraphQL) and Angular frontend.

## Structure

- `BE/` — .NET Web API + GraphQL + Postgres (optional: .NET Aspire via `AppHost/`)
- `todo-app/` — Angular UI

## Backend quick start

```bash
cd BE
docker compose up -d   # Postgres
dotnet run             # API at http://localhost:5148
```

GraphQL endpoint: `http://localhost:5148/graphql` (Banana Cake Pop UI served at the same path in dev)

Run with Aspire (brings up API + infra):

```bash
cd AppHost
dotnet run
```

## Frontend quick start

```bash
cd todo-app
npm install
ng serve
```

UI: `http://localhost:4200` (points to the backend at `http://localhost:5148` by default).
