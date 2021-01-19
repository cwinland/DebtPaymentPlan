using System;
using System.Collections.Generic;
using System.Linq;

namespace DebtPlanner
{
    public class DebtPortfolio : List<DebtInfo>
    {
        public List<DebtInfo> PaidList => this.Where(info => info.Balance == 0).ToList();
        public List<DebtInfo> UnPaidList => this.Where(info => info.Balance > 0).ToList();

        public List<List<DebtAmortizationItem>> Amortizations =>
            UnPaidList.Select(info => info.GetAmortization()).ToList();

        public double AmountPaid => PaidList.Sum(info => info.OriginalMinimum);
        public int MaxLength => UnPaidList.Max(x => x.GetAmortization().Count);

        public Dictionary<DebtInfo, List<DebtAmortizationItem>> GetAmortization()
        {
            // Get First Am
            // Get Second Am
            // Change Second Am starting after last First Am
            var amList = new Dictionary<DebtInfo, List<DebtAmortizationItem>>();
            var orderedList = UnPaidList.OrderBy(info => info.GetAmortization().Count).ToList();
            var working = orderedList[0];
            var workingAm = working.GetAmortization();
            var workingAmount = working.OriginalMinimum;
            var additionalAmounts =
                new List<Tuple<int, double>> { new Tuple<int, double>(workingAm.Count, workingAmount), };
            amList.Add(working, workingAm);
            orderedList.RemoveAt(0);

            while (orderedList.Count > 0)
            {
                var item = orderedList[0];
                var am = item.GetAmortization(null, additionalAmounts);
                amList.Add(item, am);
                workingAmount += item.OriginalMinimum;
                additionalAmounts = new List<Tuple<int, double>> { new Tuple<int, double>(am.Count, workingAmount), };
                orderedList.RemoveAt(0);
            }

            return amList;
        }
    }
}
