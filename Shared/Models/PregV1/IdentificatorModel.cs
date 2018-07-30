using System;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class IdentificatorModel : BaseModel, IPregModel
    {
        private readonly IdentifierDublicateControl _idControl;


        private readonly Func<PersonWithMetadata, bool> _getKjonnIsFemale;
        private Func<int> _getRandomAge;
        private readonly Func<PersonWithMetadata, double> _pDufNo;
        private readonly Func<PersonWithMetadata, double> _pHasDnummer;
        private readonly Func<PersonWithMetadata, double> _pHasNewNin;
        private readonly Func<PersonWithMetadata, double> _pHasOldNin;
        private DiscreteStatistic _ageStatistic;

        public IdentificatorModel(SynteticDataBuilderV1 databuilder, IdentifierDublicateControl idControl) : base(databuilder)
        {
            _idControl = idControl;
            _getKjonnIsFemale = (person) =>
            {
                if (person.HasDnummer)
                    return _model.Statistics.GetClosestStatisticByAgeQuant(person).HasDnummer_Kjonn.Sample(person.Randy).Value == 2;

                return (int) _model.Statistics.GetClosestStatisticByAgeQuant(person).Kjonn.Sample(person.Randy).Value == 2;
            };


            _pDufNo = (person) => _model.Statistics.GetClosestStatisticByAgeQuant(person).HasDufNo.TrueRatioPercent;
            _pHasDnummer = (person) => _model.Statistics.GetClosestStatisticByAgeQuant(person).HasDnummer.TrueRatioPercent;
            _pHasNewNin = (person) => _model.Statistics.GetClosestStatisticByAgeQuant(person).HasNewNIN.TrueRatioPercent;
            _pHasOldNin = (person) => _model.Statistics.GetClosestStatisticByAgeQuant(person).HasOldNIN.TrueRatioPercent;
        }

        public Func<int> GetRandomAge()
        {
            if(_getRandomAge == null)
                _getRandomAge = GetAgeGenerator();

            return _getRandomAge;
        }

        private Func<int> GetAgeGenerator()
        {
            var randy = base._databuilder.GetRandomizer();
            _ageStatistic = new DiscreteStatistic(1, "Age");
            foreach (var ageQuant in _model.Statistics.StatisticsByAgeQuants)
            {
                _ageStatistic._stats.Add(ageQuant.Key, ageQuant.Value.NumberOfPersons);
            }

            return () => NinModel.GetRandomAge(_ageStatistic.Sample(randy).Value, _model.AgeQuantLevel, randy);
        }

        public IdentificatorModel(
            IdentifierDublicateControl idControl,
            Func<PersonWithMetadata, bool> getKjonnIsFemale,
            Func<int> getRandomAge,
            double pHasDnummer = 20,
            double pHasNewNin = 1,
            double pHasOldNin = 1,
            double pHasDufNo = 0.1)
        {
            _idControl = idControl;
            _getKjonnIsFemale = getKjonnIsFemale;
            _getRandomAge = getRandomAge;
            _pHasDnummer = (person) => pHasDnummer;
            _pHasNewNin = (person) => pHasNewNin;
            _pHasOldNin = (person) => pHasOldNin;
            _pDufNo = (person) => pHasDufNo;
        }

        public void SetProperties(PersonWithMetadata person)
        {
            if (!string.IsNullOrEmpty(person.Person.NIN))
                return;

            person.HasDnummer = MultivariateBinaryGenerator.Hit(PersonStatisticKeys.HasDnummer, person.BooleanSamples, person.Randy.Hit(_pHasDnummer(person)));

            person.IsFemale = _getKjonnIsFemale(person);
            var bdayAndNin = GetNonTakenBirthdayAndNin(person, person.IsFemale, person.HasDnummer);

            person.Person.NIN = bdayAndNin.Item2;
            person.Person.DateOfBirth = bdayAndNin.Item1; 

            SetOldNin(person, person.IsFemale);
            //SetNewNin(person, person.IsFemale);
            SetDufNo(person);
        }

        private Tuple<DateTime, string> GetNonTakenBirthdayAndNin(PersonWithMetadata person, bool isFemale, bool hasDnummer)
        {
            int howManyTries = 0;

            while (true)
            {
                if (howManyTries++ > 100)
                    throw new ArgumentException($"Klarer ikke å lage nin for alder {person.Age}");

                var bdayAndNin = NinModel.GetBirthdayAndNin(person.Age, isFemale, hasDnummer, person.Randy);

                if (_idControl.TakenContains(bdayAndNin.Item2))
                    continue;

                var successfulAdd = _idControl.TakenAdd(bdayAndNin.Item2);
                if (!successfulAdd)
                    continue;

                return bdayAndNin;
            }
        }

        private void SetOldNin(PersonWithMetadata person, bool isFemale)
        {
            var hasOldNin = MultivariateBinaryGenerator.Hit(PersonStatisticKeys.HasOldNIN, person.BooleanSamples, person.Randy.Hit(_pHasOldNin(person)));

            if (!hasOldNin)
                return;

            var bdayAndNin = GetNonTakenBirthdayAndNin(person, isFemale, false);
            person.Person.OldNIN = bdayAndNin.Item2;
        }

        private void SetNewNin(PersonWithMetadata person, bool isFemale)
        {
            var hasNewNin = MultivariateBinaryGenerator.Hit(PersonStatisticKeys.HasNewNIN, person.BooleanSamples, person.Randy.Hit(_pHasNewNin(person)));

            if (!hasNewNin)
                return;

            var bdayAndNin = GetNonTakenBirthdayAndNin(person, isFemale, false);
            person.Person.NewNIN = bdayAndNin.Item2;
        }

        private void SetDufNo(PersonWithMetadata p)
        {
            var hasDufNo = MultivariateBinaryGenerator.Hit(PersonStatisticKeys.HasDufNo, p.BooleanSamples, p.Randy.Hit(_pDufNo(p)));

            if (!hasDufNo)
                return;

            var firstApplicationYear = DateTime.Now.Year - p.Randy.Next(p.Age);
            p.Person.DufNo = firstApplicationYear.ToString();
            for (int i = 0; i < 8; i++)
                p.Person.DufNo += p.Randy.Next(10).ToString();

        }
    }
}
