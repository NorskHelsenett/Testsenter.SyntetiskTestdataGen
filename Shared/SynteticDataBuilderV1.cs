using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Shared
{
    public class SynteticDataBuilderV1
    {
        private readonly IPushPregData _pusherPregData;
        private readonly IdentifierDublicateControl _idControl;
        public SynteticModel Model { get; set; }
        public IdentificatorModel IdentificatorModel { get; set; }
        public NameModel NameModel { get; set; }
        public RelationshipModel2 RelationshipModel { get; set; }
        public SivilstatusModel SivilstatusModel { get; set; }
        public RegstatusModel RegstatusModel { get; set; }
        public AdressModel AdressModel { get; set; }
        private Dictionary<string, PersonWithMetadata> _persons;
        private string _sessionId;
        private Randomizer _randomizer;

        public SynteticDataBuilderV1(SynteticModel model, IPushPregData pusherPregData, IdentifierDublicateControl idControl)
        {
            _pusherPregData = pusherPregData;
            _idControl = idControl;
            Model = model;
        }

        public void Init()
        {
            IdentificatorModel = new IdentificatorModel(this, _idControl);
            NameModel = new NameModel(this);
            RelationshipModel = GetRelationshipModel2(Model, new FamilyModel(this));
            RegstatusModel = new RegstatusModel(this);
            AdressModel = new AdressModel(this);
            SivilstatusModel = new SivilstatusModel(this);
            _randomizer = new Randomizer();
        }

        public bool Do(string sessionId, int bunchSize)
        {
            Init();

            _sessionId = sessionId;
            _randomizer.Buildup(bunchSize * 30, bunchSize *600, bunchSize * 6000);

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var nextBunch = DoNextBunch(bunchSize);
                _pusherPregData.AddToSaveQueue(nextBunch);
            }
            catch (Exception e)
            {
                ConsoleWriteline("Got exception in DoNextBunch(): " + e.Message);
                throw;
            }

            watch.Stop();
            ConsoleWriteline("Done with next bunch of " + bunchSize + ". It took " + watch.Elapsed.TotalMinutes + " minutes");

            return true;
        }

        private void ConsoleWriteline(string msg)
        {
            Outputter.WriteLine($"[Thread: {_sessionId}]: {msg}");
        }

        private IEnumerable<Person> DoNextBunch(int howMany)
        {
            _persons = new Dictionary<string, PersonWithMetadata>(howMany);
            var stopw = new Stopwatch();
            stopw.Start();
            while (howMany-- > 0)
            {
                var nextPerson = GetInitialPerson();
                nextPerson.Randy = _randomizer;

                nextPerson.BooleanSamples = Model.Statistics.BinaryGenerator.NextRow(Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson));

                IdentificatorModel.SetProperties(nextPerson);
                NameModel.SetProperties(nextPerson);

                RegstatusModel.SetProperties(nextPerson);
                AdressModel.SetProperties(nextPerson);
                RelationshipModel.SetProperties(nextPerson);

                nextPerson.Person.DateCustody = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).DateCustody.GetDateTimeFromModel(nextPerson, nextPerson.Person.Custody);

                //disse er offisielt sett null
                //nextPerson.Person.WithoutLegalCapacity = MultivariateBinaryGenerator.Sample(PersonStatisticKeys.HasWithoutLegalCapacity, nextPerson.BooleanSamples, Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).WithoutLegalCapacity, _randomizer);
                //nextPerson.Person.DateWithoutLegalCapacity = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).DateWithoutLegalCapacity.GetDateTimeFromModel(nextPerson, nextPerson.Person.WithoutLegalCapacity);

                nextPerson.Person.WorkPermit = MultivariateBinaryGenerator.Sample(PersonStatisticKeys.HasWorkPermit, nextPerson.BooleanSamples, Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).WorkPermit, _randomizer);
                nextPerson.Person.DateWorkPermit = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).DateWorkPermit.GetDateTimeFromModel(nextPerson, nextPerson.Person.WorkPermit);

                nextPerson.Person.ParentalResponsibility = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).ParentalResponsibility.Sample(_randomizer);
                nextPerson.Person.DateParentalResponsibility = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).DateParentalResponsibility.GetDateTimeFromModel(nextPerson, nextPerson.Person.ParentalResponsibility);

                nextPerson.Person.StatusCountryCode = MultivariateBinaryGenerator.Sample(PersonStatisticKeys.HasStatusCountryCode, nextPerson.BooleanSamples, Model.Statistics.StatusCountryCode, _randomizer);

                SetCityzenship(nextPerson);

                _persons.Add(nextPerson.Person.NIN, nextPerson);

                if(!string.IsNullOrEmpty(nextPerson.Person.OldNIN))
                {
                    var clone = Cloner.Clone(nextPerson);
                    clone.Person.NewNIN = nextPerson.Person.NIN;
                    clone.Person.OldNIN = null;
                    clone.Person.NIN = nextPerson.Person.OldNIN;
                    clone.Married = false;
                    clone.NumberOfKids = 0;
                    clone.MotherSearch = null;
                    clone.FatherSearch = null;
                    clone.Person.FathersNIN = null;
                    clone.Person.MothersNIN = null;
                    clone.Person.SpouseNIN = null;

                    _persons.Add(clone.Person.NIN, clone);
                }

            }
            stopw.Stop();
            ConsoleWriteline("Done generating isolated persons. It took " + stopw.Elapsed.TotalMinutes + " minutes");

            stopw.Restart();
            RelationshipModel.BringPeopleTogether(_persons, _randomizer, ConsoleWriteline);

            foreach (var person in _persons)
            {
                SivilstatusModel.SetProperties(person.Value);
            }

            stopw.Stop();
            ConsoleWriteline("Done generating relationsship within persons. It took " + stopw.Elapsed.TotalMinutes + " minutes");

            return _persons.Values.Select(x => x.Person);
        }

        private void SetCityzenship(PersonWithMetadata nextPerson)
        {
            var sample = MultivariateBinaryGenerator.Sample(PersonStatisticKeys.HasCitizenship, nextPerson.BooleanSamples, Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).Citizenship_And_CitizenshipCode, _randomizer);
            if (sample == null || !sample.Contains("_"))
                return;

            var splitted = sample.Split('_');

            nextPerson.Person.Citizenship = splitted[0] == "null" ? null : splitted[0];
            nextPerson.Person.CitizenshipCode = splitted[1] == "null" ? null : splitted[1];
            nextPerson.Person.DateCitizenship = Model.Statistics.GetClosestStatisticByAgeQuant(nextPerson).DateCitizenship.GetDateTimeFromModel(nextPerson, nextPerson.Person.Citizenship);
        }

        public PersonWithMetadata GetInitialPerson(int? predefinedAge = null, bool? predefinedIsFemale = null)
        {
            return GetInitialPerson(IdentificatorModel, predefinedAge, predefinedIsFemale);
        }

        public static PersonWithMetadata GetInitialPerson(IdentificatorModel inIdentificatorModel, int? predefinedAge = null, bool? predefinedIsFemale = null)
        {
            var p = new PersonWithMetadata()
            {
                Person = new Person(),
                PredefinedAge = predefinedAge,
                PrefinedIsFemale = predefinedIsFemale
            };

            p.Age = predefinedAge ?? inIdentificatorModel.GetRandomAge()();
            p.AgeQuants = BaseModel.GetAgeQuant(p.Age);

            return p;
        }

        private RelationshipModel2 GetRelationshipModel2(SynteticModel model, FamilyModel familyModel)
        {
            var pParentNinExistsGivenHaveParentNin = new Func<PersonWithMetadata, double>(person => model.Statistics.RelationshipStatistics.ParentninExists_GivenHaveParentNin.TrueRatioPercent);
            var pParentSameSexGivenHaveValidParentNin = new Func<PersonWithMetadata, double>(person => model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.ParentSameSex_GivenHaveValidParentNin.TrueRatioPercent);
            var hasFatherHasMotherHasSpouse = GetHasFatherHasMotherHasSpouse(familyModel);
            var getAgeDifferenceInYearsToParent = new Func<PersonWithMetadata, int>(person => (int) model.Statistics.RelationshipStatistics.AgeDifferentalInYearsToParent.GenerateSamples(1).First());
            var numberOfChildrenGivenHaveChildren = new Func<PersonWithMetadata, int>(person => model.Statistics.GetClosestStatisticByAgeQuant(person, statistics => statistics.RelationshipStatistics?.NumberOfChildren_GivenHaveChildren.NumberOfSamples > 0 && statistics.RelationshipStatistics.NumberOfChildren_GivenHaveChildren.CanGenerateSamples).RelationshipStatistics.GetNumberOfChildrenSample());
            var getSpouseAgeDifferential = new Func<PersonWithMetadata, int>(person => (int)model.Statistics.GetClosestStatisticByAgeQuant(person, statistics => statistics.RelationshipStatistics?.SpouseAgeDifferentialInYears.TotalCount > 0 && statistics.RelationshipStatistics.SpouseAgeDifferentialInYears.CanGenerateSamples).RelationshipStatistics.SpouseAgeDifferentialInYears.GenerateSamples(1).First() * (person.Randy.Hit(50) ? -1 : 1)); //SpouseAgeDifferentialInYears is defined as always positive .. 
            var pSpouseExistsGivenHasSpouse = new Func<PersonWithMetadata, double>(person => model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.SpouseExists_GivenHasSpouse.TrueRatioPercent);
            var pSpouseSameSexGivenHasValidSpouse = new Func<PersonWithMetadata, double>(person => model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.SpouseSameSex_GivenHasValidSpouse.TrueRatioPercent);
            var pSpouseDuplexGivenHasValidSpouse = new Func<PersonWithMetadata, double>(person => model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.SpouseDuplex_GivenHasValidSpouse.TrueRatioPercent);
            var pParentsAreMarriedGivenhaveMotherAndFather = new Func<PersonWithMetadata, double>(person => model.Statistics.RelationshipStatistics.ParentsAreMarried.TrueRatioPercent);

            return new RelationshipModel2(this, pParentNinExistsGivenHaveParentNin, pParentSameSexGivenHaveValidParentNin, hasFatherHasMotherHasSpouse, getAgeDifferenceInYearsToParent, numberOfChildrenGivenHaveChildren, getSpouseAgeDifferential, pSpouseExistsGivenHasSpouse,
                pSpouseSameSexGivenHasValidSpouse, pSpouseDuplexGivenHasValidSpouse, pParentsAreMarriedGivenhaveMotherAndFather);
         }

        private Func<PersonWithMetadata, Tuple<bool, bool, bool>> GetHasFatherHasMotherHasSpouse(FamilyModel familyModel)
        {
            return person =>
            {
                if (person.BooleanSamples == null ||
                    !person.BooleanSamples.ContainsKey(PersonStatisticKeys.HasFather) ||
                    !person.BooleanSamples.ContainsKey(PersonStatisticKeys.HasMother) ||
                    !person.BooleanSamples.ContainsKey(PersonStatisticKeys.HasSpouse))
                    return familyModel.HasFatherHasMotherHasSpouse(person);

                return new Tuple<bool, bool, bool>(person.BooleanSamples[PersonStatisticKeys.HasFather], person.BooleanSamples[PersonStatisticKeys.HasMother], person.BooleanSamples[PersonStatisticKeys.HasSpouse]);
            };
        }

        public Randomizer GetRandomizer() => _randomizer;
    }
}
