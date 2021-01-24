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
        public DebtAmortizationItem(DebtInfo debt)
        {
            debtInfo = debt;
            Payment = debt.CurrentPayment;
            CurrentBalance = debt.Balance;
            MonthlyRate = Payment >= CurrentBalance ? 0 : debt.AverageMonthyPr;
            debtInfo = new DebtInfo(Name, RemainingBalance, debt.Rate, Payment);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{Payment,10:C} | {Interest,8:C} | {AppliedPayment,10:C} | {RemainingBalance,12:C}";
    }
}
