# ErpIntegrationApi

REST API integration layer for an external SQL Server ERP database, built on [Milan Jovanović's Clean Architecture template](https://milanjovanovic.tech/templates/clean-architecture).

Built as a proof-of-concept REST API integration layer during a short placement (virksomhedspraktik).

## Architecture & Key Design Decisions

Because the ERP desktop application owns the database schema and manages migrations directly, this API acts as a lightweight integration layer over the existing tables:

- **Dapper instead of EF Core:** Provides explicit control over SQL queries and selects only the specific columns needed by each API endpoint from wide ERP tables (tblVarer, tblSalgsBilag), avoiding ORM change-tracking overhead or conflicting schema migrations.
- **API-Key Authentication & Tenant Routing:** Replaced user logins and JWTs with `Authorization: Api-Key <key>`. Validating a key resolves both the client's permissions (`products:read`, `orders:read`, etc.) and the target ERP database (`InitialCatalog`) on the SQL Server instance via `ITenantContext`.
- **No Domain Events or HybridCache:** Removed so the API always reads live data directly from the ERP database, ensuring changes made in the desktop application are immediately reflected in API responses.
- **MediatR-Free CQRS + Decorators:** Uses lightweight `ICommandHandler` / `IQueryHandler` abstractions wrapped in logging and FluentValidation decorators (validating incoming API payloads before executing queries against the ERP database).

## Solution Structure

- **`src/SharedKernel`** — Core primitives (`Result`, `Error`, `ValidationError`).
- **`src/Domain`** — Domain entities (`Product`, `Order`) and domain error definitions.
- **`src/Application`** — Use cases (`GetProducts`, `UpdateProduct`, `GetOrders`), shared pagination/SQL helpers (`Application.Abstractions.Pagination`), and FluentValidation validators.
- **`src/Infrastructure`** — Dapper `IDbConnectionFactory`, `ITenantContext` database switching, `ApiKeyAuthHandler`, permission authorization, and Serilog/OpenTelemetry setup.
- **`src/Web.Api`** — Minimal API endpoints, global exception handling (`ProblemDetails`), rate limiting, and Scalar OpenAPI UI.
- **`tests/`** — `ArchitectureTests` (layer dependency rules), `Application.UnitTests`, and `IntegrationTests` (Testcontainers with isolated SQL Server databases).

## Getting Started

### 1. Configure the SQL Server Connection
Set the base SQL Server connection string pointing to the central Auth database (the API uses this default catalog to validate API keys, then dynamically switches InitialCatalog to the key's assigned ERP database).

```bash
dotnet user-secrets set "ConnectionStrings:Database" "Server=...;Database=<AuthDatabaseName>;User Id=...;Password=...;TrustServerCertificate=True;" --project src/Web.Api
```

### 2. Start Observability Stack & Run the API
```bash
docker compose up -d        # Starts Grafana + Loki + Tempo + Prometheus (and PostgreSQL if still defined in compose)
dotnet run --project src/Web.Api
```

- **Scalar API Reference:** `http://localhost:5000/scalar/`
- **Grafana Telemetry UI:** `http://localhost:3000` (logs, traces, and metrics shipped via OTLP)

### 3. Provisioning & Authenticating API Keys
Endpoints require an `Authorization` header using the `Api-Key` scheme:
```http
Authorization: Api-Key <your-api-key>
```

API clients and keys are provisioned directly in the central Auth database (`dbo.tblApiClient` and `dbo.tblApiKey`). 


<details>
<summary>Auth Database Schema (SQL Server DDL)</summary>

```sql
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'tblApiClient' AND xtype='U')
BEGIN
    CREATE TABLE dbo.[tblApiClient] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [clientname] NVARCHAR(100) NOT NULL,
        [databasename] NVARCHAR(100) NOT NULL,
        [permissionsjson] NVARCHAR(MAX) NOT NULL,
        [isactive] BIT NOT NULL DEFAULT 1,
        [createdatutc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'tblApiKey' AND xtype='U')
BEGIN
    CREATE TABLE dbo.[tblApiKey] (
        [keyhash] CHAR(64) NOT NULL PRIMARY KEY,
        [clientid] INT NOT NULL,
        [expiresatutc] DATETIME2 NULL,
        [isrevoked] BIT NOT NULL DEFAULT 0,
        [createdatutc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_tblApiKey_tblApiClient FOREIGN KEY (clientid) REFERENCES dbo.tblApiClient(id)
    );
CREATE INDEX IX_tblApiKey_clientid ON dbo.tblApiKey(clientid);
END;
```

</details>

Because the API validates keys by comparing the **uppercase SHA-256 hex hash** of the incoming token, hash your raw key before inserting it:

```sql
INSERT INTO dbo.tblApiClient (clientname, databasename, permissionsjson)
VALUES ('Customer A', 'ERP_CustomerA', '["products:read","products:write","orders:read"]');

INSERT INTO dbo.tblApiKey (keyhash, clientid)
VALUES (CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST('<your-raw-api-key>' AS VARCHAR(MAX))), 2), SCOPE_IDENTITY());
```


## Running Tests

Ensure Docker is running, as `IntegrationTests` spins up a throwaway SQL Server container and creates named test databases (`ERP_Test_CustomerA`):

```bash
dotnet test ErpIntegrationApi.slnx
```

## Adding a New Feature Slice (Convention Guide)

1. **Application:** Create a feature folder under `src/Application/<Entity>/<Action>/` with the Query/Command, Response `record`, Validator (inheriting `PagedQueryValidator<T>` for list endpoints), and Dapper Handler.
2. **Web.Api:** Add a Minimal API endpoint under `src/Web.Api/Endpoints/<Entity>/` protected with `.HasPermission(":read|write")`.
3. **Tests:** Add table DDL to `IntegrationTestWebAppFactory`, create a `Test<Entity>` record + Bogus faker + `DbHelper` (`Func<T, T>` customization), and write Shouldly integration tests.
