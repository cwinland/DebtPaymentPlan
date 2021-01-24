# Debt Payment Planner and Payoff Intelligence

Library to calculate the best way to pay down your debt, sort, and produce a payment plan for the fastest payoff.

## Feature Summary

- Amortization Schedule
- Order debt to payoff the fastest.
- Debt Snowball
  - Order the debt so the bulk of the payments go toward the debt that will be paid off earlier.
  - As each card is paid off, that money is combined and added to the next card to be paid off quickly.
- Payment Statistics: Number of payments, range, utilization, APRs, etc.
- Save / Load Data

## Create a Portfolio Object

A portfolio combines all the debts to ensure that the amortization and payments are optimized.

```c#
var portfolio = new DebtPortfolio
            {
                new DebtInfo("A", 16345.44M, 3.25M, 225),
                new DebtInfo("B", 12000, 0, 125),
                new DebtInfo("C", 6000, 3.5M, 182),
                new DebtInfo("D", 4000, 12.25M, 50),
                new DebtInfo("E", 2000, 15.55M, 200),
                new DebtInfo("F", 1000, 22, 50),
                new DebtInfo("G", 500, 22, 50),
                new DebtInfo("H", 10, 50.3M, 250),
                new DebtInfo("I", 13000, 12, 100),
            };
```

### DebtInfo

```c#
// public DebtInfo(string name, decimal balance, decimal rate, decimal minPayment, bool forceMinPayment = true)
```

- Name: Name of Debt
- Balance: Current outstanding balance
- Rate: Annual Percentage Rate
- MinPayment: Minimum payment allowed to the debt
- ForceMinPayment: Indicates to allow the system to increase the minimum if it detects a payment issue

## Sample Output

The debt summary shows the debt statistics and the amortization schedule.
This is a calculation available by executing or overriding:

```c#
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
```

```text
Name: E

Balance      | % Rate | Minimum | Max Payment
------------ | ------ | ------- | -----------
   $2,000.00 | 15.55% | $200.00 |     $450.00

Number Payments: 6

Payment   1:    $200.00 |   $25.92 |    $174.08 |    $1,825.92
Payment   2:    $450.00 |   $23.67 |    $426.33 |    $1,399.59
Payment   3:    $450.00 |   $18.14 |    $431.86 |      $967.73
Payment   4:    $450.00 |   $12.55 |    $437.45 |      $530.28
Payment   5:    $450.00 |    $6.88 |    $443.12 |       $87.16
Payment   6:     $87.16 |    $0.00 |     $87.16 |        $0.00

```

## Sample Payment Summary

Payment summary shows all payments combined for all debts. Each payment is the sum of all debt for that payment.
This is a calculation available by executing or overriding:

```c#
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
```

```text
Payment   1:   $1,098.26
Payment   2:   $1,338.26
Payment   3:   $1,338.26
Payment   4:   $1,338.26
Payment   5:   $1,338.26
...
Payment  47:   $1,327.00
Payment  48:   $1,327.00
Payment  49:   $1,327.00
Payment  50:     $750.34
      Total:  $62,927.47
```

## Release Notes

- Current
  - Add File save/load with encryption

- Version 1.21.1.1922
  - Initial Release

## License

- Copyright(c) 2021 Christopher Winland
- [Apache-2.0 License]<https://github.com/cwinland/DebtPaymentPlan/blob/master/LICENSE>
