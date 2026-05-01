using System;

namespace UBB_SE_2026_923_2.Repositories
{
    internal static class SQLUtility
    {
        public static string GetConnectionString()
        {
            return "Data Source=" + Environment.MachineName + ";Initial Catalog=Pharmacy;Integrated Security=true;TrustServerCertificate=true;";
        }
    }
}
