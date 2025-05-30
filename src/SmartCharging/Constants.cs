namespace SmartCharging;

public static class Constants
{
    public static class Database
    {
        private const string Suffix = "DB";
        public static readonly string SmartCharging = $"{nameof(SmartCharging)}_{Suffix}";
    }
}
