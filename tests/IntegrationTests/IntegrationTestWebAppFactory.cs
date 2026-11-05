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
        builder.UseSetting("detailedErrors", "true");
        builder.CaptureStartupErrors(true);

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

        const string initializeSchemaSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tblVarer' AND xtype='U')
            BEGIN
                CREATE TABLE [dbo].[tblVarer](
                    [id] [int] NOT NULL,
                    [varenummer] [nvarchar](50) NULL,
                    [varenavn] [nvarchar](1000) NULL,
                    [standardpris] [nvarchar](50) NULL
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
