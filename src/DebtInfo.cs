using System;
using System.Collections.Generic;
using System.Linq;
using AppConfigSettings;
using AppConfigSettings.Enum;

namespace DebtPlanner
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <summary>
    /// Class DebtInfo.
    /// </summary>
    public class DebtInfo
    {
        public static ConfigSetting<decimal> MultiplierSetting { get; set; } =
            new ConfigSetting<decimal>("PaymentMultiplier", 1.5M, SettingScopes.Any, num => num >= 0M);

        private readonly decimal minPaymentMultiplier;

        private decimal balance;

        private decimal rate;

        /// <summary>
        /// Gets or sets the additional payment.
        /// </summary>
        /// <value>The additional payment.</value>
        public decimal AdditionalPayment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force minimum payment].
        /// </summary>
        /// <value><c>true</c> if [force minimum payment]; otherwise, <c>false</c>.</value>
        public bool ForceMinPayment { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the original minimum.
        /// </summary>
        /// <value>The original minimum.</value>
        public decimal OriginalMinimum { get; private set; }

        /// <summary>
        /// Gets the balance.
        /// </summary>
        /// <value>The balance.</value>
        public decimal Balance { get => balance; private set => balance = Math.Round(value, 2); }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Rate { get => Math.Round(rate); private set => rate = value; }

        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public decimal Minimum
        {
            get => Math.Min(Balance,
                            ForceMinPayment
                                ? Math.Max(Math.Round(AverageMonthlyInterest * minPaymentMultiplier, 2),
                                           OriginalMinimum)
                                : OriginalMinimum);
            private set => OriginalMinimum = Math.Round(value, 2);
        }

        /// <summary>
        /// Gets the minimum percent.
        /// </summary>
        /// <value>The minimum percent.</value>
        public decimal MinimumPercent => Balance > 0 && Minimum > 0 ? Math.Round(Minimum / Balance, 5) : 0;

        /// <summary>
        /// Gets the daily pr.
        /// </summary>
        /// <value>The daily pr.</value>
        public decimal DailyPr => Rate > 0 ? Math.Round(Rate / 100 / 365, 12) : 0;

        /// <summary>
        /// Gets the average monthy pr.
        /// </summary>
        /// <value>The average monthy pr.</value>
        public decimal AverageMonthyPr => Rate > 0 ? Math.Round(Rate / 100 / 12, 10) : 0;

        /// <summary>
        /// Gets the daily interest.
        /// </summary>
        /// <value>The daily interest.</value>
        public decimal DailyInterest => RoundUp(DailyPr * Balance, 2);

        /// <summary>
        /// Gets the average monthly interest.
        /// </summary>
        /// <value>The average monthly interest.</value>
        public decimal AverageMonthlyInterest => RoundUp(AverageMonthyPr * Balance, 2);

        /// <summary>
        /// Gets the current payment.
        /// </summary>
        /// <value>The current payment.</value>
        public decimal CurrentPayment => Balance > 0 ? Math.Min(Minimum + AdditionalPayment, Balance) : 0;

        /// <summary>
        /// Gets the current payment reduction.
        /// </summary>
        /// <value>The current payment reduction.</value>
        public decimal CurrentPaymentReduction => CurrentPayment > 0 ? CurrentPayment - AverageMonthlyInterest : 0;

        /// <summary>
        /// Gets the payoff months.
        /// </summary>
        /// <value>The payoff months.</value>
        public int PayoffMonths => Balance > 0
            ? (int)Math.Ceiling(Balance / CurrentPaymentReduction)
            : 0;

        /// <summary>
        /// Gets the payoff days.
        /// </summary>
        /// <value>The payoff days.</value>
        public int PayoffDays => Balance > 0
            ? (int)Math.Ceiling(Balance / (CurrentPaymentReduction / 12))
            : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtInfo" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="balance">The balance.</param>
        /// <param name="rate">The rate.</param>
        /// <param name="minimum">The minimum.</param>
        /// <param name="forceMinPayment">if set to <c>true</c> [force minimum payment].</param>
        /// <exception cref="ArgumentOutOfRangeException">minimum - Current Payment ({CurrentPayment} is too low to pay the interest of {AverageMonthlyInterest}.</exception>
        public DebtInfo(string name, decimal balance, decimal rate, decimal minimum, bool forceMinPayment = true)
        {
            minPaymentMultiplier = MultiplierSetting.Get();
            ForceMinPayment = forceMinPayment;

            Name = name;
            Balance = balance;
            Rate = rate;
            Minimum = minimum;

            if (Balance > CurrentPayment &&
                CurrentPayment <= AverageMonthlyInterest)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum),
                                                      $"Current Payment ({CurrentPayment} is too low to pay the interest of {AverageMonthlyInterest}. ");
            }
        }

        /// <summary>
        /// Applies the payment.
        /// </summary>
        /// <returns>DebtInfo.</returns>
        public DebtInfo ApplyPayment()
        {
            Balance -= CurrentPaymentReduction;

            return this;
        }

        /// <summary>
        /// Rounds up.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="places">The places.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal RoundUp(decimal input, int places)
        {
            var multiplier = (int)Math.Ceiling(Math.Pow(10, Convert.ToDouble(places)));

            return Math.Ceiling(input * multiplier) / multiplier;
        }

        /// <summary>
        /// Gets the amortization.
        /// </summary>
        /// <param name="numberOfPayments">The number of payments.</param>
        /// <param name="additionalPayments">The additional payments.</param>
        /// <returns>DebtAmortization.</returns>
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
                                             $"{Balance,12:C} | {Rate,5:N}% | {Minimum,7:C} | {CurrentPayment,11:C}";
    }
}
