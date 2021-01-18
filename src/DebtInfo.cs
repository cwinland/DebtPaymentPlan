using System;

namespace DebtPlanner
{
    public class DebtInfo
    {
        private readonly double originalBalance;

        public string Name { get; set; }
        public double Balance { get; set; }
        public double Rate { get; set; }
        private double minimum;
        public double Minimum { get => Math.Min(Balance, minimum); set => minimum = value; }
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
    }
}
