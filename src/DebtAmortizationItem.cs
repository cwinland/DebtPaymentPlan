namespace DebtPlanner
{
    public class DebtAmortizationItem
    {
        private double CurrentBalance { get; }
        private decimal MonthlyRate { get; }
        public string Name => debtInfo.Name;
        public double Payment { get; }
        public double Interest => DebtInfo.RoundUp(MonthlyRate * (decimal)CurrentBalance, 2);
        public double AppliedPayment => Payment - Interest;
        public decimal RemainingBalance => (decimal)(CurrentBalance - AppliedPayment);

        public readonly DebtInfo debtInfo;

        public DebtAmortizationItem(DebtInfo debt)
        {
            debtInfo = debt;
            Payment = debt.CurrentPayment;
            CurrentBalance = debt.Balance;
            MonthlyRate = Payment >= CurrentBalance ? 0 : debt.AverageMonthyPr;
            debtInfo = new DebtInfo(Name, (double)RemainingBalance, debt.Rate, Payment);
        }

        public DebtAmortizationItem(string name, double payment, double rate, double currentBalance) : this(
            new DebtInfo(name, currentBalance, rate, payment)) { }

        /// <inheritdoc />
        public override string ToString() =>
            $"{Payment,10:C} | {Interest,8:C} | {AppliedPayment,10:C} | {RemainingBalance,12:C}";
    }
}
