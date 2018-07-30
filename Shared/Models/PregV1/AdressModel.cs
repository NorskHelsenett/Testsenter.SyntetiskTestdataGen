using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class AdressModel : BaseModel, IPregModel
    {
        private readonly NameModel _nameModel;
        private Dictionary<string, List<ClaimableAddress>> _adresses;
        private Dictionary<string, Tuple<string, string>> _postalDictionary;
        private Dictionary<string, string> _municipalityDictionary;
        private readonly AdressStatistics _adressStatistics;
        private List<Tuple<string, string, string>> _adressLines;
        public AdressModel(SynteticDataBuilderV1 databuilder) : base(databuilder)
        {
            _nameModel = new NameModel(databuilder);
            _adressStatistics = _model.Statistics.AdressStatistics;
            GenerateAdressList();
            SetPostalcodesAndMunicipalityCodes();
        }

        public void SetProperties(PersonWithMetadata person)
        {
            var howManyAdressObjects = _model.Statistics.AdressStatistics.NumberOfAdressObjects.Sample(person.Randy);
            if (!howManyAdressObjects.HasValue || howManyAdressObjects == 0)
                return;

            if (person.Person.Addresses == null)
                person.Person.Addresses = new Address[howManyAdressObjects.Value];

            for (int i = 0; i < howManyAdressObjects; i++)
            {
                var adressObject = GetAdressObject(person);
                if (i == 0)
                    adressObject.CurrentAddress = true;

                adressObject.PostalType = _model.Statistics.AdressStatistics.PostalType.Sample(person.Randy);
                adressObject.DatePostalType = adressObject.PostalType.HasValue && _adressStatistics.HasDatePostalPlace.Sample(person.Randy) ? DateTime.Now.AddDays(person.Randy.Next(500)) : (DateTime?)null;

                if (_model.Statistics.AdressStatistics.HasDateAdrFrom.Sample(person.Randy))
                    adressObject.DateAdrFrom = NinModel.GetBirthday(person.Randy.Next(person.GetAge()), person.Randy);

                if (person.HasDnummer)
                {
                    adressObject.St = GetRekvirentKode(person.Randy);
                }

                adressObject.NIN = person.Person.NIN;
                person.Person.Addresses[i] = adressObject;
            }

            ReplacePostalcodeForSammenslaatteKommuner(person.Person);
        }

        private void ReplacePostalcodeForSammenslaatteKommuner(Person person)
        {
            if (person.Addresses == null || !person.Addresses.Any())
                return;

            foreach (var adr in person.Addresses)
            {
                if (!string.IsNullOrEmpty(adr.St) && KommuneMapping.ContainsKey(adr.St))
                    adr.St = KommuneMapping[adr.St];
            }
        }


        private static readonly Dictionary<string, string> KommuneMapping = new Dictionary<string, string>()
        {
            {"1601", "5001"},
            {"1702", "5004"},
            {"1703", "5005"},
            {"1612", "5011"},
            {"1613", "5012"},
            {"1617", "5013"},
            {"1620", "5014"},
            {"1621", "5015"},
            {"1622", "5016"},
            {"1627", "5017"},
            {"1630", "5018"},
            {"1632", "5019"},
            {"1633", "5020"},
            {"1634", "5021"},
            {"1635", "5022"},
            {"1636", "5023"},
            {"1638", "5024"},
            {"1640", "5025"},
            {"1644", "5026"},
            {"1648", "5027"},
            {"1653", "5028"},
            {"1657", "5029"},
            {"1662", "5030"},
            {"1663", "5031"},
            {"1664", "5032"},
            {"1665", "5033"},
            {"1711", "5034"},
            {"1714", "5035"},
            {"1717", "5036"},
            {"1719", "5037"},
            {"1721", "5038"},
            {"1724", "5039"},
            {"1725", "5040"},
            {"1736", "5041"},
            {"1738", "5042"},
            {"1739", "5043"},
            {"1740", "5044"},
            {"1742", "5045"},
            {"1743", "5046"},
            {"1744", "5047"},
            {"1748", "5048"},
            {"1749", "5049"},
            {"1750", "5050"},
            {"1751", "5051"},
            {"1755", "5052"},
            {"1756", "5053"},
            {"1624", "5054"},
            {"1718", "5054"},
            {"0702", "0715"},
            {"0714", "0715"},
            {"0709", "0712"},
            {"0728", "0712"},
            {"0722", "0729"},
            {"0723", "0729"}
        };

        private static int[] _rekvirentCodes = new[]
        {
            -1,
            2100,
            2300,
            9901,
            9902,
            9903,
            9904,
            9905,
            9906,
            9907,
            9908,
            9909,
            9910,
            9911,
            9912,
            9913
        };

        private string GetRekvirentKode(Randomizer r)
        {
            var index = r.Next(_rekvirentCodes.Length);
            var value = _rekvirentCodes[index];
            return value == -1 ? null : value.ToString();
        }

        public static Address Clone(Address s)
        {
            var ser = JsonConvert.SerializeObject(s);
            return JsonConvert.DeserializeObject<Address>(ser);
        }

        public static int? IndexOfCurrentAdress(Address[] adresses)
        {
            if (adresses == null)
                return null;
            for (int i = 0; i < adresses.Length; i++)
                if (adresses[i].CurrentAddress)
                    return i;

            return null;
        }

        public static Func<PersonWithMetadata, double> PSpousesSameAdress = (person) => 90;

        public static Func<PersonWithMetadata, double> PSameAdressAsParents = (person) =>
        {
            if (person.AgeQuants <= 4)
                return 70;

            if (person.AgeQuants <= 7)
                return 35;

            return 5;
        };

        public static double PParentsSameAdress = 50;

        #region BuildModel
        private void GenerateAdressList()
        {
            _adressLines = new List<Tuple<string, string, string>>();
            _adresses = new Dictionary<string, List<ClaimableAddress>>();

            using (TextReader sr = GetEmbeddedResource(encodingName:null))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("["))
                        line = line.Replace("[", "");
                    if (line.EndsWith(","))
                        line = line.Remove(line.Length - 1);
                    if (line.EndsWith("]"))
                        line = line.Remove(line.Length - 1);

                    if(String.IsNullOrEmpty(line) || line.Contains("]["))
                        continue;

                    var element = JsonConvert.DeserializeObject<AdressElement>(line);

                    var postalCode = new String(element.postalnumber.Where(Char.IsDigit).ToArray());

                    if(element.address.Length <= 4)
                        continue;

                    var adress = new ClaimableAddress
                    {
                        AdressElement = element
                    };

                    if(!_adresses.ContainsKey(postalCode))
                        _adresses.Add(postalCode, new List<ClaimableAddress>());

                    _adresses[postalCode].Add(adress);
                    _adressLines.Add(new Tuple<string, string, string>(element.place, element.address, element.postalnumber));
                }
            }

        }

        private void SetPostalcodesAndMunicipalityCodes()
        {
            _postalDictionary = new Dictionary<string, Tuple<string, string>>();
            _municipalityDictionary = new Dictionary<string, string>();

            using (StreamReader sr = GetEmbeddedResource("Postnummerregister_ansi.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var fragments = line.Split('\t').Select(x => x.Trim()).ToArray();
                    var postalCode = fragments[0];
                    var postalName = fragments[1];
                    var municipalityCode = fragments[2];
                    var municipalityName = fragments[3];

                    if(!_postalDictionary.ContainsKey(postalCode))
                        _postalDictionary.Add(postalCode, new Tuple<string, string>(postalName, municipalityCode));

                    if(!_municipalityDictionary.ContainsKey(municipalityCode))
                        _municipalityDictionary.Add(municipalityCode, municipalityName);
                }


            }
        }

        private Address GetAdressObject(PersonWithMetadata person)
        {
            var hasPostalCode = MultivariateBinaryGenerator.Hit(PersonStatisticKeys.HasPostalCode, person.BooleanSamples, person.Randy.Hit(_model.Statistics.GetClosestStatisticByAgeQuant(person).HasPostalCode.TrueRatioPercent));

            string postalCode = null;
            while(postalCode == null)
                postalCode = _model.Statistics.PostalCode.Sample(person.Randy);

            if (!_adresses.ContainsKey(postalCode) || _adresses[postalCode].All(a => a.Claimed))
                MakeAdresses(postalCode, 100, person);

            var adress = _adresses[postalCode].First(x => !x.Claimed);

            if (adress.AdressElement != null)
            {
                ConvertFromAdressElement(adress, person);
            }

            adress.Claimed = true;

            if (!hasPostalCode)
                adress.PostalCode = null;

            return adress;
        }

        private Address MakeAdressForTheDead(PersonWithMetadata person)
        {
            var a = new Address
            {
                St = _postalDictionary[_postalDictionary.ElementAt(person.Randy.Next(_postalDictionary.Keys.Count)).Key]
                    .Item2,
            };

            SetAdressLines(a, person);

            return a;
        }

        private void ConvertFromAdressElement(ClaimableAddress address, PersonWithMetadata person)
        {
            var postalCode = new String(address.AdressElement.postalnumber.Where(Char.IsDigit).ToArray());
            var postalPlace = address.AdressElement.postalnumber.Replace(postalCode, "").Trim();

            var hasSt = _adressStatistics.HasSt.Sample(person.Randy);
            address.PostalCode = _adressStatistics.HasPostalCode.Sample(person.Randy) ? postalCode : null;
            address.PostalPlace = _adressStatistics.HasPostalPlace.Sample(person.Randy) ? postalPlace : null;
            address.PostalAddress = _adressStatistics.HasPostalAdress.Sample(person.Randy) ? address.AdressElement.address : null;
            address.St = hasSt && _postalDictionary.ContainsKey(postalCode) ? _postalDictionary[postalCode].Item2 : null;
            address.Municipality = hasSt ? address.AdressElement.kommune.Replace(" kommune", "") : null;

            if (address.PostalAddress == null)
            {
                SetRestOfFields(address, "", "", 0, person);
                return;
            }

            var streetname = GetRandomName(_randomRoadPostfixes, person);
            string postalAdressLetter = null;
            int? postalAdressNumber = null;
            var splittedStreet = address.PostalAddress.Split(' ');
            var streetNumberAndChar = splittedStreet.Last();

            if (splittedStreet.Length > 1)
            {
                streetname = "";
                for (int i = 0; i < splittedStreet.Length - 1; i++)
                    streetname = splittedStreet[i] + " ";

                if (streetname.EndsWith(" "))
                    streetname = streetname.Substring(0, streetname.Length - 1);
            }

            //due to a bug in address.json 
            if (splittedStreet.Length == 1 && !string.IsNullOrEmpty(address.AdressElement.place))
            {
                Trace.WriteLine("Using " + address.AdressElement.place + " instead of " + address.AdressElement.address);
                address.PostalAddress = address.AdressElement.place;
                streetname = address.PostalAddress;
            }

            if (streetNumberAndChar.Any(Char.IsDigit))
            {
                var digits = Regex.Match(streetNumberAndChar, @"\d+").Value;
                int temppostalAdressNumber;
                var success = Int32.TryParse(digits, out temppostalAdressNumber);

                if (success && streetNumberAndChar.Contains(temppostalAdressNumber.ToString()))
                {
                    var withoutNumbers = streetNumberAndChar.Replace(temppostalAdressNumber.ToString(), "");
                    postalAdressLetter = !String.IsNullOrEmpty(withoutNumbers) ? withoutNumbers.FirstOrDefault(c => !Char.IsDigit(c)).ToString() : "";
                    postalAdressNumber = temppostalAdressNumber;
                }

                if (!success && _adressStatistics.HasHouseNumber.Sample(person.Randy))
                    postalAdressNumber = person.Randy.Next(1000);
            }

            if (!string.IsNullOrEmpty(streetname) && postalAdressNumber.HasValue)
                address.PostalAddress = streetname + " " + postalAdressNumber.Value;

            if (!string.IsNullOrEmpty(streetname) && !string.IsNullOrEmpty(postalAdressLetter))
                address.PostalAddress += " " + postalAdressLetter;

            SetRestOfFields(address, streetname, postalAdressLetter, postalAdressNumber, person);
        }

        private void MakeAdresses(string postalCode, int howmany, PersonWithMetadata person)
        {
            if(!_postalDictionary.ContainsKey(postalCode))
                _postalDictionary.Add(postalCode, new Tuple<string, string>(GetRandomName(_randomPlacePostfixes, person), _municipalityDictionary.Keys.ElementAt(person.Randy.Next(_municipalityDictionary.Keys.Count))));

            for (int i = 0; i < howmany; i++)
            {
                var adress = new ClaimableAddress();
                adress.Claimed = false;

                var streetname = GetRandomName(_randomRoadPostfixes, person);
                var postalAdressLetter = _adressStatistics.HasHouseLetter.Sample(person.Randy) ? person.Randy.NextLetter().ToString().ToUpper() : null; 
                int? postalAdressNumber = _adressStatistics.HasHouseNumber.Sample(person.Randy) ? person.Randy.Next(1000) : (int?) null;

                adress.PostalCode = postalCode;
                adress.PostalPlace = adress.PostalCode != null && _adressStatistics.HasPostalPlace.Sample(person.Randy) ? _postalDictionary[postalCode].Item1 : null;
                adress.PostalAddress = _adressStatistics.HasPostalAdress.Sample(person.Randy) ? streetname + " " + postalAdressNumber + postalAdressLetter : null;
                adress.St = _adressStatistics.HasSt.Sample(person.Randy) ? _postalDictionary[postalCode].Item2 : null;
                adress.Municipality = adress.St != null ? _municipalityDictionary[adress.St] : null;

                SetRestOfFields(adress, streetname, postalAdressLetter, postalAdressNumber, person);

                if (!_adresses.ContainsKey(postalCode))
                    _adresses.Add(postalCode, new List<ClaimableAddress>());

                _adresses[postalCode].Add(adress);
            }
        }

        private void SetRestOfFields(ClaimableAddress adress, string streetname, string postalAdressLetter, int? postalAdressNumber, PersonWithMetadata person)
        {
            adress.PostalAddress = adress.PostalAddress?.Trim();
            postalAdressLetter = postalAdressLetter?.ToUpper();

            adress.PostalAddressValidFrom = _adressStatistics.HasPostalAddressValidFrom.Sample(person.Randy) ? DateTime.Now.AddDays((-1) * person.Randy.Next(1500)) : (DateTime?)null;

            SetAdressLines(adress, person);

            adress.ApartmentNumber = _adressStatistics.HasAppartmentNumber.Sample(person.Randy) ? GetAppartmentNumber(person) : null;
            adress.BasicStatisticalUnit = _adressStatistics.HasBasicStatisticalUnit.Sample(person.Randy) ? person.Randy.Next(1000) : (int?)null;
            adress.CadastralNumber = _adressStatistics.HasCadastralNumber.Sample(person.Randy) ? person.Randy.Next(1000).ToString() : null;
            adress.CoAddress = _adressStatistics.HasCoAdress.Sample(person.Randy) ? Concatenate(_adressLines.ElementAt(person.Randy.Next(_adressLines.Count)), 25) : null;

            adress.DistrictCode = _adressStatistics.HasDistrictCodeandDistrictName.Sample(person.Randy) ? GetFourSiffers(true, null, person) : null;
            adress.DistrictName = adress.DistrictCode != null ? GetRandomName(_randomPlacePostfixes, person) : null;
            adress.Constituency = _adressStatistics.HasConstituency.Sample(person.Randy) ? GetFourSiffers(true, adress.DistrictCode, person) : null;
            adress.SchoolDistrict = _adressStatistics.HasSchoolDistrict.Sample(person.Randy) ? GetFourSiffers(true, null, person) : null;

            adress.Country = _adressStatistics.HasCountry.Sample(person.Randy) ? "Norge" : null;
            adress.HouseLetter = postalAdressLetter;
            adress.HouseNumber = postalAdressNumber;
            adress.PropertyNumber = _adressStatistics.HasPropertyNumber.Sample(person.Randy) ? person.Randy.Next(10000).ToString() : null;
            adress.StreetName = _adressStatistics.HasStreetname.Sample(person.Randy) ? streetname : null;
            adress.StreetNumber = adress.StreetName != null ? GetFourSiffers(false, null, person) : null;

            if (_adressStatistics.HasXcoordYcoord.Sample(person.Randy))
            {
                adress.XCoord = Decimal.Parse(person.Randy.Next(90) + "." + person.Randy.Next(90));
                adress.YCoord = Decimal.Parse(person.Randy.Next(90) + "." + person.Randy.Next(90));
            }
        }

        private void SetAdressLines(Address adress, PersonWithMetadata person)
        {
            var hasAddressLines = _adressStatistics.HasAdressLine12or3.Sample(person.Randy);
            if (hasAddressLines)
            {
                var adressLines = _adressLines.ElementAt(person.Randy.Next(_adressLines.Count));

                adress.AddressLine1 = _adressStatistics.HasAdressLine1_WhenHasAdressLine12or3.Sample(person.Randy) ? Max(adressLines.Item1, 30) : null;
                adress.AddressLine2 = _adressStatistics.HasAdressLine2_WhenHasAdressLine12or3.Sample(person.Randy) ? Max(adressLines.Item2, 30) : null;
                adress.AddressLine3 = _adressStatistics.HasAdressLine3_WhenHasAdressLine12or3.Sample(person.Randy) ? Max(adressLines.Item3, 30) : null;
            }
        }

        private string Max(string value, int numberOfChars)
        {
            if (value.Length < numberOfChars)
                return value;

            return value.Substring(numberOfChars);
        }

        private string GetAppartmentNumber(PersonWithMetadata person)
        {
            return "H0" + person.Randy.Next(10) + "0" + person.Randy.Next(10);
        }

        private string GetFourSiffers(bool startsWithZero, string endsWithSameAsThis, PersonWithMetadata person)
        {
            var s = "";

            s += startsWithZero ? "0" : person.Randy.Next(10).ToString();
            s += person.Randy.Next(10);
            s += person.Randy.Next(10);
            s += endsWithSameAsThis?.ElementAt(endsWithSameAsThis.Length-1).ToString() ?? person.Randy.Next(10).ToString();

            return s;
        }

        private string Concatenate(Tuple<string, string, string> elementAt, int maxNumberOfChars)
        {
            var s = Max(elementAt.Item1, maxNumberOfChars);
            if ((s.Length + elementAt.Item2.Length) > maxNumberOfChars)
                return s;

            s += elementAt.Item2;

            if ((s.Length + elementAt.Item3.Length) > maxNumberOfChars)
                return s;

            s += elementAt.Item3;

            return s;
        }

        private string GetRandomName(string[] postfixes, PersonWithMetadata person)
        {
            var prefix = person.Randy.Hit(50) ? _nameModel.NameGenerator.NextSirname(false, person.Randy) 
                : (person.Randy.Hit(50) ? _nameModel.NameGenerator.NextFemaleFirstname(person.Randy) : _nameModel.NameGenerator.NextMaleFirstname(person.Randy));

            var postfix = postfixes.ElementAt(person.Randy.Next(postfixes.Length));

            return prefix + postfix;
        }

        private static StreamReader GetEmbeddedResource(string filename = "adresses.json", string encodingName = "ISO-8859-1")
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string path = "SyntetiskTestdataGen.Shared";
            var encoding = encodingName != null ? Encoding.GetEncoding(encodingName) : Encoding.UTF8;

            return new StreamReader(thisAssembly.GetManifestResourceStream(path + ".Resources." + filename), encoding);
        }

        private string[] _randomRoadPostfixes = new[]
        {
            "gate"
            , "gata"
            , "gaten"
            , "vei"
            , "veien"
            , "veg"
            , "vegen"
            , "sti"
            , "stien"
            , "li"
            , "lia"
            , "spranget"
            , "strand"
            , "terrasse"
            , "kollen"
            , "allé"
            , "alléen"
            , "faret"
            , "bakken"
            , "bakke"
            , "haugen"
            , "hagen"
            , "stubben"
        };

        private string[] _randomPlacePostfixes = new[]
        {
            "lia"
            , "li"
            , "dal"
            , "dalen"
            , "stad"
            , "eng"
            , "engen"
            , "by"
            , "rud"
            , "støl"
            , "stølen"
            , "berg"
            , "land"
            , "landet"
            , "bostad"
            , "rike"
            , "mark"
            , "bu"
            , "set"
            , "tveit"
            , "bø"
            , "torp"
            , "rud"
            , "bøle"
            , "vik"
            , "vika"
            , "viken"
            , "fjord"
            , "åsen"
            , "kåk"
            , "voll"
            , "nes"
            , "skog"
            , "skogen"
            , "(s)øra"
            , "sund"
            , "sundet"
            , "bakke"
            , "(s)øyra"
            , "våg"
            , "vågen"
        };

        #endregion
    }

    public class AdressElement
    {
        public string address { get; set; }
        public string kommune { get; set; }
        public string type { get; set; }
        public string postalnumber { get; set; }
        public int postalnumberInt { get; set; }
        public string place { get; set; }
    }
}
