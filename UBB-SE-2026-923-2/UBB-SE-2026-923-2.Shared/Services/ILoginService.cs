namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ILoginService
    {
        public bool Login(string email, string password);

        public bool Register(string email, string password, string phoneNumber, string username);
    }
}
