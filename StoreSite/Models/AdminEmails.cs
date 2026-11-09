using System;
using System.Configuration;
using System.Linq;

namespace StoreSite.Models
{
    public static class AdminEmails
    {
        public static bool IsAdmin(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var raw = ConfigurationManager.AppSettings["AdminEmails"] ?? "";
            return raw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => x.Trim())
                      .Any(x => string.Equals(x, email.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}