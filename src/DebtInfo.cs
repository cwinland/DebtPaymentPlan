using System;
using System.Collections.Generic;

namespace DebtPlanner
{
    public class DebtInfo
    {
        private readonly double originalBalance;

        public string Name { get; }
        public double Balance { get; private set; }
        private double rate;
        public double Rate { get => Balance <= CurrentPayment ? 0 : rate; private set => rate = value; }

        private double minimum;
        public double Minimum { get => Math.Min(Balance, minimum); private set => minimum = value; }
        public decimal MinimumPercent => Balance > 0 && Minimum > 0 ? (decimal)Minimum / (decimal)Balance : 0;
        public double AdditionalPayment { get; set; } = 0;
        public decimal DailyPr => Rate > 0 ? (decimal)Rate / 100 / 365 : 0;
        public decimal AverageMonthyPr => Rate > 0 ? (decimal)Rate / 100 / 12 : 0;
        public decimal DailyInterest => DailyPr * (decimal)Balance;
        public double AverageMonthlyInterest => RoundUp((double)(AverageMonthyPr * (decimal)Balance), 2);

        public double CurrentPayment => Balance > 0 ? Minimum + AdditionalPayment : 0;

        public double CurrentPaymentReduction => CurrentPayment > 0 ? CurrentPayment - AverageMonthlyInterest : 0;

        public int PayoffMonths => Balance > 0
            ? (int)Math.Ceiling(Balance / CurrentPaymentReduction)
            : 0;

        public int PayoffDays => Balance > 0
            ? (int)Math.Ceiling(Balance / (CurrentPaymentReduction / 12))
            : 0;

        public DebtInfo(string name, double balance, double rate, double minPayment)
        {
            Name = name;
            Balance = balance;
            originalBalance = Balance;
            Rate = rate;
            Minimum = minPayment;
        }

        public DebtInfo ApplyPayment()
        {
            Balance -= CurrentPaymentReduction;

            return this;
        }

        public DebtInfo ResetMinimum()
        {
            var minPercent = MinimumPercent;
            var newMin = (decimal)Balance * minPercent;
            Minimum = RoundUp(newMin, 2);

            return this;
        }

        public static double RoundUp(decimal input, int places) => RoundUp((double)input, places);

        public static double RoundUp(double input, int places)
        {
            var multiplier = (int)Math.Ceiling(Math.Pow(10, Convert.ToDouble(places)));

            return Math.Ceiling(input * multiplier) / multiplier;
        }

        public List<DebtAmortizationItem> GetAmortization()
        {
            var result = new List<DebtAmortizationItem>();
            var currentDebt = this;

            while (currentDebt.Balance > 0)
            {
                var debtAmortizationItem = new DebtAmortizationItem(currentDebt);
                result.Add(debtAmortizationItem);
                currentDebt = debtAmortizationItem.debtInfo;
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name}\n" +
                                             $"{"Balance".PadRight(12)} | % Rate | Minimum\n" +
                                             "-------------------------------\n" +
                                             $"{Balance,12:C} | {Rate,5}% | {Minimum,7:C}";
    }
}
