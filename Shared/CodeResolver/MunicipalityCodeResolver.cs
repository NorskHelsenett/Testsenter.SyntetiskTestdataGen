using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyntetiskTestdataGen.Shared.CodeResolver
{
    public interface IMunicipalityCodeResolver
    {
        string GetNameForCode(string municipalityCode);
    }

    // Note: Same concerns as for ZipCodeResolver.
    public class MunicipalityCodeResolver : IMunicipalityCodeResolver
    {
        private static readonly IDictionary<string, string> MunicipalityCodeToPlace = new Dictionary<string, string>();

        static MunicipalityCodeResolver()
        {
            //var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "MunicipalityCodes.txt");
            var lines = File.ReadAllLines(@"CodeResolver\MunicipalityCodes.txt", Encoding.GetEncoding(1252));
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                var items = line.Split(';');
                if (items.Length != 2)
                    throw new Exception("Found a postal code line that did not consist of two semicolon-separated items");
                var code = items[0].Trim().PadLeft(4, '0');
                MunicipalityCodeToPlace[code] = items[1].Trim().ToUpper();
            }
        }

        public static string GetNameForCode(string municipalityCode)
        {
            if (string.IsNullOrWhiteSpace(municipalityCode))
                return "";
            municipalityCode = municipalityCode.PadLeft(4, '0');
            string place;
            return MunicipalityCodeToPlace.TryGetValue(municipalityCode, out place) ? place : "";
        }

        string IMunicipalityCodeResolver.GetNameForCode(string municipalityCode)
        {
            return GetNameForCode(municipalityCode);
        }
    }
}
