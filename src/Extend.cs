using System.Linq;
using System.Security;
using DebtPlanner.Data;

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

        public static void Save(this DebtPortfolio portfolio) => new DebtFileStorage().Save(portfolio);
    }
}
