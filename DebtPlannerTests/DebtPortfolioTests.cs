using System.Linq;
using DebtPlanner;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtPlannerTests
{
    [TestClass]
    public class DebtPortfolioTests : TestBase
    {
        private DebtPortfolio CreatePortfolio() => new DebtPortfolio { a0, a1, a2, a3, a4, a5, a6, a7, };

        [TestMethod]
        public void CreatedPortfolio() => CreatePortfolio().Should().NotBeNull();

        [TestMethod]
        [DataRow(0, 7)]
        [DataRow(5, 0)]
        [DataRow(6, 1)]
        [DataRow(4, 2)]
        [DataRow(7, 3)]
        [DataRow(1, 4)]
        [DataRow(3, 5)]
        [DataRow(2, 6)]
        public void OrderedPortfolio(int xIndex, int aIndex)
        {
            var a = CreatePortfolio();
            var x = a.OrderBy(info => info.PayoffDays).ToList();
            x[xIndex].Should().Be(a[aIndex]);
        }

        [TestMethod]
        [DataRow(0, 80)]
        [DataRow(1, 96)]
        [DataRow(2, 35)]
        [DataRow(3, 168)]
        [DataRow(4, 11)]
        [DataRow(5, 26)]
        [DataRow(6, 12)]
        [DataRow(7, 0)]
        public void NumberOfPayments(int pIndex, int expectedPayments)
        {
            var p = CreatePortfolio();
            p[pIndex].GetAmortization().Count.Should().Be(expectedPayments);
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
            p.Min(info => info.PayoffMonths).Should().Be(a7.PayoffMonths);
            p.Where(x => x.Balance > 0).Min(info => info.PayoffMonths).Should().Be(a4.PayoffMonths);
        }
    }
}
