using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SmartCharging.TestsShared.Fixtures;

public class SharedFixture<TEntryPoint, TEfCoreDbContext> : IAsyncLifetime
    where TEntryPoint : class
    where TEfCoreDbContext : DbContext
{
    // fields
    private readonly IMessageSink _messageSink;
    private IHttpContextAccessor? _httpContextAccessor;
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private HttpClient? _guestClient;

    // properties
    public event Func<Task>? SharedFixtureInitialized;
    public event Func<Task>? SharedFixtureDisposed;
    public PostgresContainerFixture PostgresContainerFixture { get; }
    public CustomWebApplicationFactory<TEntryPoint> Factory { get; }
    public IServiceProvider ServiceProvider => _serviceProvider ??= Factory.Services;

    public IConfiguration Configuration => _configuration ??= ServiceProvider.GetRequiredService<IConfiguration>();

    public IHttpContextAccessor HttpContextAccessor =>
        _httpContextAccessor ??= ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    /// <summary>
    /// We should not dispose this GuestClient, because we reuse it in our tests
    /// </summary>
    public HttpClient GuestClient
    {
        get
        {
            if (_guestClient == null)
            {
                _guestClient = Factory.CreateDefaultClient();

                // Set the media type of the request to JSON - we need this for getting problem details result for all http calls because problem details just return response for request with media type JSON
                _guestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return _guestClient;
        }
    }

    // constructor
    public SharedFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        messageSink.OnMessage(new DiagnosticMessage("Constructing SharedFixture..."));

        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        PostgresContainerFixture = new PostgresContainerFixture();

        Factory = new CustomWebApplicationFactory<TEntryPoint>();
    }

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        Factory.SetOutputHelper(outputHelper);
    }

    public async Task InitializeAsync()
    {
        // for having the capability of overriding dependencies in IntegrationTestBase, we should not build a service provider here.
        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Started..."));

        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        await PostgresContainerFixture.InitializeAsync();

        if (SharedFixtureInitialized is not null)
        {
            await SharedFixtureInitialized.Invoke();
        }
    }

    public async Task DisposeAsync()
    {
        await PostgresContainerFixture.DisposeAsync();

        GuestClient.Dispose();

        if (SharedFixtureDisposed is not null)
        {
            await SharedFixtureDisposed.Invoke();
        }

        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Stopped..."));
    }

    public void WithTestConfigureServices(Action<IServiceCollection> services)
    {
        Factory.WithTestConfigureServices(services);
    }

    public void WithTestConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> appConfiguration)
    {
        Factory.WithTestConfigureAppConfiguration(appConfiguration);
    }

    public void WithTestConfiguration(Action<IConfiguration> configurations)
    {
        Factory.WithTestConfiguration(configurations);
    }

    public void AddOverrideEnvKeyValues(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideEnvKeyValues(keyValuesAction);
    }

    public void AddOverrideInMemoryConfig(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideInMemoryConfig(keyValuesAction);
    }

    public async Task ResetDatabasesAsync()
    {
        await PostgresContainerFixture.ResetDbAsync();
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    public async ValueTask WaitUntilConditionMet(
        Func<Task<bool>> conditionToMet,
        int? timeoutSecond = null,
        string? exception = null
    )
    {
        var time = timeoutSecond ?? 300;

        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();

        while (!meet)
        {
            if (timeoutExpired)
            {
                throw new TimeoutException(
                    exception ?? $"Condition not met for the test in the '{timeoutExpired}' second."
                );
            }

            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(time);
        }
    }

    public async Task ExecuteTxContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task ExecuteAndResetStateContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.RollbackTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteTxContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteAndResetStateContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.RollbackTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task ExecuteEfDbContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TEfCoreDbContext>()));
    }

    public async Task<T> ExecuteEfDbContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        return await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TEfCoreDbContext>()));
    }

    public Task ExecuteEfDbContextAsync(Func<TEfCoreDbContext, Task> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TEfCoreDbContext>()));

    public Task<TEntity> ExecuteEfDbContextAsync<TEntity>(Func<TEfCoreDbContext, Task<TEntity>> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TEfCoreDbContext>()));

    public async Task InsertEfDbContextAsync<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().AddRange(entities);

            return db.SaveChangesAsync();
        });
    }

    public async Task InsertEfDbContextAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2, TEntity3>(
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3
    )
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2, TEntity3, TEntity4>(
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3,
        TEntity4 entity4
    )
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T?> FindEfDbContextAsync<T>(object id)
        where T : class
    {
        return ExecuteEfDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }
}
