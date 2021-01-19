using System;
using DebtPlanner;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtPlannerTests
{
    [TestClass]
    public class DebtInfoTests : TestBase
    {
        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25)]
        [DataRow("B", 8000, 350.00, 2.0)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22.00)]
        public void Create_Percent(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();
            i.MinimumPercent.Should().Be((decimal)min / (decimal)balance);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25)]
        [DataRow("B", 8000, 350.00, 2.0)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22.00)]
        public void Create_Payment(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();

            i.CurrentPayment.Should().Be(i.Balance > 0 ? i.Minimum + i.AdditionalPayment : 0);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25)]
        [DataRow("B", 8000, 350.00, 2.0)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22.00)]
        public void Create_PaymentReduction(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();

            i.CurrentPaymentReduction.Should().Be(i.CurrentPayment - i.AverageMonthlyInterest);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25)]
        [DataRow("B", 8000, 350.00, 2.0)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22.00)]
        public void Create_PayoffMonths(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();
            i.PayoffMonths.Should()
             .Be(i.Balance > 0
                     ? (int)Math.Ceiling(i.Balance / i.CurrentPaymentReduction)
                     : 0);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25)]
        [DataRow("B", 8000, 350.00, 2.0)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22.00)]
        public void Create_Balance(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();
            var newBalance = i.Balance - i.CurrentPaymentReduction;
            i.ApplyPayment();

            i.Balance.Should().Be(DebtInfo.RoundUp(newBalance, 2));
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25, 29)]
        [DataRow("B", 8000, 350.00, 2.0, 24)]
        [DataRow("C", 6000, 200, 3.5, 33)]
        [DataRow("D", 4000, 200, 12.25, 26)]
        [DataRow("E", 2000, 200, 15.55, 12)]
        [DataRow("F", 1000, 55, 10.25, 22)]
        [DataRow("G", 500, 100, 22.00, 6)]
        public void Create_PayoffNumberMonths(string name, double balance, double min, double rate, int expected)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();
            i.PayoffMonths.Should().Be(expected);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25, 337)]
        [DataRow("B", 8000, 350.00, 2.0, 286)]
        [DataRow("C", 6000, 200, 3.5, 395)]
        [DataRow("D", 4000, 200, 12.25, 302)]
        [DataRow("E", 2000, 200, 15.55, 138)]
        [DataRow("F", 1000, 55, 10.25, 259)]
        [DataRow("G", 500, 100, 22.00, 67)]
        public void Create_PayoffNumberDays(string name, double balance, double min, double rate, int expected)
        {
            var i = new DebtInfo(name, balance, rate, min);
            i.Should().NotBeNull();
            i.PayoffDays.Should().Be(expected);
        }

        [TestMethod]
        [DataRow("A", 10000.0, 400.0, 5.25, 337)]
        [DataRow("B", 8000, 350.00, 2.0, 286)]
        [DataRow("C", 6000, 200, 3.5, 395)]
        [DataRow("D", 4000, 200, 12.25, 302)]
        [DataRow("E", 2000, 200, 15.55, 138)]
        [DataRow("F", 1000, 55, 10.25, 259)]
        [DataRow("G", 500, 100, 22.00, 67)]
        public void GetAmortization(string name, double balance, double min, double rate, int expected)
        {
            var i = new DebtInfo(name, balance, rate, min);
            var list = i.GetAmortization();
            list.Count.Should().BeGreaterThan(0);
            Console.WriteLine(i);
            Console.WriteLine(
                $"\n{"Payment".PadRight(10)} | {"Interest".PadRight(8)} | {"Applied".PadRight(10)} | Remaining" +
                "\n-------------------------------------------------");
            list.ForEach(Console.WriteLine);
        }
    }
}
