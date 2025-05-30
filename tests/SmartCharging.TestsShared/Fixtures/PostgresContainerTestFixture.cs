using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace SmartCharging.TestsShared.Fixtures;

public class PostgresContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; }
    public int HostPort => PostgresContainer.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
    public int TcpContainerPort => PostgreSqlBuilder.PostgreSqlPort;

    public string ConnectionString => PostgresContainer.GetConnectionString();

    public PostgresContainerFixture()
    {
        var options = new PostgresContainerOptions();
        var postgresContainerBuilder = new PostgreSqlBuilder()
            .WithDatabase(options.DatabaseName)
            .WithCleanUp(true)
            .WithName(options.Name)
            .WithImage(options.ImageName);

        PostgresContainer = postgresContainerBuilder.Build();
    }

    public async Task InitializeAsync()
    {
        await PostgresContainer.StartAsync();
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(PostgresContainer.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            var checkpoint = await Respawner.CreateAsync(
                connection,
                new RespawnerOptions { DbAdapter = DbAdapter.Postgres }
            );

            // https://github.com/jbogard/Respawn/issues/108
            // https://github.com/jbogard/Respawn/pull/115 - fixed
            await checkpoint.ResetAsync(connection)!;
        }
        catch (Exception e) { }
    }

    public async Task DisposeAsync()
    {
        await PostgresContainer.StopAsync();
        await PostgresContainer.DisposeAsync(); //important for the event to cleanup to be fired!
    }
}

public sealed class PostgresContainerOptions
{
    public string Name { get; set; } = "postgres_" + Guid.NewGuid();
    public string ImageName { get; set; } = "postgres:latest";
    public string DatabaseName { get; set; } = "test_db";
}
