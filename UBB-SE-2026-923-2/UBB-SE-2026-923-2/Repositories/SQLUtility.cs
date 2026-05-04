using UBB_SE_2026_923_2.Configuration;

namespace UBB_SE_2026_923_2.Repositories
{
    internal static class SQLUtility
    {
        public static string GetConnectionString() => AppSettings.ConnectionString;
    }
}
