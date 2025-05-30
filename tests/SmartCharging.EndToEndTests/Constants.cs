namespace SmartCharging.EndToEndTests;

public static class Constants
{
    public static class Routes
    {
        public static class Groups
        {
            private const string RootBaseAddress = "api/v1";

            private const string GroupsBaseAddress = $"{RootBaseAddress}/groups";

            public static string GetByPage => $"{GroupsBaseAddress}/";

            public static string GetById(Guid id) => $"{GroupsBaseAddress}/{id}";

            public static string Delete(Guid id) => $"{GroupsBaseAddress}/{id}";

            public static string Put(Guid id) => $"{GroupsBaseAddress}/{id}";

            public static string Create => $"{GroupsBaseAddress}/";

            public static string AddChargeStation(Guid groupId) => $"{GroupsBaseAddress}/{groupId}/charge-stations";
        }
    }
}
