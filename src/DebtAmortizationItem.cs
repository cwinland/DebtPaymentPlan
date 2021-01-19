namespace DebtPlanner
{
    public class DebtAmortizationItem
    {
        private decimal CurrentBalance { get; }
        private decimal MonthlyRate { get; }
        public string Name => debtInfo.Name;
        public decimal Payment { get; }
        public decimal Interest => DebtInfo.RoundUp(MonthlyRate * CurrentBalance, 2);
        public decimal AppliedPayment => Payment - Interest;
        public decimal RemainingBalance => (decimal)(CurrentBalance - AppliedPayment);

        public readonly DebtInfo debtInfo;

        public DebtAmortizationItem(DebtInfo debt)
        {
            debtInfo = debt;
            Payment = debt.CurrentPayment;
            CurrentBalance = debt.Balance;
            MonthlyRate = Payment >= CurrentBalance ? 0 : debt.AverageMonthyPr;
            debtInfo = new DebtInfo(Name, RemainingBalance, debt.Rate, Payment);
        }

        public DebtAmortizationItem(string name, decimal payment, decimal rate, decimal currentBalance) : this(
            new DebtInfo(name, currentBalance, rate, payment)) { }

        /// <inheritdoc />
        public override string ToString() =>
            $"{Payment,10:C} | {Interest,8:C} | {AppliedPayment,10:C} | {RemainingBalance,12:C}";
    }
}
