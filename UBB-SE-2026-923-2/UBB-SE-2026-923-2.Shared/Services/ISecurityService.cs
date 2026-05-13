namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ISecurityService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string stored);
    }
}
