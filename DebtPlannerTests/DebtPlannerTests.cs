using DebtPlanner;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtPlannerTests
{
    [TestClass]
    public class DebtPlannerTests : TestBase
    {
        [TestMethod]
        public void TestPlanner()
        {
            var i = new DebtInfo("A", 10000, 2.25, 200);
            var x = new DebtPortfolio { i, };
            var p = new DebtPlanner.DebtPlanner { Portfolio = x, };
            p.Should().NotBeNull();
        }
    }
}
