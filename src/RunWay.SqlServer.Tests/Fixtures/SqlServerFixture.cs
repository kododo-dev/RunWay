using Kododo.RunWay;
using Kododo.RunWay.SqlServer;
using Kododo.RunWay.Storage.Tests;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace Kododo.RunWay.SqlServer.Tests.Fixtures;

public sealed class SqlServerFixture : IAsyncLifetime, IStorageTestFixture
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public void ConfigureStorage(IRunWayConfiguration config) =>
        config.UseSqlServer(_ => new SqlConnection(ConnectionString));

    public async Task ResetAsync()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("""
            DELETE FROM runway.jobs_audit;
            DELETE FROM runway.jobs;
            DELETE FROM runway.runner_job_types;
            DELETE FROM runway.runners;
            """, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
