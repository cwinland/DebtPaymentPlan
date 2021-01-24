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
        public void CreatedPortfolio()
        {
            var port = CreatePortfolio();
            port.Should().NotBeNull();
            var newPort = new DebtPortfolio(port.ToList());
            newPort.Should().NotBeNullOrEmpty();
            newPort.Should().Equals(port);
        }

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
        [DataRow(0, 79)]
        [DataRow(1, 96)]
        [DataRow(2, 35)]
        [DataRow(3, 111)]
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
            var p = new DebtPortfolio(CreatePortfolio().OrderBy(x => x.Name).ToList());
            var l = p.GetAmortization();
            p = new DebtPortfolio(CreatePortfolio().OrderByDescending(x => x.Name).ToList());
            var m = p.GetAmortization();
            l.ToList().ForEach(x => x.Value.Equals(m.First(y => y.Key.Name == x.Key.Name).Value));
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

        [TestMethod]
        public void Test()
        {
            var portfolio = new DebtPortfolio
            {
                new DebtInfo("A", 16345.44M, 3.25M, 225),
                new DebtInfo("B", 12000, 0, 125),
                new DebtInfo("C", 6000, 3.5M, 182),
                new DebtInfo("D", 4000, 12.25M, 50),
                new DebtInfo("E", 2000, 15.55M, 200),
                new DebtInfo("F", 1000, 22, 50),
                new DebtInfo("G", 500, 22, 50),
                new DebtInfo("H", 10, 50.3M, 250),
                new DebtInfo("I", 13000, 12, 100),
            };

            var schedule = portfolio.ToString();
            Console.WriteLine(schedule);
        }
    }
}
