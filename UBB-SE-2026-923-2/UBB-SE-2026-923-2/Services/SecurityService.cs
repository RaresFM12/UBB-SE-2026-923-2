using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UBB_SE_2026_923_2.Services
{
    public class SecurityService : ISecurityService
    {
        public string HashPassword(string password)
        {
            return password;
        }

        public bool VerifyPassword(string password, string stored)
        {
            return password == stored;
        }
    }
}
