using System.Data.Common;

namespace SmartCharging.ServiceDefaults.EF;

public interface IConnectionFactory : IDisposable
{
    Task<DbConnection> GetOrCreateConnectionAsync();
}
