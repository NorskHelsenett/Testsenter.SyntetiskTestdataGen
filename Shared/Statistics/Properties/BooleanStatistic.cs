using System;
using System.Collections.Generic;
using Accord.Statistics;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class BooleanStatistic : Statistic
    {
        public int NumberOfTrues { get; set; }
        public int NumberOfFalse { get; set; }

        public double TrueRatio => (double) NumberOfTrues / TotalCount;
        public double FalseRatio => (double)NumberOfFalse / TotalCount;
        public double TrueRatioPercent => (((double) NumberOfTrues)*100.0) / TotalCount;

        [JsonConstructor, Obsolete]
        public BooleanStatistic() { }

        public BooleanStatistic(int howManySamplesToTake, string name) : base(howManySamplesToTake, name)
        {
            IsBoolean = true;
        }

        public BooleanStatistic(int howManySamplesToTake, string name, CorrelationFactory correlationFactory, Func<Person, int, double?> calculateValueForStatistic) : base(howManySamplesToTake, name, correlationFactory, calculateValueForStatistic)
        {
            IsBoolean = true;
        }

        public double GetStandardDeviation()
        {
            if (NumberOfSamples == 0 || NumberOfTrues == NumberOfSamples || NumberOfFalse == NumberOfSamples)
                return 0;

            if (StDeviation != 0)
                return StDeviation;

            var s = new double[NumberOfSamples];
            for (int i = 0; i < NumberOfSamples; i++)
                s[i] = i > NumberOfTrues ? 0 : 1;

            return s.StandardDeviation();
        }

        public override void Update(double? value)
        {
            if (value.HasValue)
            {
                if (value.Value == 1)
                    NumberOfTrues++;

                if (value.Value == 0)
                    NumberOfFalse++;
            }

            base.Update(value);
        }

        public bool Sample(Randomizer randy)
        {
            return randy.Hit(TrueRatioPercent);
        }

        public override string ToString()
        {
            return $"Ratio: {TrueRatio * 100}, TotalCount: {NumberOfSamples}, TrueCount: {NumberOfTrues}";
        }

        public static List<BooleanStatistic> GetAll(PersonStatistics ps)
        {
            var result = new List<BooleanStatistic>();
            var properties = ps.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(BooleanStatistic))
                    result.Add(((BooleanStatistic)property.GetValue(ps, null)));
            }

            return result;
        }
    }
}
