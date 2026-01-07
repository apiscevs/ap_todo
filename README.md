# ap_todo

Full-stack Todo app with a .NET 9 backend and Angular frontend.

## Structure

- `BE/` — .NET Web API + Postgres (optional: .NET Aspire via `AppHost/`)
- `todo-app/` — Angular UI

## Quick start

Backend (requires Docker for Postgres/Redis):

```bash
cd BE
docker compose up -d
dotnet run
```

Frontend:

```bash
cd todo-app
npm install
ng serve
```

Open `http://localhost:4200` (UI) and `http://localhost:5148` (API default).
