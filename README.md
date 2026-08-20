# Marketplace — Order Performance & Risk Analysis API

An ASP.NET Core Web API that turns raw marketplace order data into per-item profitability, and flags the orders that lose money, look mispriced, or carry thin margins.

## The problem it solves

A seller listing on several marketplaces (Trendyol, Hepsiburada, Amazon TR, N11, Çiçeksepeti) sees gross revenue in each platform's own panel, but not what is actually left after cost of goods, platform commission and shipping. An order can look healthy on turnover and still be a loss.

This API stores orders with their line items and computes, for every item:

```
netProfit = (salePrice − purchasePrice − salePrice × commissionRate / 100 − shippingCost) × quantity
```

The summary, the per-platform breakdown, the loss list, the daily trend and the risk classification are all derived from that single figure. The anomaly report is the one exception — it ignores profit entirely and looks at price consistency instead. See [Analysis model](#analysis-model) for the exact rules.

## Tech stack

| Layer | Choice | Version |
| --- | --- | --- |
| Runtime / TFM | .NET | `net10.0` |
| Web framework | ASP.NET Core Web API (controllers) | 10.x |
| Database | MongoDB | server-side; driver below |
| Data access | `MongoDB.Driver` | 3.7.0 |
| API docs (Swashbuckle) | `Swashbuckle.AspNetCore` | 10.1.4 |
| API docs (built-in) | `Microsoft.AspNetCore.OpenApi` | 10.0.2 |
| Language features | `Nullable: enable`, `ImplicitUsings: enable`, XML doc generation on | — |

No ORM, no repository abstraction, no external services. The only infrastructure dependency is a reachable MongoDB instance.

## Project layout

```
Marketplace/
├── Controllers/           HTTP surface — routing, status codes, XML docs for Swagger
│   ├── OrdersController.cs
│   ├── PlatformsController.cs
│   └── ReportsController.cs
├── Services/              Business logic + data access
│   ├── OrderAnalysisService.cs
│   ├── PlatformService.cs
│   └── DatabaseSeeder.cs
├── Models/                Persistence documents, DTOs and bound configuration
│   ├── Order.cs, OrderItem.cs, Platform.cs
│   ├── OrderCreateDto.cs, PlatformCreateDto.cs
│   └── DatabaseSettings.cs
├── Properties/
│   └── launchSettings.json
├── Program.cs             Composition root: DI, OpenAPI, pipeline, dev-only seeding
├── Marketplace.http       Ready-to-run requests for every endpoint
└── appsettings.json       DatabaseSettings section
```

**Why it is layered this way.** Three folders, one direction of dependency: `Controllers → Services → Models`. Controllers never touch the MongoDB driver, so the HTTP contract and the storage decision can move independently. Services own both the queries and the arithmetic, which keeps the profit formula in exactly one place (`CalculateNetProfit`) instead of spread across six report endpoints. Models split into two groups on purpose — `Order`/`Platform` carry the BSON attributes and are what the database sees, while `OrderCreateDto`/`PlatformCreateDto` are what clients are allowed to send, so a caller cannot set or overwrite a document `Id` through the API. A separate repository layer was deliberately left out: with one data store and no second consumer, it would add indirection without removing a dependency.

## API

Routes are case-insensitive. `OrdersController` and `PlatformsController` derive their route from the controller name; `ReportsController` overrides it with a fixed `api/report` — note the singular.

### Orders — `/api/Orders`

| Method | Route | Description | Input | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/api/Orders` | List every stored order | — | `200` array of orders |
| `GET` | `/api/Orders/{id}` | Fetch one order | `id` — Mongo ObjectId | `200` order · `404` |
| `POST` | `/api/Orders` | Create an order with its line items | `OrderCreateDto` body | `201` + `Location` header |
| `PUT` | `/api/Orders/{id}` | Replace platform, date and items of an order | `id`, `OrderCreateDto` body | `200` updated order · `404` |
| `DELETE` | `/api/Orders/{id}` | Delete an order permanently | `id` | `204` · `404` |

`OrderCreateDto`:

```jsonc
{
  "platformId": "60a7b1...",          // ObjectId of an existing Platform
  "date": "2024-02-01T12:00:00Z",
  "items": [
    {
      "name": "Kablosuz Kulaklık",
      "purchasePrice": 100.0,          // cost of goods, per unit
      "salePrice": 150.0,              // per unit
      "commissionRate": 12,            // percent (12 = 12%)
      "shippingCost": 10.0,            // per unit
      "quantity": 2
    }
  ]
}
```

### Platforms — `/api/Platforms`

| Method | Route | Description | Input | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/api/Platforms` | List every marketplace | — | `200` array |
| `GET` | `/api/Platforms/{id}` | Fetch one marketplace | `id` — Mongo ObjectId | `200` · `404` |
| `POST` | `/api/Platforms` | Register a marketplace | `{ "name": "Trendyol" }` | `201` + `Location` header |
| `PUT` | `/api/Platforms/{id}` | Rename a marketplace | `id`, `{ "name": "..." }` | `200` · `404` |
| `DELETE` | `/api/Platforms/{id}` | Delete a marketplace | `id` | `204` · `404` |

### Reports — `/api/report`

Every report accepts the same two optional query parameters, `startDate` and `endDate`, applied as an inclusive range on the order date and pushed down to MongoDB as a `$gte`/`$lte` filter. Omit both to analyse the whole dataset. All return `200`.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/report/summary` | Order count, unit count, total turnover, total net profit |
| `GET` | `/api/report/platform` | Per-platform turnover, net profit and profit margin |
| `GET` | `/api/report/loss` | Every line item whose net profit is negative |
| `GET` | `/api/report/anomaly` | Line items priced more than 50% away from the mean price of that product |
| `GET` | `/api/report/trend` | Turnover and net profit grouped by calendar day |
| `GET` | `/api/report/risk` | Every line item classified `Low` / `Medium` / `High` risk |

Example: `GET /api/report/summary?startDate=2024-01-01&endDate=2024-01-31`

## Analysis model

Every figure in every report is built from one per-item calculation.

**Net profit (per line item)** — `OrderAnalysisService.CalculateNetProfit`:

| Input | Where it comes from | Role |
| --- | --- | --- |
| `salePrice` | order item | revenue per unit |
| `purchasePrice` | order item | cost of goods per unit |
| `commissionRate` | order item | percentage the marketplace takes off `salePrice` |
| `shippingCost` | order item | shipping charged per unit |
| `quantity` | order item | multiplier for the whole per-unit result |

```
netProfit = (salePrice − purchasePrice − salePrice × commissionRate / 100 − shippingCost) × quantity
turnover  = salePrice × quantity
margin %  = turnover > 0 ? netProfit / turnover × 100 : 0
```

**How each report derives from it:**

- **Summary** — sums `quantity`, `turnover` and `netProfit` over every item in the filtered window, plus the order count.
- **Platform report** — groups orders by `platformId`, sums turnover and net profit per group, computes the margin, and resolves the display name from the `Platforms` collection (falling back to `"Unknown"` when the id no longer resolves).
- **Loss report** — flattens orders to line items and keeps those with `netProfit < 0`, returned with their order id, platform, date and the item itself. This is what catches an item that sells well but is priced under its true landed cost.
- **Anomaly report** — computes the mean `salePrice` per product *name* across the filtered window, then flags any line item whose deviation from that mean exceeds 50%: `|salePrice − mean| / mean × 100 > 50`. Intended to surface pricing mistakes and mis-keyed listings rather than genuine losses.
- **Trend report** — groups orders by calendar day (`date.Date`), ordered ascending, emitting daily turnover and daily net profit — the series you would plot to see margin eroding over time.
- **Risk report** — classifies every line item from its own margin:

  | Condition | Risk level |
  | --- | --- |
  | `netProfit < 0` **or** `margin < 5%` | `High` |
  | `5% ≤ margin ≤ 15%` | `Medium` |
  | `margin > 15%` | `Low` |

  So "risk" here means *margin fragility*: an item at 3% margin is one commission change or one return away from losing money, which is why it lands in the same bucket as an item already at a loss.

## Running it

### Prerequisites

- .NET SDK 10.0
- A MongoDB instance reachable at the configured connection string

### Steps

```bash
git clone https://github.com/imdecoder/Marketplace.git
cd Marketplace

dotnet restore
dotnet run
```

The app listens on `http://localhost:5190` (the `http` profile). For HTTPS on `https://localhost:7180`:

```bash
dotnet run --launch-profile https
```

### Swagger

In the **Development** environment the Swagger UI is served at the application root:

- Swagger UI → <http://localhost:5190/>
- Swashbuckle document → `http://localhost:5190/swagger/v1/swagger.json`
- Built-in OpenAPI document → `http://localhost:5190/openapi/v1.json`

The controllers' XML documentation comments are compiled into `Marketplace.xml` (`GenerateDocumentationFile` is enabled) and fed into Swagger, so each endpoint carries its description and a sample request.

Outside Development none of these are mapped — the API serves only its routes.

### Configuration

The `DatabaseSettings` section in `appsettings.json` is bound to `Models/DatabaseSettings.cs` via the options pattern:

```json
{
  "DatabaseSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MarketplaceDB",
    "OrdersCollectionName": "Orders",
    "PlatformsCollectionName": "Platforms"
  }
}
```

The committed default points at a local MongoDB with no credentials. **Never commit a connection string that contains a real username or password.** Override it out-of-band instead — environment variables use `__` as the section separator:

```bash
# bash
export DatabaseSettings__ConnectionString="mongodb://<host>:27017"

# PowerShell
$env:DatabaseSettings__ConnectionString = "mongodb://<host>:27017"
```

Or, for local development, use .NET user secrets (run `dotnet user-secrets init` once to add a `UserSecretsId` to the project first):

```bash
dotnet user-secrets set "DatabaseSettings:ConnectionString" "mongodb://<host>:27017"
```

### Seed data

On startup **in Development only**, `DatabaseSeeder` checks whether the `Orders` collection is empty. If it is, it inserts 5 platforms and 100 orders built from a catalogue of 20 products, spread over the previous 90 days, with roughly 15% of line items deliberately priced below cost so the loss, anomaly and risk reports have something to report. The generator is seeded with a fixed value (`new Random(42)`), so the dataset is identical on every machine. If the collection already holds orders the seeder returns immediately and touches nothing.

### Trying the endpoints

`Marketplace.http` contains a ready request for all sixteen endpoints and can be executed directly from Visual Studio or the VS Code REST Client extension.

## Design notes

- **Options pattern for configuration.** `DatabaseSettings` is bound once with `Services.Configure<T>` and injected as `IOptions<DatabaseSettings>`, so no service reads `IConfiguration` directly and the connection string never appears in a service body.
- **Services registered as singletons.** `OrderAnalysisService`, `PlatformService` and `DatabaseSeeder` hold nothing but `readonly IMongoCollection<T>` handles. The MongoDB driver's `MongoClient` is thread-safe and owns its own connection pool, so it is meant to be long-lived — creating one per request would create a new pool per request. There is no mutable instance state anywhere in these classes, which is what makes singleton lifetime safe here rather than merely convenient.
- **Separate DTOs for writes.** `POST` and `PUT` bind to `OrderCreateDto` / `PlatformCreateDto` rather than to the persistence models, so the client controls only the fields it should. `Id` is assigned by MongoDB and is invisible on the way in.
- **One profit formula.** `CalculateNetProfit` is a single private static method used by all six reports. Commission, shipping and quantity are applied in exactly one place, so a change to how commission is charged cannot silently disagree between the summary and the risk report.
- **Filter at the database, aggregate in memory.** The date range is translated into a MongoDB filter so the server never ships documents outside the window. The grouping and arithmetic then run in LINQ over the returned list. For a dataset of this size that is the simpler and more readable choice; a materially larger dataset would move the grouping into an aggregation pipeline instead.
- **Deterministic, environment-gated seeding.** A fixed random seed makes the demo dataset reproducible, and the seeder is guarded by `IsDevelopment()` so it can never write sample data into a real environment.

## Known limitations

Issues identified in the current code, recorded here with their impact and the intended resolution.

**Malformed ids return `500` instead of `404`.** Route `{id}` values are passed straight to the MongoDB driver. `Order.Id`, `Order.PlatformId` and `Platform.Id` are declared with `[BsonRepresentation(BsonType.ObjectId)]`, so the driver parses the string while translating the LINQ filter — a value that is not a 24-character hex string raises `FormatException` before any query is issued, and nothing catches it. *Impact:* six endpoints are affected — `GET`, `PUT` and `DELETE` by id on both `OrdersController` and `PlatformsController` — and each reports a server fault for what is a client input error, which also makes the two cases indistinguishable to a caller. `POST /api/Orders` fails the same way when `platformId` is well-formed JSON but not a valid ObjectId. *Resolution:* guard with `ObjectId.TryParse` and return `NotFound()` for unparseable ids; as the number of id-bearing endpoints grows, a custom model binder or a route constraint is the cleaner form, since it rejects the request before the action executes instead of repeating the same check in every method.

**Duplicate OpenAPI registration.** `Program.cs` registers both the built-in .NET pipeline (`AddOpenApi` / `MapOpenApi`) and Swashbuckle (`AddSwaggerGen` / `UseSwagger` / `UseSwaggerUI`). Both describe the same controllers. *Impact:* two documents are published for one API — `/openapi/v1.json` and `/swagger/v1/swagger.json` — leaving no single source of truth for client generation, and two packages are carried where one would do. *Resolution:* drop one registration. Removing Swashbuckle sheds the larger dependency, at the cost of the XML-comment integration (`IncludeXmlComments` has no built-in equivalent) and the Swagger UI served at the application root, which would then need a separate UI package. Removing `AddOpenApi` / `MapOpenApi` is the smaller change and preserves both. Note that `Microsoft.OpenApi` is reached through *both* paths, so either removal on its own leaves that dependency in the graph.

**Transitive package vulnerabilities.** `dotnet list package --vulnerable --include-transitive` reports three advisories, none of them in a directly referenced package:

| Package | Resolved | Severity | Reached through | Advisory |
| --- | --- | --- | --- | --- |
| `Microsoft.OpenApi` | 2.4.1 | High | `Microsoft.AspNetCore.OpenApi` 10.0.2 **and** `Swashbuckle.AspNetCore` 10.1.4 | [GHSA-v5pm-xwqc-g5wc](https://github.com/advisories/GHSA-v5pm-xwqc-g5wc) |
| `Snappier` | 1.0.0 | High | `MongoDB.Driver` 3.7.0 | [GHSA-pggp-6c3x-2xmx](https://github.com/advisories/GHSA-pggp-6c3x-2xmx) |
| `SharpCompress` | 0.30.1 | Moderate | `MongoDB.Driver` 3.7.0 | [GHSA-6c8g-7p36-r338](https://github.com/advisories/GHSA-6c8g-7p36-r338) |

*Impact:* the advisories sit in code the application loads at runtime, and the build reports them as `NU1902`/`NU1903` warnings rather than errors, so they do not block anything today. *Resolution:* two routes, in order of preference — raise the direct dependency (`MongoDB.Driver`, `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`) to a release whose graph resolves the advisory; or, where no such release exists yet, add an explicit top-level `PackageReference` at a fixed version, since a direct reference takes precedence over the transitive one. The fixed versions are listed in each advisory above. This scan belongs in CI with `TreatWarningsAsErrors` scoped to `NU1901-NU1904`, so a newly published advisory fails the build instead of waiting to be noticed in local output.

**Each service constructs its own `MongoClient`.** `OrderAnalysisService`, `PlatformService` and `DatabaseSeeder` each call `new MongoClient(...)` in their constructor, so the application holds three connection pools where one would do. *Impact:* wasteful but bounded today, because all three services are singletons and therefore so are their pools. The real risk is that the client's lifetime is invisible in the DI graph — if the services were later changed to `AddScoped`, which looks like a reasonable refactor, every request would open a fresh pool and exhaust sockets under load. *Resolution:* register `IMongoClient` once as a singleton in `Program.cs` and inject it, so the pool's lifetime lives at the composition root and cannot be broken by changing a service registration. This is the same class of mistake as constructing `HttpClient` per request instead of using `IHttpClientFactory`.

**Authorization middleware is registered but inert.** `Program.cs` calls `UseAuthorization()` without `UseAuthentication()`, and no controller carries `[Authorize]`. Authorization cannot enforce anything without an authenticated principal, so every endpoint — including both `DELETE` routes — is open. *Impact:* a reader scanning the pipeline sees `UseAuthorization()` and may reasonably conclude the API is protected, which makes a dead call worse than no call at all. Anonymous access is an accepted trade-off for a demo running against local seed data; the misleading line is not. *Resolution:* add JWT bearer authentication with `UseAuthentication()` placed *before* `UseAuthorization()` (the order is silently significant), and apply `[Authorize]` as a global filter with `[AllowAnonymous]` for the exceptions, so newly added endpoints default to closed.

## Roadmap

- **Unit tests** — starting with `CalculateNetProfit` and the risk classification boundaries (0%, 5%, 15%), then controller-level tests against a mocked service.
- **Docker Compose** — one file bringing up the API together with MongoDB, so the project runs with a single command and no local MongoDB install.
- **Structured logging with a correlation ID** — a middleware assigning a request id, flowed through the log scope so a single report request can be traced end to end.
- **Idempotency keys on write endpoints** — an `Idempotency-Key` header on `POST`/`PUT` so a retried order creation cannot produce a duplicate document.

---

**Emin Arif Pirinç**

- GitHub — [github.com/imdecoder](https://github.com/imdecoder)
- LinkedIn — [linkedin.com/in/eminarif](https://www.linkedin.com/in/eminarif)
