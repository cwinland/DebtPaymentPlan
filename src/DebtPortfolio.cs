using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebtPlanner
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global

    /// <summary>
    /// Class DebtPortfolio.
    /// Implements the <see cref="List{T}" />
    /// </summary>
    /// <seealso cref="List{DebtInfo}" />
    public class DebtPortfolio : List<DebtInfo>
    {
        public virtual string Summary
        {
            get
            {
                var builder = new StringBuilder();
                builder.AppendLine("Summary:");

                if (Count == 0)
                {
                    builder.AppendLine("No Debt! Congratulations!");

                    return builder.ToString();
                }

                this.OrderBy(x => x.Name)
                    .ToList()
                    .ForEach(x => builder.AppendLine($"{x}\n"));

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>The header.</value>
        public virtual string Header
        {
            get
            {
                if (Count == 0)
                {
                    return string.Empty;
                }

                var builder = new StringBuilder();
                builder.Append("Header:");

                foreach (var keyValuePair in GetAmortization())
                {
                    var debtInfo = keyValuePair.Key;
                    var debtAmortization = keyValuePair.Value;
                    builder.AppendLine($"\n{debtInfo}");
                    builder.AppendLine($"\nNumber Payments: {debtAmortization.Count}\n");

                    builder.AppendLine("Schedule:");
                    builder.AppendLine(debtAmortization.ToString());
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <value>The payments.</value>
        public virtual string Payments
        {
            get
            {
                var list = GetAmortization();
                var builder = new StringBuilder();
                builder.AppendLine("Payments:");

                if (list.Count == 0)
                {
                    builder.AppendLine("No Payments!\n");

                    return builder.ToString();
                }

                var maxPayments = list.Values.ToList().Max(x => x.Count);

                for (var i = 0; i < maxPayments; i++)
                {
                    var paymentNum = i + 1;
                    builder.AppendLine(
                        $"Payment {paymentNum,3}: {list.Values.Sum(x => x.Count > i ? x[i].Payment : 0),11:C}");
                }

                builder.AppendLine($"{"Total",11}: {list.Values.Sum(x => x.Sum(y => y.Payment)),11:C}");

                return builder.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtPortfolio"/> class.
        /// </summary>
        public DebtPortfolio() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtPortfolio"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public DebtPortfolio(IEnumerable<DebtInfo> list) : this() => AddRange(list);

        /// <summary>
        /// Gets the amortization.
        /// </summary>
        /// <returns>Dictionary&lt;DebtInfo, DebtAmortization&gt;.</returns>
        public virtual Dictionary<DebtInfo, DebtAmortization> GetAmortization()
        {
            var amList = new Dictionary<DebtInfo, DebtAmortization>();
            var orderedList = new SortedList<int, DebtInfo>();
            this.Where(info => info.Balance > 0)
                .ToList()
                .ForEach(x =>
                         {
                             var item = new DebtInfo(x.Name,
                                                     x.Balance,
                                                     x.Rate,
                                                     x.OriginalMinimum,
                                                     x.ForceMinPayment);
                             var aCount = item.GetAmortization().Count;

                             while (orderedList.ContainsKey(aCount))
                             {
                                 aCount++;
                             }

                             orderedList.Add(aCount, item);
                         });

            if (orderedList.Count == 0)
            {
                return amList;
            }

            var working = orderedList.FirstOrDefault().Value;
            var workingAm = working.GetAmortization();
            var workingAmount = working.OriginalMinimum;
            var additionalAmounts =
                new List<Tuple<int, decimal>> { new Tuple<int, decimal>(workingAm.Count, workingAmount), };
            amList.Add(working, workingAm);
            orderedList.RemoveAt(0);

            while (orderedList.Count > 0)
            {
                var item = orderedList.FirstOrDefault().Value;
                var am = item.GetAmortization(null, additionalAmounts);
                item.AdditionalPayment = am.Max(x => x.Payment) - item.Minimum;
                amList.Add(item, am);
                workingAmount += item.OriginalMinimum;
                additionalAmounts = new List<Tuple<int, decimal>> { new Tuple<int, decimal>(am.Count, workingAmount), };
                orderedList.RemoveAt(0);
            }

            return amList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(Header);

            builder.AppendLine("");
            builder.AppendLine(Payments);

            return builder.ToString();
        }
    }
}
