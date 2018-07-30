using System;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class NameModel : BaseModel, IPregModel
    {
        public NameRandomzier NameGenerator { get; set; }
        public NameRandomzier SameNameGenerator { get; set; }
        private readonly Func<PersonWithMetadata, double> _pHasFirstname;
        private readonly Func<PersonWithMetadata, double> _pHasMiddlename;
        private readonly Func<PersonWithMetadata, int> _givenHasFirstNameHowManyNames;
        private readonly Func<PersonWithMetadata, int> _givenHasMiddleNameHowManyNames;
        private readonly Func<PersonWithMetadata, int> _givenHasHasSirnameHowManyNames;
        private readonly Func<PersonWithMetadata, double> _givenHasMultipleSirnameUseBindestrek;

        private readonly Func<PersonWithMetadata, bool> _pHasMaidenName = (person) =>
        {
            if (string.IsNullOrEmpty(person.Person.NIN))
                return false;

            var isFemale = CommonFunctions.IsFemale(person.Person.NIN);
            if (!isFemale.HasValue)
                return false;

            return isFemale.Value ? person.Randy.Hit(23) : person.Randy.Hit(7); //hentet fra SQL statistikk
        };
        public int DoubleSirname { get; set; }
        private readonly Func<PersonWithMetadata, double> _pHasSirname;

        private const int PIsSame = 1; //ca 40 000 samer i norge er ca 1 %
        private const int MaxLength = 49;

        public static Func<PersonWithMetadata, double> PHasSamesirnameAsParents = person =>
        {
            if (person.AgeQuants < 5)
                return 70;
            if (person.AgeQuants < 8)
                return 40;

            return 10;
        };

        public NameModel(SynteticDataBuilderV1 databuilder) : base(databuilder)
        {
            _pHasFirstname = (person) => _model.Statistics.NameStatistics.HasFirstname.TrueRatioPercent;
            _pHasMiddlename = (person) => _model.Statistics.NameStatistics.HasMiddlename.TrueRatioPercent;
            _pHasSirname = (person) => _model.Statistics.NameStatistics.HasSirname.TrueRatioPercent;
            _givenHasFirstNameHowManyNames = (person) => _model.Statistics.NameStatistics.GivenHasFirstName_HowManyNames.Sample(person.Randy) ?? 0;
            _givenHasMiddleNameHowManyNames = (person) => _model.Statistics.NameStatistics.GivenHasMiddleName_HowManyNames.Sample(person.Randy) ?? 0;
            _givenHasHasSirnameHowManyNames = (person) => _model.Statistics.NameStatistics.GivenHasHasSirname_HowManyNames.Sample(person.Randy) ?? 0;
            _givenHasMultipleSirnameUseBindestrek = (person) => _model.Statistics.NameStatistics.HasMultipleSirname_UseBindestrek.TrueRatioPercent;

            Setup();
        }

        public NameModel(int pHasFirstname, int pHasMiddlename, int pHasSirname)
        {
            _pHasFirstname = (person) => pHasFirstname;
            _pHasMiddlename = (person) => pHasMiddlename;
            _pHasSirname = (person) => pHasSirname;

            _givenHasFirstNameHowManyNames = (person) => person.Randy.Next(3);
            _givenHasMiddleNameHowManyNames = (person) => person.Randy.Next(3);
            _givenHasHasSirnameHowManyNames = (person) => person.Randy.Next(3);
            _givenHasMultipleSirnameUseBindestrek = (person) => 30;

            Setup();
        }

        public static DateTime? GetDateWhenNameRegistered(Person person)
        {
            if (!person.DateOfBirth.HasValue)
                return (DateTime?) null;

            var age = CommonFunctions.GetAge(person.DateOfBirth.Value);

            if (age < 25 || age % 2 == 0)
                return person.DateOfBirth.Value;

            return person.DateOfBirth.Value.AddYears(age % 7).AddDays(age % 5);
        }

        private void Setup()
        {
            DoubleSirname = 15;
            NameGenerator = new NameRandomzier(GetCsvPaths(), true);
            SameNameGenerator = new NameRandomzier(GetSameCsvPaths(), false);
        }

        public void SetProperties(PersonWithMetadata person)
        {
            var isSame = person.Randy.Hit(PIsSame);

            SetFornavn(person, isSame ? SameNameGenerator : NameGenerator);
            SetMellomnavn(person, isSame ? SameNameGenerator : NameGenerator);
            SetEtternavn(person, isSame ? SameNameGenerator : NameGenerator);
            SetMaidenName(person, isSame ? SameNameGenerator : NameGenerator);
        }

        private void SetMaidenName(PersonWithMetadata person, NameRandomzier nameRandomzier)
        {
            if (!_pHasMaidenName(person))
                return;

            person.Person.WithoutLegalCapacity = GetEtternavn(nameRandomzier, person); //"låner" WithoutLegalCapacity, siden det ikke brukes 
        }

        private void SetEtternavn(PersonWithMetadata person, NameRandomzier nameGenerator)
        {
            if (!person.Randy.Hit(_pHasSirname(person)))
                return;

            person.Person.Sn = GetEtternavn(nameGenerator, person);
        }

        private string GetEtternavn(NameRandomzier nameGenerator, PersonWithMetadata person)
        {
            var howMany = _givenHasHasSirnameHowManyNames(person);

            var name = "";
            var lastWasBindestrek = false;
            for (int i = 0; i < howMany; i++)
            {
                var addThis = nameGenerator.NextSirname(person.Randy);
                if ((name + addThis).Length > MaxLength)
                    break;

                name += addThis;

                if (i != howMany - 1)
                {
                    var useBindestrek = !lastWasBindestrek && person.Randy.Hit(_givenHasMultipleSirnameUseBindestrek(person));
                    if (useBindestrek)
                    {
                        name += "-";
                        lastWasBindestrek = true;
                    }
                    else
                    {
                        name += " ";
                        lastWasBindestrek = false;
                    }
                }

            }

            if (name.EndsWith("-"))
                name = name.Substring(0, name.LastIndexOf("-", StringComparison.Ordinal) - 1);

            return name;
        }

        private void SetMellomnavn(PersonWithMetadata person, NameRandomzier nameGenerator)
        {
            if (!person.Randy.Hit(_pHasMiddlename(person)))
                return;

            if (person.IsFemale)
                person.Person.MiddleName = GetName(_givenHasMiddleNameHowManyNames(person), person.Person.GivenName, person.Randy, nameGenerator.NextFemaleMiddlename);
            else
                person.Person.MiddleName = GetName(_givenHasMiddleNameHowManyNames(person), person.Person.GivenName, person.Randy, nameGenerator.NextMaleMiddlename);

        }

        private void SetFornavn(PersonWithMetadata person, NameRandomzier nameGenerator)
        {
            if (!person.Randy.Hit(_pHasFirstname(person)))
                return;

            if(person.IsFemale)
                person.Person.GivenName = GetName(_givenHasFirstNameHowManyNames(person), person.Randy, nameGenerator.NextFemaleFirstname);
            else
                person.Person.GivenName = GetName(_givenHasFirstNameHowManyNames(person), person.Randy, nameGenerator.NextMaleFirstname);
        }

        private string GetName(int howMany, string arg, Randomizer randy, Func<string, Randomizer, string> nameGenerator)
        {
            var name = "";
            for (int i = 0; i < howMany; i++)
            {
                var addThis = nameGenerator(arg, randy);
                if ((name + addThis).Length > MaxLength)
                    break;

                name += addThis;
                name += " ";
            }

            name = name.Trim();
            return name;
        }

        private string GetName(int howMany, Randomizer randy, Func<Randomizer, string> nameGenerator, string delimiter = " ")
        {
            var name = "";
            for (int i = 0; i < howMany; i++)
            {
                var addThis = nameGenerator(randy);
                if ((name + addThis).Length > MaxLength)
                    break;

                name += addThis;

                if(i != howMany -1)
                    name += delimiter;
            }

            name = name.Trim();
            return name;
        }

        private string[] GetCsvPaths()
        {
            var bae = @"";
            return new[] {bae + "fornavn-jente.csv", bae + "fornavn-gutt.csv", bae + "etternavn.csv"};
        }

        private string[] GetSameCsvPaths()
        {
            var bae = @"";
            return new[] { bae + "sameFemaleNames.txt", bae + "sameMaleNames.txt", bae + "sameSirnames.txt" };
        }
    }
}
