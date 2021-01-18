using System.Linq;
using DebtPlanner;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtPlannerTests
{
    [TestClass]
    public class DebtPortfolioTests : TestBase
    {
        private readonly DebtInfo a0 = new DebtInfo("A", 16000, 3.25, 225);
        private readonly DebtInfo a1 = new DebtInfo("B", 12000, 0, 125);
        private readonly DebtInfo a2 = new DebtInfo("C", 6000, 3.5, 182);
        private readonly DebtInfo a3 = new DebtInfo("D", 4000, 12.25, 50);
        private readonly DebtInfo a4 = new DebtInfo("E", 2000, 15.55, 200);
        private readonly DebtInfo a5 = new DebtInfo("F", 1000, 22, 50);
        private readonly DebtInfo a6 = new DebtInfo("G", 500, 22, 50);

        private DebtPortfolio CreatePortfolio() => new DebtPortfolio { a0, a1, a2, a3, a4, a5, a6, };

        [TestMethod]
        public void CreatedPortfolio() => CreatePortfolio().Should().NotBeNull();

        [TestMethod]
        [DataRow(0, 4)]
        [DataRow(1, 6)]
        [DataRow(2, 5)]
        [DataRow(3, 2)]
        [DataRow(4, 0)]
        [DataRow(5, 1)]
        [DataRow(6, 3)]
        public void OrderedPortfolio(int index, int expectedInfoIndex)
        {
            var p = CreatePortfolio();
            var x = p.OrderBy(info => info.PayoffDays).ToList();
            x[index].Should().Be(p[expectedInfoIndex]);
        }

        [TestMethod]
        public void PayoffMonths_Max()
        {
            var p = CreatePortfolio();
            var x = p.Max(info => info.PayoffMonths);
            x.Should().Be(a3.PayoffMonths);
        }

        [TestMethod]
        public void PayoffMonths_Min()
        {
            var p = CreatePortfolio();
            var x = p.Min(info => info.PayoffMonths);
            x.Should().Be(a4.PayoffMonths);
        }
    }
}
