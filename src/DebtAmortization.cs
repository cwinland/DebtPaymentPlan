using System.Collections.Generic;
using System.Text;

namespace DebtPlanner
{
    public class DebtAmortization : List<DebtAmortizationItem> {
        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var i = 0; i < Count; i++)
            {
                var paymentNum = i + 1;
                builder.AppendLine($"Payment {paymentNum,3}: {this[i]}");
            }

            return builder.ToString();
        }
    }
}
