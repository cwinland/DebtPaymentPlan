using System;
using System.Collections.Generic;
using System.Linq;

namespace DebtPlanner
{
    public class DebtInfo
    {
        private readonly decimal minPaymentMultiplier = 1.5M;
        private decimal balance;

        private decimal minimum;
        private decimal rate;
        public decimal AdditionalPayment { get; set; }
        public bool ForceMinPayment { get; set; }
        public string Name { get; }
        public decimal Balance { get => balance; private set => balance = Math.Round(value, 2); }
        public decimal Rate { get => rate; private set => rate = value; }

        public decimal OriginalMinimum => minimum;

        public decimal Minimum
        {
            get => Math.Min(Balance,
                            ForceMinPayment
                                ? Math.Max(AverageMonthlyInterest * minPaymentMultiplier, OriginalMinimum)
                                : OriginalMinimum);
            private set => minimum = Math.Round(value, 2);
        }

        public decimal MinimumPercent => Balance > 0 && Minimum > 0 ? Minimum / Balance : 0;
        public decimal DailyPr => Rate > 0 ? Rate / 100 / 365 : 0;
        public decimal AverageMonthyPr => Rate > 0 ? Rate / 100 / 12 : 0;
        public decimal DailyInterest => DailyPr * Balance;
        public decimal AverageMonthlyInterest => RoundUp(AverageMonthyPr * Balance, 2);

        public decimal CurrentPayment => Balance > 0 ? Math.Min(Minimum + AdditionalPayment, Balance) : 0;

        public decimal CurrentPaymentReduction => CurrentPayment > 0 ? CurrentPayment - AverageMonthlyInterest : 0;

        public int PayoffMonths => Balance > 0
            ? (int)Math.Ceiling(Balance / CurrentPaymentReduction)
            : 0;

        public int PayoffDays => Balance > 0
            ? (int)Math.Ceiling(Balance / (CurrentPaymentReduction / 12))
            : 0;

        public DebtInfo(string name, decimal balance, decimal rate, decimal minPayment, bool forceMinPayment = true)
        {
            ForceMinPayment = forceMinPayment;

            Name = name;
            Balance = balance;
            Rate = rate;
            Minimum = minPayment;

            if (Balance > CurrentPayment &&
                CurrentPayment <= AverageMonthlyInterest)
            {
                throw new ArgumentOutOfRangeException(nameof(minPayment),
                                                      $"Current Payment ({CurrentPayment} is too low to pay the interest of {AverageMonthlyInterest}. ");
            }
        }

        public DebtInfo ApplyPayment()
        {
            Balance -= CurrentPaymentReduction;

            return this;
        }

        public DebtInfo ResetMinimum()
        {
            var minPercent = MinimumPercent;
            var newMin = Balance * minPercent;
            Minimum = RoundUp(newMin, 2);

            return this;
        }

        public static decimal RoundUp(decimal input, int places)
        {
            var multiplier = (int)Math.Ceiling(Math.Pow(10, Convert.ToDouble(places)));

            return Math.Ceiling(input * multiplier) / multiplier;
        }

        public virtual DebtAmortization GetAmortization(
            int? numberOfPayments = null, List<Tuple<int, decimal>> additionalPayments = null)
        {
            var result = new DebtAmortization();
            var currentDebt = this;
            var paymentsMade = 0;

            if (additionalPayments == null)
            {
                additionalPayments = new List<Tuple<int, decimal>>();
            }

            var additionalPayment = additionalPayments.OrderBy(x => x.Item1).FirstOrDefault();

            if (additionalPayment != null &&
                GetAmortization().Count <= additionalPayment.Item1)
            {
                return GetAmortization(numberOfPayments);
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
