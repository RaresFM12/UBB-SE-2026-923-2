using System;

namespace UBB_SE_2026_923_2.Configuration;

public static class AppSettings
{
    public const string ConnectionString =
        @"Data Source=localhost\SQLEXPRESS;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
    public static readonly DateTime SqlMinimumDate = new DateTime(1753, 1, 1);
}



