using System;
using System.IO;
using System.Linq;
using DebtPlanner;
using DebtPlanner.Data;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DebtPlannerTests
{
    [TestClass]
    public class SerializationTests
    {
        private readonly DebtPortfolio debtPortfolio = new DebtPortfolio
        {
            new DebtInfo("AB", 12345.67M, 1.2M, 240),
            new DebtInfo("CD", 10234.67M, 12.3M, 175),
            new DebtInfo("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 9345.67M, 1.2M, 150),
            new DebtInfo("EF", 9035.67M, 4.5M, 120),
            new DebtInfo("GH", 7245.67M, 19M, 90),
            new DebtInfo("IJ", 5345.67M, 15.50M, 70),
            new DebtInfo("KL", 2345.67M, 20M, 50),
        };

        private const string DEBT_STRING =
            "[{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"AB\",\"Balance\":12345.67,\"Rate\":1.0,\"OriginalMinimum\":240.0,\"Minimum\":240.0,\"MinimumPercent\":0.01944,\"DailyPr\":0.000027397260,\"AverageMonthyPr\":0.0008333333,\"DailyInterest\":0.34,\"AverageMonthlyInterest\":10.29,\"CurrentPayment\":240.0,\"CurrentPaymentReduction\":229.71,\"PayoffMonths\":54,\"PayoffDays\":645},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"CD\",\"Balance\":10234.67,\"Rate\":12.0,\"OriginalMinimum\":175.0,\"Minimum\":175.0,\"MinimumPercent\":0.01710,\"DailyPr\":0.000328767123,\"AverageMonthyPr\":0.01,\"DailyInterest\":3.37,\"AverageMonthlyInterest\":102.35,\"CurrentPayment\":175.0,\"CurrentPaymentReduction\":72.65,\"PayoffMonths\":141,\"PayoffDays\":1691},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\",\"Balance\":9345.67,\"Rate\":1.0,\"OriginalMinimum\":150.0,\"Minimum\":150.0,\"MinimumPercent\":0.01605,\"DailyPr\":0.000027397260,\"AverageMonthyPr\":0.0008333333,\"DailyInterest\":0.26,\"AverageMonthlyInterest\":7.79,\"CurrentPayment\":150.0,\"CurrentPaymentReduction\":142.21,\"PayoffMonths\":66,\"PayoffDays\":789},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"EF\",\"Balance\":9035.67,\"Rate\":4.0,\"OriginalMinimum\":120.0,\"Minimum\":120.0,\"MinimumPercent\":0.01328,\"DailyPr\":0.000109589041,\"AverageMonthyPr\":0.0033333333,\"DailyInterest\":1.0,\"AverageMonthlyInterest\":30.12,\"CurrentPayment\":120.0,\"CurrentPaymentReduction\":89.88,\"PayoffMonths\":101,\"PayoffDays\":1207},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"GH\",\"Balance\":7245.67,\"Rate\":19.0,\"OriginalMinimum\":90.0,\"Minimum\":172.10,\"MinimumPercent\":0.02375,\"DailyPr\":0.000520547945,\"AverageMonthyPr\":0.0158333333,\"DailyInterest\":3.78,\"AverageMonthlyInterest\":114.73,\"CurrentPayment\":172.10,\"CurrentPaymentReduction\":57.37,\"PayoffMonths\":127,\"PayoffDays\":1516},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"IJ\",\"Balance\":5345.67,\"Rate\":16.0,\"OriginalMinimum\":70.0,\"Minimum\":106.92,\"MinimumPercent\":0.02000,\"DailyPr\":0.000438356164,\"AverageMonthyPr\":0.0133333333,\"DailyInterest\":2.35,\"AverageMonthlyInterest\":71.28,\"CurrentPayment\":106.92,\"CurrentPaymentReduction\":35.64,\"PayoffMonths\":150,\"PayoffDays\":1800},{\"AdditionalPayment\":0.0,\"ForceMinPayment\":true,\"Name\":\"KL\",\"Balance\":2345.67,\"Rate\":20.0,\"OriginalMinimum\":50.0,\"Minimum\":58.65,\"MinimumPercent\":0.02500,\"DailyPr\":0.000547945205,\"AverageMonthyPr\":0.0166666667,\"DailyInterest\":1.29,\"AverageMonthlyInterest\":39.1,\"CurrentPayment\":58.65,\"CurrentPaymentReduction\":19.55,\"PayoffMonths\":120,\"PayoffDays\":1440}]";

        [TestMethod]
        public void ConvertToJson()
        {
            var str = JsonConvert.SerializeObject(debtPortfolio);
            debtPortfolio.Should().Equals(JsonConvert.DeserializeObject(str));
        }

        [TestMethod]
        public void ConvertFromJson()
        {
            var obj = JsonConvert.DeserializeObject(DEBT_STRING);
            debtPortfolio.Should().Equals(obj);
        }

        [TestMethod]
        public void WriteRead_Default()
        {
            var data = new DebtFileStorage();
            data.Save(debtPortfolio);
            File.Exists(data.FilePath).Should().BeTrue();
            var obj = data.Load();
            obj.Should().HaveCount(debtPortfolio.Count);
            obj.ForEach(x =>
                            debtPortfolio
                                .First(y => y.Name == x.Name)
                                .ToString()
                                .Should()
                                .Be(x.ToString()));
        }

        [TestMethod]
        public void Read_Default()
        {
            var data = new DebtFileStorage();
            var obj = data.Load();
            obj.Should().HaveCount(debtPortfolio.Count);
            obj.ForEach(x =>
                            debtPortfolio
                                .First(y => y.Name == x.Name)
                                .ToString()
                                .Should()
                                .Be(x.ToString()));
        }

        [TestMethod]
        public void WriteRead_CustomKey()
        {
            const string TEST_S = "PRETEnd P@sword Goes Here!Sometimes.";
            var data = new DebtFileStorage(null,
                                           "DebtFile.Custom.dat",
                                           TEST_S.ToSecureString());
            data.Save(debtPortfolio);
            File.Exists(data.FilePath).Should().BeTrue();
            var obj = data.Load();
            obj.Should().HaveCount(debtPortfolio.Count);
            obj.ForEach(x =>
                            debtPortfolio
                                .First(y => y.Name == x.Name)
                                .ToString()
                                .Should()
                                .Be(x.ToString()));
        }

        [TestMethod]
        public void WriteRead_CustomDirFile()
        {
            var data = new DebtFileStorage(new DirectoryInfo(Environment.CurrentDirectory),
                                           "testData.file.dat");
            data.Save(debtPortfolio);
            File.Exists(data.FilePath).Should().BeTrue();
            var obj = data.Load();
            obj.Should().HaveCount(debtPortfolio.Count);
            obj.ForEach(x =>
                            debtPortfolio
                                .First(y => y.Name == x.Name)
                                .ToString()
                                .Should()
                                .Be(x.ToString()));
        }

        [TestMethod]
        public void FileLocation()
        {
            var currentTime = DateTime.Now;
            const string NEW_FILE = "testData.file.dat";
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var newDirectory = directory.CreateSubdirectory("test");

            var data = new DebtFileStorage(newDirectory,
                                           NEW_FILE);
            data.Save(debtPortfolio);
            var savedFile = new FileInfo(Path.Combine(newDirectory.ToString(), NEW_FILE));
            savedFile.Exists.Should().BeTrue();
            savedFile.LastWriteTime.Should().BeOnOrAfter(currentTime);
        }
    }
}
