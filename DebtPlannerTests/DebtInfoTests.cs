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
        [DataRow("A", 10000, 400, 5.25)]
        [DataRow("B", 8000, 350, 2)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22)]
        public void Create_Percent(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();
            i.MinimumPercent().Should().Be(Math.Round((decimal)min / (decimal)balance, 5));
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5.25)]
        [DataRow("B", 8000, 350, 2)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22)]
        public void Create_Payment(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();

            i.GetCurrentPayment().Should().Be(i.Balance > 0 ? i.GetMinimum() + i.AdditionalPayment : 0);
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5.25)]
        [DataRow("B", 8000, 350, 2)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22)]
        public void Create_PaymentReduction(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();

            i.GetCurrentPaymentReduction().Should().Be(i.GetCurrentPayment() - i.GetAverageMonthlyInterest());
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5.25)]
        [DataRow("B", 8000, 350, 2)]
        [DataRow("C", 6000, 200, 3.5)]
        [DataRow("D", 4000, 200, 12.25)]
        [DataRow("E", 2000, 200, 15.55)]
        [DataRow("F", 1000, 55, 10.25)]
        [DataRow("G", 500, 100, 22)]
        public void Create_PayoffMonths(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();
            i.GetPayoffMonths()
             .Should()
             .Be(i.Balance > 0
                     ? (int)Math.Ceiling(i.Balance / i.GetCurrentPaymentReduction())
                     : 0);
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5)]
        [DataRow("B", 8000, 350, 2)]
        [DataRow("C", 6000, 200, 3)]
        [DataRow("D", 4000, 200, 12)]
        [DataRow("E", 2000, 200, 15)]
        [DataRow("F", 1000, 55, 10)]
        [DataRow("G", 500, 100, 22)]
        public void Create_Balance(string name, double balance, double min, double rate)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();
            var newBalance = i.Balance - i.GetCurrentPaymentReduction();
            i.ApplyPayment();

            i.Balance.Should().Be(DebtInfo.RoundUp(newBalance, 2));
        }

        [TestMethod]
        public void CheckMinimumPayment()
        {
            var info = new DebtInfo("A", 16000, 3.25M, 225, 225, false, null);
            var am = info.GetAmortization(1)[0];
            am.Payment.Should().Be(info.OriginalMinimum);
            info.ForceMinPayment = true;
            var am2 = info.GetAmortization(1)[0];
            am2.Payment.Should().BeGreaterOrEqualTo(info.OriginalMinimum);
        }

        [TestMethod]
        public void MinimumTooLow()
        {
            Func<DebtInfo> act = () => new DebtInfo("tooLow", 100000M, 20M, 1, 1, false, null);
            act.Should().Throw<ArgumentOutOfRangeException>();

            Func<DebtInfo> act2 = () => new DebtInfo("tooLow", 100000M, 20M, 1);
            act2.Should().NotThrow<ArgumentOutOfRangeException>();
            act2().GetMinimum().Should().BeGreaterThan(1);
        }

        [TestMethod]
        public void CalendarInterestRate()
        {
            var info = new DebtInfo("Test", 12345, 0, 100);
            info.GetInterestRate().Should().Be(0);

            info.SetInterestRate(3.25M);
            info.SetInterestRate(4.5M, DateTime.Parse("3/15/2021"));
            info.SetInterestRate(12M, DateTime.Parse("4/15/2021"));
            info.SetInterestRate(0, DateTime.Parse("12/30/2022"));

            info.GetInterestRate().Should().Be(3.25M);

            info.GetInterestRate(DateTime.Parse("3/14/2021")).Should().Be(3.25M);
            info.GetInterestRate(DateTime.Parse("3/15/2021")).Should().Be(4.5M);
            info.GetInterestRate(DateTime.Parse("4/15/2021")).Should().Be(12M);
            info.GetInterestRate(DateTime.Parse("5/15/2021")).Should().Be(12M);
            info.GetInterestRate(DateTime.Parse("6/15/2021")).Should().Be(12M);
            info.GetInterestRate(DateTime.Parse("7/15/2021")).Should().Be(12M);
            info.GetInterestRate(DateTime.Parse("12/30/2021")).Should().Be(12M);
            info.GetInterestRate(DateTime.Parse("12/30/2022")).Should().Be(0);
            info.GetInterestRate(DateTime.Parse("1/2/2023")).Should().Be(0);
        }

        [TestMethod]
        public void TestScheduleWithRateChanges()
        {
            var info = new DebtInfo("Test", 12345, 0, 100);
            info.SetInterestRate(3.25M);
            info.SetInterestRate(4.5M, DateTime.Parse("3/15/2021"));
            info.SetInterestRate(12M, DateTime.Parse("4/15/2021"));
            info.SetInterestRate(0, DateTime.Parse("12/30/2022"));

            Console.WriteLine(info.GetAmortization());
        }
    }
}
