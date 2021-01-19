using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebtPlanner
{
    public class DebtPortfolio : List<DebtInfo>
    {
        public virtual string Header
        {
            get
            {
                var builder = new StringBuilder();

                foreach (var keyValuePair in GetAmortization())
                {
                    var debtInfo = keyValuePair.Key;
                    var debtAmortization = keyValuePair.Value;
                    builder.AppendLine($"\n{debtInfo}");
                    builder.AppendLine($"\nNumber Payments: {debtAmortization.Count}\n");
                    builder.AppendLine(debtAmortization.ToString());
                }

                return builder.ToString();
            }
        }

        public virtual string Payments
        {
            get
            {
                var list = GetAmortization();
                var builder = new StringBuilder();

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
                             orderedList.Add(aCount, item);
                         });
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
