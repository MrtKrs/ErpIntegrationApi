using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Web.Api;

namespace IntegrationTests;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());

        // Provide deterministic JWT settings so tokens can be issued and validated in tests.
        builder.UseSetting("Jwt:Secret", "super-duper-secret-value-that-should-be-in-user-secrets");
        builder.UseSetting("Jwt:Issuer", "clean-architecture");
        builder.UseSetting("Jwt:Audience", "developers");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");

        // Relax rate limiting so the test suite is not throttled.
        builder.UseSetting("RateLimiting:Global:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Authentication:PermitLimit", "100000");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Initialize a dummy schema map
        using IServiceScope scope = Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        
        using IDbConnection connection = connectionFactory.CreateConnection();

        //placeholder script. replace with real database table schemas
        const string initializeSchemaSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TodoItems' AND xtype='U')
            BEGIN
                CREATE TABLE TodoItems (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    UserId UNIQUEIDENTIFIER NOT NULL,
                    Description NVARCHAR(MAX) NOT NULL,
                    IsCompleted BIT NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    DueDate DATETIME2 NULL,
                    Labels NVARCHAR(MAX) NULL,
                    CompletedAt DATETIME2 NULL
                );
            END";

        await connection.ExecuteAsync(initializeSchemaSql);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
