using System;
using System.Collections.Generic;
using System.Linq;
using AppConfigSettings;
using AppConfigSettings.Enum;
using Newtonsoft.Json;

namespace DebtPlanner
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <summary>
    /// Class DebtInfo.
    /// </summary>
    public class DebtInfo
    {
        private const string MULTIPLIER_SETTING = "PaymentMultiplier";

        private readonly decimal minPaymentMultiplier;

        private decimal balance;

        /// <summary>
        /// Gets or sets the additional payment.
        /// </summary>
        /// <value>The additional payment.</value>
        public decimal AdditionalPayment { get; set; }

        /// <summary>
        /// Gets the original minimum.
        /// </summary>
        /// <value>The original minimum.</value>
        private decimal CurrentMinimum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force minimum payment].
        /// </summary>
        /// <value><c>true</c> if [force minimum payment]; otherwise, <c>false</c>.</value>
        public bool ForceMinPayment { get; set; }

        /// <summary>
        /// Interest rate calendar for automatic changes.
        /// </summary>
        public InterestCalendar FutureRatesCalendar { get; internal set; } = new InterestCalendar();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        public decimal OriginalMinimum { get; private set; }

        protected internal static ConfigSetting<decimal> MultiplierSetting { get; set; } =
            new ConfigSetting<decimal>(MULTIPLIER_SETTING, 1.5M, SettingScopes.Any, num => num >= 0M);

        /// <summary>
        /// Gets the balance.
        /// </summary>
        /// <value>The balance.</value>
        public decimal Balance { get => balance; internal set => balance = Math.Round(value, 2); }

        public DebtInfo()
        {
            ForceMinPayment = true;
            minPaymentMultiplier = MultiplierSetting.Get();
        }

        public DebtInfo(string name, decimal balance, decimal rate, decimal minimum) : this(name,
            balance,
            rate,
            minimum,
            minimum,
            true,
            new InterestCalendar()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtInfo" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="balance">The balance.</param>
        /// <param name="rate">The rate.</param>
        /// <param name="currentMinimum"></param>
        /// <param name="originalMinimum"></param>
        /// <param name="forceMinPayment">if set to <c>true</c> [force minimum payment].</param>
        /// <param name="interestCalendar">Future interest rates calendar, if applicable.</param>
        /// <exception cref="ArgumentOutOfRangeException">minimum - Current Payment ({CurrentPayment} is too low to pay the interest of {AverageMonthlyInterest}.</exception>
        [JsonConstructor]
        public DebtInfo(
            string name, decimal balance, decimal rate, decimal currentMinimum, decimal originalMinimum,
            bool forceMinPayment,
            InterestCalendar interestCalendar) : this()
        {
            ForceMinPayment = forceMinPayment;

            Name = name;
            Balance = balance;
            SetInterestRate(rate);
            OriginalMinimum = originalMinimum;
            SetMinimum(Math.Min(Math.Max(currentMinimum, originalMinimum), Balance));

            if (interestCalendar != null)
            {
                foreach (var keyValuePair in interestCalendar.Where(keyValuePair =>
                                                                        !FutureRatesCalendar.ContainsKey(
                                                                            keyValuePair.Key)))
                {
                    FutureRatesCalendar.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (Balance > GetCurrentPayment() &&
                GetCurrentPayment() <= GetAverageMonthlyInterest())
            {
                throw new ArgumentOutOfRangeException(nameof(currentMinimum),
                                                      $"Current Payment ({GetCurrentPayment()} is too low to pay the interest of {GetAverageMonthlyInterest()}. ");
            }
        }

        /// <summary>
        /// Get interest rate for the specified/optional date.
        /// </summary>
        /// <param name="date">Date for interest rate. If omitted, today's date is used.</param>
        /// <returns>Interest Rate Decimal.</returns>
        public decimal GetInterestRate(DateTime? date = null)
        {
            var searchDate = date ?? DateTime.Today;
            var rate = FutureRatesCalendar
                       .Where(x => x.Key <= searchDate)
                       .OrderByDescending(x => x.Key)
                       .ToList()
                       .First()
                       .Value;

            return Math.Round(rate, 2);
        }

        /// <summary>
        /// Set interest rate for optional date.
        /// </summary>
        /// <param name="rateDecimal">Interest rate in percent.</param>
        /// <param name="date">Date which interest rate will apply.</param>
        /// <returns>Current interest rate as of specified/optional date.</returns>
        public decimal SetInterestRate(decimal rateDecimal, DateTime? date = null)
        {
            var searchDate = date ?? DateTime.Today;
            FutureRatesCalendar[searchDate] = rateDecimal;

            return GetInterestRate(searchDate);
        }

        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public decimal GetMinimum(DateTime? dateAsOf = null) => Math.Min(Balance,
                                                                         ForceMinPayment
                                                                             ? Math.Max(
                                                                                 Math.Round(
                                                                                     GetAverageMonthlyInterest(
                                                                                         dateAsOf) *
                                                                                     minPaymentMultiplier,
                                                                                     2),
                                                                                 CurrentMinimum)
                                                                             : CurrentMinimum);

        /// <summary>
        /// Sets the minimum.
        /// </summary>
        /// <param name="value"></param>
        public void SetMinimum(decimal value) => CurrentMinimum = Math.Round(Math.Max(value, OriginalMinimum), 2);

        /// <summary>
        /// Gets the minimum percent.
        /// </summary>
        /// <value>The minimum percent.</value>
        public decimal MinimumPercent(DateTime? dateAsOf = null) => Balance > 0 && GetMinimum(dateAsOf) > 0
            ? Math.Round(GetMinimum(dateAsOf) / Balance, 5)
            : 0;

        /// <summary>
        /// Gets the average monthy pr.
        /// </summary>
        /// <value>The average monthy pr.</value>
        public decimal GetAverageMonthyPr(DateTime? dateAsOf = null) => GetInterestRate(dateAsOf) > 0
            ? Math.Round(GetInterestRate(dateAsOf) / 100 / 12, 10)
            : 0;

        /// <summary>
        /// Gets the average monthly interest.
        /// </summary>
        /// <value>The average monthly interest.</value>
        public decimal GetAverageMonthlyInterest(DateTime? dateAsOf = null) =>
            RoundUp(GetAverageMonthyPr(dateAsOf) * Balance, 2);

        /// <summary>
        /// Gets the current payment.
        /// </summary>
        /// <value>The current payment.</value>
        public decimal GetCurrentPayment(DateTime? dateAsOf = null) =>
            Balance > 0 ? Math.Min(GetMinimum(dateAsOf) + AdditionalPayment, Balance) : 0;

        /// <summary>
        /// Gets the current payment reduction.
        /// </summary>
        /// <value>The current payment reduction.</value>
        public decimal GetCurrentPaymentReduction(DateTime? dateAsOf = null) => GetCurrentPayment(dateAsOf) > 0
            ? GetCurrentPayment(dateAsOf) - GetAverageMonthlyInterest(dateAsOf)
            : 0;

        /// <summary>
        /// Gets the payoff months.
        /// </summary>
        /// <value>The payoff months.</value>
        public int GetPayoffMonths(DateTime? dateAsOf = null) => Balance > 0
            ? (int)Math.Ceiling(Balance / GetCurrentPaymentReduction(dateAsOf))
            : 0;

        /// <summary>
        /// Gets the payoff days.
        /// </summary>
        /// <value>The payoff days.</value>
        public int GetPayoffDays(DateTime? dateAsOf = null) => Balance > 0
            ? (int)Math.Ceiling(Balance / (GetCurrentPaymentReduction(dateAsOf) / 12))
            : 0;

        /// <summary>
        /// Applies the payment.
        /// </summary>
        /// <returns>DebtInfo.</returns>
        public DebtInfo ApplyPayment(DateTime? dateAsOf = null)
        {
            Balance -= GetCurrentPaymentReduction(dateAsOf);

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
            int? numberOfPayments = null, List<(int OrderNumber, decimal Amount)> additionalPayments = null)
        {
            var result = new DebtAmortization();
            var currentDebt = this;
            var paymentsMade = 0;

            if (additionalPayments == null)
            {
                additionalPayments = new List<(int OrderNumber, decimal Amount)>();
            }

            var additionalPayment = additionalPayments
                                    .OrderBy(x => x.OrderNumber)
                                    .FirstOrDefault();

            if (additionalPayment != default &&
                GetAmortization().Count <= additionalPayment.OrderNumber)
            {
                return GetAmortization(numberOfPayments);
            }

            var today = DateTime.Today;

            while (currentDebt.Balance > 0 &&
                   (!numberOfPayments.HasValue || numberOfPayments > 0))
            {
                if (additionalPayment != default &&
                    additionalPayment.OrderNumber == paymentsMade)
                {
                    currentDebt.AdditionalPayment += additionalPayment.Amount;
                }

                var debtAmortizationItem = new DebtAmortizationItem(currentDebt, today);
                result.Add(debtAmortizationItem);
                currentDebt = debtAmortizationItem.debtInfo;
                paymentsMade++;

                if (numberOfPayments.HasValue)
                {
                    numberOfPayments--;
                }

                today = today.AddMonths(1);
            }

            return result;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public override string ToString() => ToString(DateTime.Today);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateAsOf"></param>
        /// <returns></returns>
        public string ToString(DateTime dateAsOf) => $"Name: {Name}\n" +
                                                     $"{"Balance".PadRight(12)} | % Rate | Minimum | Max Payment\n" +
                                                     "---------------------------------------------\n" +
                                                     $"{Balance,12:C} | {GetInterestRate(dateAsOf),5:N}% | {GetMinimum(dateAsOf),7:C} | {GetCurrentPayment(dateAsOf),11:C}";
    }
}
