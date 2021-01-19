using System;
using System.Collections.Generic;
using System.Linq;

namespace DebtPlanner
{
    public class DebtInfo
    {
        private readonly double originalBalance;

        public string Name { get; }
        public double Balance { get; private set; }
        private double rate;
        public double Rate { get => Balance <= CurrentPayment ? 0 : rate; private set => rate = value; }
        public double OriginalMinimum => minimum;
        private double minimum;
        public double Minimum { get => Math.Min(Balance, minimum); private set => minimum = value; }
        public decimal MinimumPercent => Balance > 0 && Minimum > 0 ? (decimal)Minimum / (decimal)Balance : 0;
        public double AdditionalPayment { get; set; } = 0;
        public decimal DailyPr => Rate > 0 ? (decimal)Rate / 100 / 365 : 0;
        public decimal AverageMonthyPr => Rate > 0 ? (decimal)Rate / 100 / 12 : 0;
        public decimal DailyInterest => DailyPr * (decimal)Balance;

        public double AverageMonthlyInterest => RoundUp((double)(AverageMonthyPr * (decimal)Balance), 2);

        public double CurrentPayment => Balance > 0 ? Math.Min(Minimum + AdditionalPayment, Balance) : 0;

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

        public List<DebtAmortizationItem> GetAmortization(
            int? numberOfPayments = null, List<Tuple<int, double>> additionalPayments = null)
        {
            var result = new List<DebtAmortizationItem>();
            var currentDebt = this;
            var paymentsMade = 0;

            if (additionalPayments == null)
            {
                additionalPayments = new List<Tuple<int, double>>();
            }

            var additionalPayment = additionalPayments.OrderBy(x => x.Item1).FirstOrDefault();

            if (additionalPayment != null &&
                GetAmortization().Count <= additionalPayment.Item1)
            {
                return GetAmortization();
            }

            while (currentDebt.Balance > 0 &&
                   (!numberOfPayments.HasValue || numberOfPayments > 0))
            {
                if (additionalPayment != null &&
                    additionalPayment.Item1 == paymentsMade)
                {
                    currentDebt.AdditionalPayment += additionalPayment.Item2;
                }

                var debtAmortizationItem = new DebtAmortizationItem(currentDebt);
                result.Add(debtAmortizationItem);
                currentDebt = debtAmortizationItem.debtInfo;
                paymentsMade++;

                if (numberOfPayments.HasValue)
                {
                    numberOfPayments--;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString() => $"Name: {Name}\n" +
                                             $"{"Balance".PadRight(12)} | % Rate | Minimum | Max Payment\n" +
                                             "---------------------------------------------\n" +
                                             $"{Balance,12:C} | {Rate,5}% | {Minimum,7:C} | {CurrentPayment,11:C}";
    }
}
