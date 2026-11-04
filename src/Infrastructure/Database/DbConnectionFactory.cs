using System.Data;
using Application.Abstractions.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database;

public class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}