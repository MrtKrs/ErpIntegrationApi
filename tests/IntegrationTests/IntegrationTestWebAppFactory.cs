using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Authentication;
using Infrastructure.Database;
using IntegrationTests.Common.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        using (IDbConnection authConnection = connectionFactory.CreateAuthConnection())
        {
            await authConnection.ExecuteAsync($"""
                IF DB_ID('{BaseIntegrationTest.TestErpDatabaseName}') IS NULL
                    CREATE DATABASE [{BaseIntegrationTest.TestErpDatabaseName}];
            """);

            await authConnection.ExecuteAsync("""
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
            """);
        }

        var apiKeyDbHelper = new ApiKeyDbHelper(connectionFactory);

        await apiKeyDbHelper.SeedApiKeyAsync(k => k with
        {
            RawKey = BaseIntegrationTest.AdminApiKey,
            ClientName = "Integration Test Admin",
            DatabaseName = BaseIntegrationTest.TestErpDatabaseName,
            Permissions = ["products:read", "products:write", "orders:read", "orders:write"]
        });

        await apiKeyDbHelper.SeedApiKeyAsync(k => k with
        {
            RawKey = BaseIntegrationTest.ReadOnlyApiKey,
            ClientName = "Integration Test ReadOnly",
            DatabaseName = BaseIntegrationTest.TestErpDatabaseName,
            Permissions = ["products:read", "orders:read"] 
        });
        
        tenantContext.SetDatabase(BaseIntegrationTest.TestErpDatabaseName);
        using IDbConnection erpConnection = connectionFactory.CreateErpConnection();
        const string initializeSchemaSql = """
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tblVarer' AND xtype='U')
            BEGIN
                CREATE TABLE [dbo].[tblVarer](
                    [id] [int] IDENTITY(1,1) NOT NULL,
                    [varenummer] [nvarchar](50) NULL,
                    [varenavn] [nvarchar](1000) NULL,
                    [standardpris] [nvarchar](50) NULL
                );
            END;
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'tblLager' AND xtype='U')
            BEGIN
                CREATE Table [dbo].[tblLager](
                    [id] [int] IDENTITY(1,1) NOT NULL,
                    [vareid] [int] NULL,
                    [benytlagerstyring] [bit] NULL,
                    [beholdning] [int] NULL,
                    [lavgrænse] [int] NULL,
                    [reserveret] [int] NULL,
                    [disponibel] [int] NULL,
                    [bestilt] [int] NULL,
                );
            END;

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'tblSalgsBilag' AND xtype='U')
            BEGIN
                CREATE TABLE [dbo].[tblSalgsBilag](
                    [id] [int] IDENTITY(1,1) NOT NULL,
                    [bilagsid] [nvarchar](50) NULL,
                    [bilagstype] [int] NULL,
                    [bilagsdato] [date] NULL,
                    [nettobeløb] [nvarchar](50) NULL,
                    [moms] [nvarchar](50) NULL,
                    [bilagsbeløb] [nvarchar](50) NULL,
                    [debitorid] [int] NULL,
                    [debitornr] [nvarchar](50) NULL,
                    [debitornavn] [nvarchar](50) NULL,
                    [bemærkninger] [ntext] NULL,
                    [sælgernavn] [nvarchar](50) NULL,
                    [fakturanr] [nvarchar](50) NULL,
                    [vedrbilagsnr] [nvarchar](50) NULL
                );
            END;
            """;

        await erpConnection.ExecuteAsync(initializeSchemaSql);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
