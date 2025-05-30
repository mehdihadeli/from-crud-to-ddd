using Microsoft.EntityFrameworkCore;
using SmartCharging.TestsShared.Fixtures;
using Xunit.Abstractions;

namespace SmartCharging.TestsShared.TestBases;

public class EndToEndTestBase<TEntryPoint, TContext>(
    SharedFixture<TEntryPoint, TContext> sharedFixture,
    ITestOutputHelper outputHelper
) : IntegrationTestBase<TEntryPoint, TContext>(sharedFixture, outputHelper)
    where TEntryPoint : class
    where TContext : DbContext;
