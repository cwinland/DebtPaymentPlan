using System;
using System.IO;
using System.Security;
using EncryptamajigCore;
using Newtonsoft.Json;

namespace DebtPlanner.Data
{
    /// <summary>
    /// Class DebtFileStorage.
    /// </summary>
    public class DebtFileStorage
    {
        /// <summary>
        /// The default file name
        /// </summary>
        public const string DefaultFileName = "DebtFile.dat";

        /// <summary>
        /// The default path
        /// </summary>
        public static readonly string DefaultPath = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public DirectoryInfo DirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        protected internal SecureString Key { get; set; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath => Path.Combine(DirectoryPath.FullName, FileName);

        /// <summary>
        /// Initializes a new instance of the <see cref="DebtFileStorage"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="securityKey">The security key.</param>
        public DebtFileStorage(DirectoryInfo path = null, string fileName = null, SecureString securityKey = null)
        {
            Key = securityKey ?? GenerateSecureString();

            DirectoryPath = path != null &&
                            path.Exists
                ? path
                : new DirectoryInfo(DefaultPath);

            FileName = !string.IsNullOrWhiteSpace(fileName)
                ? fileName
                : DefaultFileName;
        }

        /// <summary>
        /// Saves the specified portfolio.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        public void Save(DebtPortfolio portfolio)
        {
            var data = JsonConvert.SerializeObject(portfolio);

            var eData = AesEncryptamajig.Encrypt(data, Key.ToString());

            using (var sw = new StreamWriter(FilePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.AutoCompleteOnClose = true;
                writer.CloseOutput = true;
                writer.WriteRawValue(eData);
                writer.Close();
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <returns>DebtPortfolio.</returns>
        public DebtPortfolio Load() => JsonConvert.DeserializeObject<DebtPortfolio>(
            AesEncryptamajig.Decrypt(File.ReadAllText(FilePath), Key.ToString()));

        private static SecureString GenerateSecureString() => (Environment.MachineName +
                                                               Environment.ProcessorCount.ToString()
                                                                          .PadLeft(Environment.MachineName.Length))
            .ToSecureString();
    }
}
