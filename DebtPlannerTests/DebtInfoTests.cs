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
            i.MinimumPercent.Should().Be(Math.Round((decimal)min / (decimal)balance, 5));
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

            i.CurrentPayment.Should().Be(i.Balance > 0 ? i.Minimum + i.AdditionalPayment : 0);
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

            i.CurrentPaymentReduction.Should().Be(i.CurrentPayment - i.AverageMonthlyInterest);
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
            i.PayoffMonths.Should()
             .Be(i.Balance > 0
                     ? (int)Math.Ceiling(i.Balance / i.CurrentPaymentReduction)
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
            var newBalance = i.Balance - i.CurrentPaymentReduction;
            i.ApplyPayment();

            i.Balance.Should().Be(DebtInfo.RoundUp(newBalance, 2));
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5.25, 28)]
        [DataRow("B", 8000, 350, 2, 24)]
        [DataRow("C", 6000, 200, 3.5, 34)]
        [DataRow("D", 4000, 200, 12.25, 25)]
        [DataRow("E", 2000, 200, 15.55, 12)]
        [DataRow("F", 1000, 55, 10.25, 22)]
        [DataRow("G", 500, 100, 22, 6)]
        public void Create_PayoffNumberMonths(string name, double balance, double min, double rate, int expected)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();
            i.PayoffMonths.Should().Be(expected);
        }

        [TestMethod]
        [DataRow("A", 10000, 400, 5.25, 335)]
        [DataRow("B", 8000, 350, 2, 286)]
        [DataRow("C", 6000, 200, 3.5, 400)]
        [DataRow("D", 4000, 200, 12.25, 301)]
        [DataRow("E", 2000, 200, 15.55, 139)]
        [DataRow("F", 1000, 55, 10.25, 258)]
        [DataRow("G", 500, 100, 22, 67)]
        public void Create_PayoffNumberDays(string name, double balance, double min, double rate, int expected)
        {
            var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
            i.Should().NotBeNull();
            i.PayoffDays.Should().Be(expected);
        }

        //[TestMethod]
        //[DataRow("A", 10000, 400, 5.25, 337)]
        //[DataRow("B", 8000, 350, 2, 286)]
        //[DataRow("C", 6000, 200, 3.5, 395)]
        //[DataRow("D", 4000, 200, 12.25, 302)]
        //[DataRow("E", 2000, 200, 15.55, 138)]
        //[DataRow("F", 1000, 55, 10.25, 259)]
        //[DataRow("G", 500, 100, 22, 67)]
        //public void GetAmortization(string name, double balance, double min, double rate, int expected)
        //{
        //    var i = new DebtInfo(name, (decimal)balance, (decimal)rate, (decimal)min);
        //    var list = i.GetAmortization();
        //    list.Count.Should().BeGreaterThan(0);
        //    Console.WriteLine(i);
        //    Console.WriteLine(
        //        $"\n{"Payment".PadRight(10)} | {"Interest".PadRight(8)} | {"Applied".PadRight(10)} | Remaining" +
        //        "\n-------------------------------------------------");
        //    list.ForEach(Console.WriteLine);
        //}

        [TestMethod]
        public void CheckMinimumPayment()
        {
            b8.ForceMinPayment = false;
            var am = b8.GetAmortization(1)[0];
            am.AppliedPayment.Should().BeLessThan(am.Interest);
            b8.ForceMinPayment = true;
            var am2 = b8.GetAmortization(1)[0];
            am2.AppliedPayment.Should().BeGreaterOrEqualTo(am.Interest / 2);
        }

        [TestMethod]
        public void MinimumTooLow()
        {
            Func<DebtInfo> act = () => new DebtInfo("tooLow", 100000M, 20M, 1, false);
            act.Should().Throw<ArgumentOutOfRangeException>();

            Func<DebtInfo> act2 = () => new DebtInfo("tooLow", 100000M, 20M, 1);
            act2.Should().NotThrow<ArgumentOutOfRangeException>();
            act2().Minimum.Should().BeGreaterThan(1);
        }
    }
}
