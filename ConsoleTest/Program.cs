using System;
using DebtPlanner;
using DebtPlanner.Data;

namespace DebtPlannerConsoleTest
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedMember.Local
    internal class Program
    {
        private static DebtPortfolio portfolio = new DebtPortfolio();
        private static readonly DebtFileStorage debtFile = new DebtFileStorage();

        private static void Main()
        {
            // Complete InitDebt and uncomment to create the initial data file.
            //InitDebt();
            portfolio = Load();

            Console.WriteLine(portfolio.Summary);
            Console.WriteLine("Schedule(s):");
            Console.WriteLine(portfolio);
            Save();
        }

        private static void InitDebt()
        {
            // Use this to create initial debt file.
            portfolio = new DebtPortfolio { new DebtInfo("A", 12345, 3.25M, 225), new DebtInfo("B", 2180, 12M, 125), };
            Save();
        }

        private static void Save() => debtFile.Save(portfolio);

        private static DebtPortfolio Load() => debtFile.Load();
    }
}
