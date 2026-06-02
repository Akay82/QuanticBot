# Quantic Trader API

Layered .NET API for trading-bot management, paper trading, instruments, market data, and order execution with PostgreSQL.

## Projects

- `QuanticApi`: HTTP API, middleware, dependency injection, and configuration.
- `QuanticApi.Business`: contracts, domain models, repository interfaces, and business services.
- `QuanticApi.Data`: EF Core PostgreSQL context, repositories, table mappings, migrations, and procedure callers.
- `Database/Procedures`: PostgreSQL procedures used by transactional API actions.

## PostgreSQL Setup

Create a PostgreSQL database named `Quantic`, then replace the local placeholder password in `QuanticApi/appsettings.Development.json`.

For deployed environments, use an environment variable instead of committing credentials:

```powershell
$env:ConnectionStrings__TradingDatabase="Host=localhost;Port=5432;Database=Quantic;Username=postgres;Password=your-password"
```

Restore the local EF tool and apply the schema:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project QuanticApi.Data --startup-project QuanticApi
psql -U postgres -d Quantic -f Database/Procedures/001_order_workflow.sql
```

Run the API:

```powershell
dotnet run --project QuanticApi
```

If the tables were already created manually, do not apply `database update` against that database. Use migrations for a clean database or baseline the existing database first.

## Twelve Data Forex Charts

The backend fetches these five forex pairs from Twelve Data once every ten minutes:

- `EUR/USD`
- `GBP/USD`
- `USD/JPY`
- `AUD/USD`
- `USD/CAD`

Candles are saved to `quantic.price_history`. Angular reads the saved data from this API instead of calling Twelve Data directly.

Set the Twelve Data API key as an environment variable before starting the API:

```powershell
$env:TwelveData__ApiKey="your-twelve-data-api-key"
dotnet run --project QuanticApi
```

Do not commit the real key in `appsettings.json`.

Configuration is under the `TwelveData` section in `QuanticApi/appsettings.json`. Each refresh uses five Twelve Data requests, one for each forex pair. A ten-minute interval uses about `720` requests per day, which stays below the current free-plan allowance of `800` requests per day. The initial request stores four hours of one-minute candles, then later requests retrieve a twenty-candle overlap so retries remain safe.

Frontend chart APIs:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/market-data/forex/charts?timeframe=1m&candleCount=120` | Get all available forex charts |
| `GET` | `/api/market-data/forex/charts/{instrumentId}?timeframe=1m&candleCount=120` | Get one chart |
| `POST` | `/api/market-data/forex/refresh` | Trigger a manual Twelve Data refresh for setup checks |

`candleCount` is capped at `1000`.

Chart requests support these calculated timeframes:

```text
1m, 5m, 15m, 30m, 1h, 4h, 1d
```

Only one-minute candles are fetched from Twelve Data and stored in `quantic.price_history`. Larger chart timeframes are calculated by the backend from the saved one-minute candles, so chart filtering does not create extra Twelve Data requests.

Examples:

```text
GET /api/market-data/forex/charts?timeframe=5m&candleCount=120
GET /api/market-data/forex/charts?timeframe=15m&candleCount=120
```

## CRUD APIs

Every table below has `GET`, `GET /{id}`, `POST`, `PUT /{id}`, and `DELETE /{id}` endpoints:

| Resource | Route | List filters |
| --- | --- | --- |
| Users | `/api/users` | `email`, `isActive` |
| Instruments | `/api/instruments` | `symbol`, `marketType`, `exchange`, `isActive` |
| Paper accounts | `/api/paper-accounts` | `userId`, `currency`, `isActive` |
| Strategies | `/api/strategies` | `userId`, `strategyType`, `isActive` |
| Orders | `/api/orders` | `accountId`, `strategyId`, `instrumentId`, `status`, `side`, `orderType` |
| Positions | `/api/positions` | `accountId`, `instrumentId` |
| Price history | `/api/price-history` | `instrumentId`, `timeframe`, `from`, `to` |
| Signals | `/api/signals` | `strategyId`, `instrumentId`, `signalType`, `from`, `to` |
| Trades | `/api/trades` | `orderId`, `accountId`, `instrumentId`, `side`, `from`, `to` |
| Watchlists | `/api/watchlists` | `userId`, `instrumentId` |

List endpoints accept `pageNumber` and `pageSize`. The default page size is `20`, and the maximum is `100`.

Example:

```text
GET /api/orders?pageNumber=1&pageSize=25&accountId=10&status=PENDING
```

## Order Workflow

These actions use PostgreSQL procedures from `Database/Procedures`:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/orders/{id}/cancel` | Cancel a pending order |
| `POST` | `/api/orders/{id}/execute` | Execute an order and atomically update balances, positions, and trades |

Example execute body:

```json
{
  "executedPrice": 105.25
}
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
| `POST` | `/api/trading-bots/{id}/pause` | Pause the bot |
| `POST` | `/api/trading-bots/{id}/stop` | Alias for pause |
| `PUT` | `/api/trading-bots/{id}/settings` | Create or update swing strategy settings |
| `GET` | `/api/trading-bots/{id}/dashboard` | Get settings, managed position, and recent logs |
| `GET` | `/api/trading-bots/{id}/logs?count=100` | Poll recent bot logs |
| `GET` | `/api/trading-bots/{id}/logs/stream` | Subscribe to live bot logs with Server-Sent Events |
| `POST` | `/api/trading-bots/{id}/evaluate` | Evaluate one completed candle immediately for testing |

## EMA RSI Swing Bot

Run `Database/Scripts/002_ema_rsi_swing_bot.sql` in pgAdmin before using swing-bot endpoints.

The worker evaluates running bots every minute. It uses completed candles only and will not evaluate the same candle twice. The default paper-trading strategy is:

```text
Timeframe: 4h
Entry: EMA 50 > EMA 200, RSI 14 between 40 and 55, close > EMA 50
Exit: stop loss, take profit, RSI >= 70, or EMA 50 < EMA 200
Stop loss: entry - (ATR 14 * 1.5)
Take profit: entry + (ATR 14 * 3)
Position sizing: 1% risk, capped by bot allocation and paper-account balance
```

Create a bot:

```http
POST /api/trading-bots
Content-Type: application/json

{
  "name": "EUR USD Swing Bot",
  "symbol": "EUR/USD",
  "exchange": "TWELVE_DATA",
  "strategy": "EmaRsiSwingBot",
  "allocation": 1000
}
```

Configure it using an active paper `accountId` and forex `instrumentId`:

```http
PUT /api/trading-bots/{botId}/settings
Content-Type: application/json

{
  "accountId": 1,
  "instrumentId": 1,
  "timeframe": "4h",
  "fastEmaPeriod": 50,
  "slowEmaPeriod": 200,
  "rsiPeriod": 14,
  "atrPeriod": 14,
  "rsiEntryMin": 40,
  "rsiEntryMax": 55,
  "rsiExit": 70,
  "atrStopMultiplier": 1.5,
  "atrTakeProfitMultiplier": 3,
  "riskPercent": 1
}
```

Start it:

```http
POST /api/trading-bots/{botId}/start
```

For frontend updates, poll `/api/trading-bots/{botId}/dashboard` and subscribe to `/api/trading-bots/{botId}/logs/stream` with the browser `EventSource` API.

A real `4h` EMA-200 strategy requires at least 200 completed four-hour candles. For early paper tests with limited history, use a shorter timeframe and smaller EMA periods, then switch to the default swing settings after sufficient history is stored.

Use `QuanticApi/QuanticApi.http` to send sample requests from Visual Studio.
