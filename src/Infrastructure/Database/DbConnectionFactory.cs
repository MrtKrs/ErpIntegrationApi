using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database;

public class DbConnectionFactory(string connectionString, ITenantContext tenantContext) : IDbConnectionFactory
{
    public IDbConnection CreateAuthConnection() => new SqlConnection(connectionString);

    public IDbConnection CreateErpConnection()
    {
        if(string.IsNullOrWhiteSpace(tenantContext.DatabaseName))
        {
            throw new InvalidOperationException("No ERP database resolved for the current request.");
        }

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
          InitialCatalog = tenantContext.DatabaseName
        };
        return new SqlConnection(builder.ConnectionString);
    }
}