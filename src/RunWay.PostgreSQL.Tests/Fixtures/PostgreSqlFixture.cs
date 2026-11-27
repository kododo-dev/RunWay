using DotNet.Testcontainers.Builders;
using Kododo.RunWay;
using Kododo.RunWay.PostgreSQL;
using Kododo.RunWay.Storage.Tests;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Kododo.RunWay.PostgreSQL.Tests.Fixtures;

public sealed class PostgreSqlFixture : IAsyncLifetime, IStorageTestFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    public void ConfigureStorage(IRunWayConfiguration config) =>
        config.UsePostgreSQL(_ => new NpgsqlConnection(ConnectionString));

    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""
            DELETE FROM runway.jobs_audit;
            DELETE FROM runway.jobs;
            DELETE FROM runway.runner_job_types;
            DELETE FROM runway.runners;
            """, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
