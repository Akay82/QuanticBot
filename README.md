# Quantic Trader API

Layered .NET API baseline for managing trading bots with PostgreSQL.

## Projects

- `QuanticApi`: HTTP API, middleware, dependency injection, and configuration.
- `QuanticApi.Business`: trading-bot contracts, domain model, repository interface, and business rules.
- `QuanticApi.Data`: EF Core PostgreSQL context, repository implementation, entity configuration, and migrations.

## PostgreSQL Setup

Create a PostgreSQL database named `quantic_trader`, then replace the local placeholder password in `QuanticApi/appsettings.Development.json`.

For deployed environments, use an environment variable instead of committing credentials:

```powershell
$env:ConnectionStrings__TradingDatabase="Host=localhost;Port=5432;Database=quantic_trader;Username=postgres;Password=your-password"
```

Restore the local EF tool and apply the schema:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project QuanticApi.Data --startup-project QuanticApi
```

Run the API:

```powershell
dotnet run --project QuanticApi
```

## Trading Bot APIs

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/health` | API health check |
| `GET` | `/api/trading-bots` | List bots |
| `GET` | `/api/trading-bots/{id}` | Get one bot |
| `POST` | `/api/trading-bots` | Create a bot |
| `PUT` | `/api/trading-bots/{id}` | Update a stopped bot |
| `DELETE` | `/api/trading-bots/{id}` | Delete a stopped bot |
| `POST` | `/api/trading-bots/{id}/start` | Mark a bot as running |
| `POST` | `/api/trading-bots/{id}/stop` | Mark a bot as stopped |

Use `QuanticApi/QuanticApi.http` to send sample requests from Visual Studio.
