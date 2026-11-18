# Clean Architecture
Based on Milan Jovanović's template:
https://milanjovanovic.tech/templates/clean-architecture

## What's left from the template?

- **SharedKernel** project with common Domain-Driven Design abstractions.
- **Domain** layer with sample entities *domain events removed*. [^1]
- **Application** layer with abstractions for:
  - CQRS (lightweight, MediatR-free command/query handlers)
  - Cross-cutting concerns (logging, validation) implemented as decorators
- **Infrastructure** layer with:
  - Permission-based authorization
  - User login and JWT tokens replaced with Api-Key.
  - *EF Core removed*, replaced with Dapper. *PostgreSQL* only used for OpenTelemetry.
  - *HybridCache unused* for fast, unified caching with cache invalidation. [^1]
  - Serilog structured logging
- **Web.Api** layer with:
  - Minimal API endpoints
  - **Rate limiting** (configurable global + authentication policies)
  - **OpenTelemetry** tracing and metrics (ASP.NET Core, HTTP, Npgsql, runtime)
  - Global exception handling and `ProblemDetails`
  - Scalar API reference (OpenAPI)
- **Grafana + Loki + Tempo + Prometheus** for logs, traces, and metrics, via the
  bundled `grafana/otel-lgtm` image (the app ships telemetry over OTLP)
  - Grafana is available at http://localhost:3000 by default
- **Testing** projects
  - Architecture testing (`ArchitectureTests`)
  - Unit testing (`Application.UnitTests`)
  - Integration testing with **Testcontainers** (`IntegrationTests`)

## Getting started
Configure database:
```bash
dotnet user-secrets set "ConnectionStrings:Database" "<ConnectionString>" --project src/Web.Api
```

Run, by default access at: http://localhost:5000/scalar/:
```bash
docker compose up -d        # PostgreSQL + Grafana/Loki/Tempo
dotnet run --project src/Web.Api
```

Run the full test suite (the integration tests spin up a throwaway MSSQL container,
so Docker must be running):
```bash
dotnet test GoErpApi.slnx
```

[^1]: Domain events were removed as the API does not have control over the database and so there
nothing to notify CacheLayer.