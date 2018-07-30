using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public class PersonStatistics : StatisticsContainer<PersonStatistics>
    {
        public RelationshipStatistics RelationshipStatistics { get; set; }
        public SivilstatusStatistics SivilstatusStatistics { get; set; }
        public RegstatusStatistics RegstatusStatistics { get; set; }
        public NameStatistics NameStatistics { get; set; }
        public AdressStatistics AdressStatistics { get; set; }

        public DiscreteStatistic Kjonn { get; set; }
        public DiscreteStatistic HasDnummer_Kjonn { get; set; }
        public Statistic Age { get; set; }


        //regcodes
        public BooleanStatistic IsUkjent { get; set; } //-1000
        public BooleanStatistic IsBosatt { get; set; } //1
        public BooleanStatistic IsUtflyttet { get; set; } //2
        public BooleanStatistic IsUtvandret { get; set; } //3
        public BooleanStatistic IsForsvunnet { get; set; } //4
        public BooleanStatistic IsDead { get; set; } //5
        public BooleanStatistic IsUtgaatFodselsnummer { get; set; } //6
        public BooleanStatistic IsFodselsregistrert { get; set; } //7
        public BooleanStatistic IsAnnullertTilgang { get; set; } //8
        public BooleanStatistic IsUregistrertTilgang { get; set; } //9

        public BooleanStatistic HasFirstname { get; set; }
        public BooleanStatistic HasLastname { get; set; }

        public BooleanStatistic HasFather { get; set; }
        public BooleanStatistic HasMother { get; set; }
        public BooleanStatistic HasSpouse { get; set; }
        public BooleanStatistic HasDnummer { get; set; }
        public BooleanStatistic HasValidNin { get; set; }
        public DiscreteStringStatistics Citizenship_And_CitizenshipCode { get; set; }
        public BooleanStatistic HasCitizenship { get; set; }
        public DateStatistics DateCitizenship { get; set; }
        public DiscreteStringStatistics Custody { get; set; }
        public BooleanStatistic HasCustody { get; set; }
        public DateStatistics DateCustody { get; set; }
        public DiscreteStringStatistics WithoutLegalCapacity { get; set; }
        public BooleanStatistic HasWithoutLegalCapacity { get; set; }
        public DateStatistics DateWithoutLegalCapacity { get; set; }
        public BooleanStatistic HasDufNo { get; set; }
        public DiscreteStatistic MaritalStatus { get; set; }
        public BooleanStatistic HasOldNIN { get; set; }
        public BooleanStatistic HasNewNIN { get; set; }
        public DiscreteStatistic ParentalResponsibility { get; set; }
        public DateStatistics DateParentalResponsibility { get; set; }
        public DiscreteStatistic RegStatus { get; set; }
        public DiscreteStatistic StatusCountryCode { get; set; }
        public BooleanStatistic HasStatusCountryCode { get; set; }
        public DateStatistics DateWorkPermit { get; set; }
        public DiscreteStringStatistics WorkPermit { get; set; }
        public BooleanStatistic HasWorkPermit { get; set; }
        public DiscreteStringStatistics PostalCode { get; set; }
        public BooleanStatistic HasPostalCode { get; set; }
        public DependentDiscreteStatistic RegStatus_Vs_PostalType { get; set; }


        public static PersonStatistics CreateRoot(string sessionId, int ageQuants, int numberOfSamplesToTake, int expectedNumberOfAgeQuants)
        {
            var ps = new PersonStatistics(sessionId, numberOfSamplesToTake, ageQuants, null)
            {
                StatisticsByAgeQuants = new Dictionary<int, PersonStatistics>(),
                AgeQuants = ageQuants
            };

            for (int i = 0; i < expectedNumberOfAgeQuants; i++)
                ps.StatisticsByAgeQuants.Add(i, new PersonStatistics(sessionId, numberOfSamplesToTake, ageQuants, i));

            return ps;
        }

        public PersonStatistics(string sessionId, int numberOfSamplesToTake, int ageQuants, int? ageQuantLevel) : base(sessionId, numberOfSamplesToTake, ageQuants, ageQuantLevel)
        {
            RelationshipStatistics = new RelationshipStatistics(numberOfSamplesToTake);
            SivilstatusStatistics = new SivilstatusStatistics(numberOfSamplesToTake);
            NameStatistics = ageQuantLevel.HasValue ? null : new NameStatistics(numberOfSamplesToTake);
            RegstatusStatistics = new RegstatusStatistics(numberOfSamplesToTake);
            AdressStatistics = ageQuantLevel.HasValue ? null : new AdressStatistics(numberOfSamplesToTake);
            RegStatus_Vs_PostalType = new DependentDiscreteStatistic(numberOfSamplesToTake, "RegStatus_Vs_PostalType", (p,a) => new Tuple<double?, double?>(p.RegStatus, AdressStatistics.GetCurrent(p.Addresses)?.PostalType));

            CustomModels.Add("RelationshipStatistics", RelationshipStatistics);
            CustomModels.Add("SivilstatusStatistics", SivilstatusStatistics);
            CustomModels.Add("NameStatistics", NameStatistics);
            CustomModels.Add("RegstatusStatistics", RegstatusStatistics);
            CustomModels.Add("AdressStatistics", AdressStatistics);
            CustomModels.Add("RegStatus_Vs_PostalType", RegStatus_Vs_PostalType);

            Kjonn = new DiscreteStatistic(numberOfSamplesToTake, "Kjonn", CorrelationFactory, (p, a) =>
            {
                if (NinModel.IsValidNin(p.NIN) && !CommonFunctions.HasDnummer(p.NIN))
                    return CommonFunctions.GetKjonn(p.NIN).Value;

                return Statistic.DontUpdateWhenThisValue;
            });

            HasDnummer_Kjonn = new DiscreteStatistic(numberOfSamplesToTake, "HasDnummer_Kjonn", CorrelationFactory,
                (p, a) =>
                {
                    if (NinModel.IsValidNin(p.NIN) && CommonFunctions.HasDnummer(p.NIN))
                        return CommonFunctions.GetKjonn(p.NIN).Value;

                    return Statistic.DontUpdateWhenThisValue;
                });

            HasDnummer = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasDnummer, CorrelationFactory, (p, a) =>
            {
                if (NinModel.IsValidNin(p.NIN))
                    return CommonFunctions.HasDnummer(p.NIN) ? 1 : 0;

                return Statistic.DontUpdateWhenThisValue;
            });

            Age = new Statistic(numberOfSamplesToTake, PersonStatisticKeys.Age, CorrelationFactory, (p,a) => p.DateOfBirth.HasValue? CommonFunctions.GetAge(p.DateOfBirth.Value).GetValue() : null);

            IsDead = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsDead, CorrelationFactory, (p,a) => IsPersonDead(p));
            IsUkjent = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsUkjent, CorrelationFactory, (p, a) => HasUkjentRegCode(p)); //-1000
            IsBosatt = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsBosatt, CorrelationFactory, (p, a) => HasRegcode(p, 1)); //1
            IsUtflyttet = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsUtflyttet, CorrelationFactory, (p, a) => HasRegcode(p, 2)); //2
            IsUtvandret = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsUtvandret, CorrelationFactory, (p, a) => HasRegcode(p, 3)); //3
            IsForsvunnet = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsForsvunnet, CorrelationFactory, (p, a) => HasRegcode(p, 4)); //4
            IsUtgaatFodselsnummer = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsUtgaatFodselsnummer, CorrelationFactory, (p, a) => HasRegcode(p, 6)); //6
            IsFodselsregistrert = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsFodselsregistrert, CorrelationFactory, (p, a) => HasRegcode(p, 7)); //7
            IsAnnullertTilgang = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsAnnullertTilgang, CorrelationFactory, (p, a) => HasRegcode(p, 8)); //8
            IsUregistrertTilgang = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.IsUregistrertTilgang, CorrelationFactory, (p, a) => HasRegcode(p, 9)); //9

            HasFirstname = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasFirstname, CorrelationFactory, (p, a) => string.IsNullOrEmpty(p.GivenName) || string.IsNullOrWhiteSpace(p.GivenName) ? 0 : 1);
            HasLastname = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasLastname, CorrelationFactory, (p, a) => string.IsNullOrEmpty(p.Sn) || string.IsNullOrWhiteSpace(p.Sn) ? 0 : 1);

            HasFather = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasFather, CorrelationFactory, (p,a) => String.IsNullOrEmpty(p.FathersNIN) ? 0 : 1);
            HasMother = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasMother, CorrelationFactory, (p, a) => String.IsNullOrEmpty(p.MothersNIN) ? 0 : 1);
            HasSpouse = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasSpouse, CorrelationFactory, (p, a) => String.IsNullOrEmpty(p.SpouseNIN) ? 0 : 1);
            Citizenship_And_CitizenshipCode = new DiscreteStringStatistics(numberOfSamplesToTake, ageQuantLevel, PersonStatisticKeys.Citizenship_And_CitizenshipCode, CorrelationFactory, (p,a) => (string.IsNullOrEmpty(p.Citizenship) ? "null" : p.Citizenship) + "_" + (string.IsNullOrEmpty(p.CitizenshipCode) ? "null" : p.CitizenshipCode));
            HasCitizenship = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasCitizenship, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.Citizenship) ? 0 : 1);
            Custody = new DiscreteStringStatistics(numberOfSamplesToTake, ageQuantLevel, PersonStatisticKeys.Custody, CorrelationFactory, (p,a) => p.Custody);
            HasCustody = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasCustody, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.Custody) ? 0 : 1);
            WithoutLegalCapacity = new DiscreteStringStatistics(numberOfSamplesToTake, ageQuantLevel, PersonStatisticKeys.WithoutLegalCapacity, CorrelationFactory, (p,a) => p.WithoutLegalCapacity);
            HasWithoutLegalCapacity = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasWithoutLegalCapacity, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.WithoutLegalCapacity) ? 0 : 1);
            HasValidNin = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasValidNin, CorrelationFactory, (p,a) => NinModel.IsValidNin(p.NIN) ? 1 : 0);
            HasDufNo = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasDufNo, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.DufNo) ? 0 : 1);
            MaritalStatus = new DiscreteStatistic(numberOfSamplesToTake, PersonStatisticKeys.MaritalStatus, CorrelationFactory, (p,a) => p.MaritalStatus.GetValue());
            HasOldNIN = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasOldNIN, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.OldNIN) ? 0 : 1);
            HasNewNIN = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasNewNIN, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.NewNIN) ? 0 : 1);
            ParentalResponsibility = new DiscreteStatistic(numberOfSamplesToTake, PersonStatisticKeys.ParentalResponsibility, CorrelationFactory, (p,a) => p.ParentalResponsibility.GetValue());
            RegStatus = new DiscreteStatistic(numberOfSamplesToTake, PersonStatisticKeys.RegStatus, CorrelationFactory, (p,a) => p.RegStatus.GetValue());
            StatusCountryCode = new DiscreteStatistic(numberOfSamplesToTake, PersonStatisticKeys.StatusCountryCode, CorrelationFactory, (p,a) => p.StatusCountryCode);
            HasStatusCountryCode = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasStatusCountryCode, CorrelationFactory, (p,a) => p.StatusCountryCode.HasValue ? 1 : 0);
            WorkPermit = new DiscreteStringStatistics(numberOfSamplesToTake, ageQuantLevel, PersonStatisticKeys.WorkPermit, CorrelationFactory, (p,a) => p.WorkPermit);
            HasWorkPermit = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasWorkPermit, CorrelationFactory, (p,a) => string.IsNullOrEmpty(p.WorkPermit) ? 0 : 1);
            PostalCode = new DiscreteStringStatistics(numberOfSamplesToTake, ageQuantLevel, PersonStatisticKeys.PostalCode, CorrelationFactory, (p,a) => p.Addresses != null && p.Addresses.Any() ? p.Addresses.First().PostalCode : null);
            HasPostalCode = new BooleanStatistic(numberOfSamplesToTake, PersonStatisticKeys.HasPostalCode, CorrelationFactory, (p,aa) => p.Addresses != null && p.Addresses.Any(a => a.CurrentAddress && !string.IsNullOrEmpty(a.PostalCode)) ? 1 : 0);

            DateWorkPermit = new DateStatistics(numberOfSamplesToTake, PersonStatisticKeys.DateWorkPermit, CorrelationFactory, (p, a) => new Tuple<string,DateTime?, DateTime?>(p.WorkPermit, p.DateWorkPermit, p.DateOfBirth));
            DateParentalResponsibility = new DateStatistics(numberOfSamplesToTake, PersonStatisticKeys.DateParentalResponsibility, CorrelationFactory, (p, a) => new Tuple<string, DateTime?, DateTime?>(p.ParentalResponsibility?.ToString(), p.DateParentalResponsibility, p.DateOfBirth));
            DateWithoutLegalCapacity = new DateStatistics(numberOfSamplesToTake, PersonStatisticKeys.DateWithoutLegalCapacity, CorrelationFactory, (p, a) => new Tuple<string, DateTime?, DateTime?>(p.WithoutLegalCapacity, p.DateWithoutLegalCapacity, p.DateOfBirth));
            DateCustody = new DateStatistics(numberOfSamplesToTake, PersonStatisticKeys.DateCustody, CorrelationFactory, (p, a) => new Tuple<string, DateTime?, DateTime?>(p.Custody, p.DateCustody, p.DateOfBirth));
            DateCitizenship = new DateStatistics(numberOfSamplesToTake, PersonStatisticKeys.DateCitizenship, CorrelationFactory, (p, a) => new Tuple<string, DateTime?, DateTime?>(p.Citizenship, p.DateCitizenship, p.DateOfBirth));

            AddEntitiesToBaseDictionary<BooleanStatistic>();
            AddEntitiesToBaseDictionary<DiscreteStringStatistics>();
            AddEntitiesToBaseDictionary<DiscreteStatistic>();
            AddEntitiesToBaseDictionary<DateStatistics>();

            Statistics.Add(Age.Name, Age);
        }

        private void AddEntitiesToBaseDictionary<T>() where T : Statistic
        {
            PropertyInfo[] properties = typeof(PersonStatistics).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (p.PropertyType != typeof(T))
                    continue; 

                var stat = (T) p.GetValue(this);
                Statistics.Add(stat.Name, stat);
            }
        }

        public static int IsPersonDead(Person p)
        {
            return HasRegcode(p, 5);
        }
        
        public static int HasRegcode(Person p, int regCode)
        {
            return p.RegStatus.HasValue && p.RegStatus.Value == regCode ? 1 : 0;
        }

        public static int HasUkjentRegCode(Person p)
        {
            return (!p.RegStatus.HasValue || p.RegStatus.Value == -1000) ? 1 : 0;
        }

        private static int _validRangeMin, _validRangeMax;
        private static bool CanResolveBirthdayFromNin(string personNin)
        {
            if (_validRangeMin == 0)
            {
                int oldestPossible = 150;
                int oldestPossibleBdayYear = DateTime.UtcNow.Year - oldestPossible;
                _validRangeMin = int.Parse((DateTime.UtcNow.Year + 1).ToString().Substring(2, 2));
                _validRangeMax = int.Parse(oldestPossibleBdayYear.ToString().Substring(2, 2));
            }

            var bdayYearFromNin = int.Parse(personNin.Substring(4, 2));

            return bdayYearFromNin >= _validRangeMin && bdayYearFromNin <= _validRangeMax;
        }

        public static int InvalidQuant = -1000000;
        public static int CalculateAgeQuant(Person person, int ageQuantLevel, out bool birthdayWasFoundFromNin)
        {
            if (!person.DateOfBirth.HasValue)
            {
                birthdayWasFoundFromNin = true;
                if (!CanResolveBirthdayFromNin(person.NIN))
                    return InvalidQuant;

                var dateOfBirth = CommonFunctions.GetBirthdayFromNin(person.NIN);
                if (!dateOfBirth.HasValue)
                {
                    var yearIdentifier = CommonFunctions.GetYearFromNin(person.NIN);
                    dateOfBirth = CommonFunctions.GetBirthdayFromNin("0101" + yearIdentifier.ToString().Substring(2,2) + "00000");
                }

                person.DateOfBirth = dateOfBirth.Value.DateTime;
            }
            else
                birthdayWasFoundFromNin = false;

            var age = CommonFunctions.GetAge(person.DateOfBirth.Value);

            return age / ageQuantLevel;
        }

        public PersonStatistics GetClosestStatisticByAgeQuant(PersonWithMetadata person, Func<PersonStatistics, bool> isValidForMe = null)
        {
            var ageQ = person.AgeQuants;

            if (ageQ <= 0)
                return StatisticsByAgeQuants.ContainsKey(1) ? StatisticsByAgeQuants[1] : this;

            do
            {
                if (StatisticsByAgeQuants.ContainsKey(ageQ) && (isValidForMe == null || isValidForMe(StatisticsByAgeQuants[ageQ])))
                    return StatisticsByAgeQuants[ageQ];

            } while (ageQ-- > 0);

            return this;
        }
    }
}
