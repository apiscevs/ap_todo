# ap_todo

Full-stack Todo app with a .NET 9 backend (Postgres/Redis, REST + GraphQL) and Angular frontend.

## Structure

- `BE/` — .NET Web API + GraphQL + Postgres (optional: .NET Aspire via `AppHost/`)
- `todo-app/` — Angular UI

## Backend quick start

```bash
cd BE
docker compose up -d   # Postgres + Redis
dotnet run             # API at http://localhost:5148
```

GraphQL endpoint: `http://localhost:5148/graphql`  
Banana Cake Pop (dev): `http://localhost:5148/graphql/ui`  
REST endpoints remain for legacy clients at `/api/todos`.

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
