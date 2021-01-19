using System;
using System.Linq;
using DebtPlanner;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtPlannerTests
{
    [TestClass]
    public class DebtPortfolioTests : TestBase
    {
        private DebtPortfolio CreatePortfolio() => new DebtPortfolio { a0, a1, a2, a3, a4, a5, a6, a7, b8, };

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
        [DataRow(3, 109)]
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
        [DataRow(7, 0)]
        [DataRow(4, 1)]
        [DataRow(6, 2)]
        [DataRow(5, 3)]
        [DataRow(2, 4)]
        [DataRow(0, 5)]
        [DataRow(1, 6)]
        [DataRow(3, 7)]
        public void OrderedNumberOfPayments(int pIndex, int orderedIndex)
        {
            var p = CreatePortfolio();
            var x = p.OrderBy(info => info.GetAmortization().Count).ToList();
            x[orderedIndex].Should().Be(p[pIndex]);
        }

        [TestMethod]
        public void FirstNonZero()
        {
            var p = CreatePortfolio();
            var paidInfo = p.Where(info => info.Balance == 0).ToList();
            var paid = paidInfo.Sum(info => info.OriginalMinimum);
            var x = p.Where(info => info.Balance > 0).OrderBy(info => info.GetAmortization().Count).First();
            x.Should().Be(a4);
            var originalA = a4.GetAmortization();
            a4.AdditionalPayment = paid;
            var newA = a4.GetAmortization();
            originalA.Count.Should().NotBe(newA.Count);
        }

        [TestMethod]
        public void PayoffMonths_Max()
        {
            var p = CreatePortfolio();
            var x = p.Max(info => info.PayoffMonths);
            x.Should().Be(b8.PayoffMonths);
        }

        [TestMethod]
        public void PayoffMonths_Min()
        {
            var p = CreatePortfolio();
            p.Min(info => info.PayoffMonths).Should().Be(a7.PayoffMonths);
            p.Where(x => x.Balance > 0).Min(info => info.PayoffMonths).Should().Be(a4.PayoffMonths);
        }

        [TestMethod]
        public void Amortization_NeverChanges()
        {
            var p = CreatePortfolio();
            var l = p.GetAmortization();
            var m = p.GetAmortization();
            l.ToString().Should().Be(m.ToString());
        }

        [TestMethod]
        public void WriteTest()
        {
            var p = CreatePortfolio();

            Console.WriteLine(p);
        }

        [TestMethod]
        public void ToString_NeverChanges()
        {
            var p = CreatePortfolio();
            var h1 = p.Header;
            var h2 = p.Header;
            h1.Should().Be(h2);

            var pay1 = p.Payments;
            var pay2 = p.Payments;

            pay1.Should().Be(pay2);
            var h3 = p.Header;
            h2.Should().Be(h3);

            var pay3 = p.Payments;
            pay2.Should().Be(pay3);
        }
    }
}
