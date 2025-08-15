using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLSoft.Wind.Database.Models;

namespace JLSoft.Wind.Services
{
    public static class SessionManager
    {
        private static UserData _currentUser;
        private static readonly object _lock = new object();

        public static UserData CurrentUser
        {
            get
            {
                lock (_lock)
                {
                    return _currentUser;
                }
            }
        }

        public static bool IsLoggedIn => CurrentUser != null;

        // 只有登录流程可以调用
        public static void Login(UserData user)
        {
            lock (_lock)
            {
                _currentUser = user;
            }
        }

        // 只有登出流程可以调用
        public static void Logout()
        {
            lock (_lock)
            {
                _currentUser = null;
            }
        }
    }
}
