using Humanizer;

namespace SmartCharging.ServiceDefaults.Constants;

public class AspireApplicationResources
{
    public static class PostgresDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "pg";
        public static readonly string SmartCharging = $"{Prefix}-{nameof(SmartCharging).Kebaberize()}{Postfix}";
    }

    public static class Api
    {
        public static readonly string SmartChargingApi = $"{nameof(SmartChargingApi).Kebaberize()}";
        public static readonly string SmartChargingStatisticsApi = $"{nameof(SmartChargingStatisticsApi).Kebaberize()}";
    }
}
