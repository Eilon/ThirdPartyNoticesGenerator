using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ThirdPartyNoticeGenerator
{
    class Program
    {
        private static readonly string ThirdPartyNoticeTitleFormat = "License notices for {0}{1}";

        static void Main(string[] args)
        {
            var licenseEntries = GetThirdPartyLicenseEntries();
            var thirdPartyNoticesHeader = File.ReadAllText("ThirdPartyNoticesHeader.txt");

            var outputPath = "output";
            Directory.CreateDirectory(outputPath);

            using (var fullThirdPartyLicenseFile = File.CreateText(Path.Combine(outputPath, "ThirdPartyNotices.txt")))
            {
                // Write header
                fullThirdPartyLicenseFile.Write(thirdPartyNoticesHeader);

                // Copy each individual license
                foreach (var licenseEntry in licenseEntries)
                {
                    var thirdPartyNoticeTitle =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            ThirdPartyNoticeTitleFormat,
                            licenseEntry.PackageId,
                            string.IsNullOrEmpty(licenseEntry.PackageVersion)
                                ? string.Empty
                                : (" " + licenseEntry.PackageVersion));
                    var thirdPartyNoticeUnderline =
                        new string('-', thirdPartyNoticeTitle.Length);

                    var licenseFile = Path.Combine("licenses", licenseEntry.LicenseFile);
                    var licenseContents = File.ReadAllText(licenseFile, Encoding.UTF8);

                    if (string.IsNullOrEmpty(licenseContents))
                    {
                        throw new InvalidOperationException($"License file {licenseFile} is empty.");
                    }

                    fullThirdPartyLicenseFile.WriteLine();
                    fullThirdPartyLicenseFile.WriteLine(thirdPartyNoticeTitle);
                    fullThirdPartyLicenseFile.WriteLine(thirdPartyNoticeUnderline);
                    fullThirdPartyLicenseFile.WriteLine();
                    fullThirdPartyLicenseFile.WriteLine(licenseContents);
                }
            }
        }

        private static ThirdPartyLicense[] GetThirdPartyLicenseEntries()
        {
            var jsonSerializer = new JsonSerializer();
            using (var thirdPartyLicenseListStream = File.OpenText("thirdpartylicenses.json"))
            using (var thirdPartyLicenseJsonReader = new JsonTextReader(thirdPartyLicenseListStream))
            {
                return jsonSerializer.Deserialize<ThirdPartyLicense[]>(thirdPartyLicenseJsonReader);
            }
        }
    }

    public class ThirdPartyLicense
    {
        /// <summary>
        /// The id of the package.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// The version of the package.
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// The name of the license file in the 'licenses' folder.
        /// </summary>
        public string LicenseFile { get; set; }
    }
}
