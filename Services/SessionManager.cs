using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoAPL.Services
{
    public static class SessionManager
    {
        public static List<Cookie> SessionCookies { get; set; } = new List<Cookie>();

        public static void SetSessionCookies(IEnumerable<Cookie> cookies)
        {
            SessionCookies = cookies.ToList();
        }

        public static List<Cookie> GetSessionCookies()
        {
            return SessionCookies;
        }
    }
}
