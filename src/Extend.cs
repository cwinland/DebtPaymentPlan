using System.Linq;
using System.Security;

namespace DebtPlanner
{
    public static class Extend
    {
        public static SecureString ToSecureString(this string plain)
        {
            var sString = new SecureString();
            plain.ToCharArray().ToList().ForEach(c => sString.AppendChar(c));

            return sString;
        }
    }
}
