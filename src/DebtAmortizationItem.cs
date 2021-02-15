using System;

namespace DebtPlanner
{
    /// <summary>
    /// Class DebtAmortizationItem.
    /// </summary>
    public class DebtAmortizationItem
    {
        /// <summary>
        /// The debt information
        /// </summary>
        public readonly DebtInfo debtInfo;

        /// <summary>
        /// Gets the payment.
        /// </summary>
        /// <value>The payment.</value>
        public decimal Payment { get; }

        private decimal CurrentBalance { get; }
        private decimal MonthlyRate { get; }
        public decimal Rate { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => debtInfo.Name;

        /// <summary>
        /// Gets the interest.
        /// </summary>
        /// <value>The interest.</value>
        public decimal Interest => DebtInfo.RoundUp(MonthlyRate * CurrentBalance, 2);

        /// <summary>
        /// Gets the applied payment.
        /// </summary>
        /// <value>The applied payment.</value>
        public decimal AppliedPayment => Payment - Interest;

        /// <summary>
        /// Gets the remaining balance.
        /// </summary>
        /// <value>The remaining balance.</value>
        public decimal RemainingBalance => CurrentBalance - AppliedPayment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtAmortizationItem"/> class.
        /// </summary>
        /// <param name="debt">The debt.</param>
        /// <param name="dateAsOf"></param>
        public DebtAmortizationItem(DebtInfo debt, DateTime? dateAsOf = null)
        {
            var date = dateAsOf ?? DateTime.Today;

            debtInfo = debt;
            Payment = debt.GetCurrentPayment(date);
            CurrentBalance = debt.Balance;
            MonthlyRate = Payment >= CurrentBalance ? 0 : debt.GetAverageMonthyPr(date);

            Rate = debt.GetInterestRate(date);
            debtInfo = new DebtInfo(Name,
                                    RemainingBalance,
                                    debt.GetInterestRate(date),
                                    Payment,
                                    debt.OriginalMinimum,
                                    debt.ForceMinPayment,
                                    debt.FutureRatesCalendar);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{Payment,10:C} | {Rate,5:N}% | {Interest,8:C} | {AppliedPayment,10:C} | {RemainingBalance,12:C}";
    }
}
