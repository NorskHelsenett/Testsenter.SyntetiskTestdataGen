using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyntetiskTestdataGen.Shared.CodeResolver
{


    // Note: I'm really in doubt about whether the postal codes should be denormalized into the Persons table 
    // (which will create a nightmare whenever some postal code changes), or whether we should join Persons and PostalCodes at query time,
    // or whether we should just keep it in memory like we do now (partially because this is a quick fix).
    public class ZipCodeResolver 
    {
        private static readonly IDictionary<string, string> ZipCodeToPlace = new Dictionary<string, string>();
        private static readonly IDictionary<string, List<string>> PlaceToZipCodes = new Dictionary<string, List<string>>();

        static ZipCodeResolver()
        {
            var lines = File.ReadAllLines(@"CodeResolver\ZipCodes.txt", Encoding.GetEncoding(1252));
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                var items = line.Split(';');
                if (items.Length != 2)
                    throw new Exception("Found a postal code line that did not consist of two semicolon-separated items");
                var code = items[0].Trim().PadLeft(4, '0');
                ZipCodeToPlace[code] = items[1].Trim().ToUpper();
            }
            foreach (var zip in ZipCodeToPlace)
            {
                if (!PlaceToZipCodes.ContainsKey(zip.Value.ToUpper()))
                    PlaceToZipCodes.Add(zip.Value.ToUpper(), new List<string>());
                PlaceToZipCodes[zip.Value.ToUpper()].Add(zip.Key);
            }
        }

        public static string GetNameForCode(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                return "";
            zipCode = zipCode.PadLeft(4, '0');
            string place;
            return ZipCodeToPlace.TryGetValue(zipCode, out place) ? place : "";
        }
    }
}
