using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class DateStatistics : Statistic
    {
        private readonly int _numberOfSamples;
        private readonly string _name;
        private readonly CorrelationFactory _correlationFactory;
        public Dictionary<string, DiscreteStatistic> DateDifference { get; set; }
        private const string _novalue = "___null___";

        [JsonConstructor, Obsolete]
        public DateStatistics() { }

        public DateStatistics(int numberOfSamples, string name) : base(numberOfSamples, name)
        {
            _numberOfSamples = numberOfSamples;
            DateDifference = new Dictionary<string, DiscreteStatistic>();
        }

        public DateStatistics(int numberOfSamples, string name, CorrelationFactory correlationFactory, Func<Person, int, Tuple<string, DateTime?, DateTime?>> getter) : base(numberOfSamples, name, correlationFactory, calculateValueForStatistic: null)
        {
            _numberOfSamples = numberOfSamples;
            _name = name;
            _correlationFactory = correlationFactory;
            DateDifference = new Dictionary<string, DiscreteStatistic>();

            CalculateValueForStatistic = GetValueAsDouble(getter);
        }

        private Func<Person, int, bool, double?> GetValueAsDouble(Func<Person, int, Tuple<string, DateTime?, DateTime?>> getter)
        {
            return (p, a, updateIt) =>
            {
                var input = getter(p, a);
                var applicableDate = input.Item2;
                var dateOfBirth = input.Item3;
                var applicablevalue = input.Item1 ?? _novalue;

                if(updateIt)
                    Update(applicablevalue, applicableDate, dateOfBirth);

                return applicableDate?.Year ?? ValueToUseWhenNull;
            };
        }

        public void Update(int? value, DateTime? applicableDate, DateTime? dateOfBirth)
        {
            var asString = value.HasValue ? value.Value.ToString() : null;
            Update(asString, applicableDate, dateOfBirth);
        }

        public void Update(string value, DateTime? applicableDate, DateTime? dateOfBirth)
        {
            var applicablevalue = value ?? _novalue;

            if (!DateDifference.ContainsKey(applicablevalue))
                DateDifference[applicablevalue] = new DiscreteStatistic(_numberOfSamples, Name);

            if (!applicableDate.HasValue)
                DateDifference[applicablevalue].Update(ValueToUseWhenNull);

            if (!applicableDate.HasValue || !dateOfBirth.HasValue)
                return;

            var diffPercent = (int)(100.0 * (((double)applicableDate.Value.Year - dateOfBirth.Value.Year) / CommonFunctions.GetAge(dateOfBirth.Value)));
            DateDifference[applicablevalue].Update(diffPercent);
        }

        public DateTime? GetDateTimeFromModel(PersonWithMetadata nextPerson, string value)
        {
            if (!nextPerson.Person.DateOfBirth.HasValue)
                return null;

            var diffPercent = !string.IsNullOrEmpty(value) ? DateDifference[value].Sample(nextPerson.Randy) : (DateDifference.ContainsKey(_novalue) ? DateDifference[_novalue].Sample(nextPerson.Randy) : DateDifference[string.Empty].Sample(nextPerson.Randy));
            if (!diffPercent.HasValue)
                return null;

            var recreatedDatestatus = (int)Math.Round((diffPercent.Value / 100.0) * CommonFunctions.GetAge(nextPerson.Person.DateOfBirth.Value) + nextPerson.Person.DateOfBirth.Value.Year);

            return NinModel.GetBirthday(DateTime.Now.Year - recreatedDatestatus, nextPerson.Randy);
        }

        public DateTime? GetDateTimeFromModel(PersonWithMetadata person, int? value)
        {
            return GetDateTimeFromModel(person, value?.ToString());
        }

        public void SetDistribution()
        {
            foreach (var discreteStatistic in DateDifference.Values)
            {
                discreteStatistic.ComputeProbabilities();
            }
        }

        

        public void DisposeSamples()
        {
            foreach (var discreteStatistic in DateDifference.Values)
            {
                discreteStatistic.DisposeSamples();
            }
        }
    }
}
