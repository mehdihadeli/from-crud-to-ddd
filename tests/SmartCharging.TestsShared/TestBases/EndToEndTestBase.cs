using Microsoft.EntityFrameworkCore;
using SmartCharging.TestsShared.Fixtures;

namespace SmartCharging.TestsShared.TestBases;

public class EndToEndTestBase<TEntryPoint, TContext>(SharedFixture<TEntryPoint, TContext> sharedFixture)
    : IntegrationTestBase<TEntryPoint, TContext>(sharedFixture)
    where TEntryPoint : class
    where TContext : DbContext;
