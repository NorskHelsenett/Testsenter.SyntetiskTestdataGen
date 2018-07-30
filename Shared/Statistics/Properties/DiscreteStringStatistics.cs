using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class DiscreteStringStatisticElement
    {
        public int Count { get; set; }
        public double? Representation { get; set; }
    }

    public class DiscreteStringStatistics : Statistic
    {
        private readonly int? _ageQuantLevel;
        public Dictionary<string, DiscreteStringStatisticElement> Stats { get; set; }

        private const string _emptyString = "___emtpystring___";
        private const int _maximumNumberOfStrings = 10000;
        public List<Tuple<string, double>> Cdf { get; set; }
        public List<Tuple<string, double>> Pdf { get; set; }

        [JsonConstructor, Obsolete]
        public DiscreteStringStatistics() { }

        public DiscreteStringStatistics(int howManySamplesToTake, string name, int? ageQuantLevel) : base(howManySamplesToTake, name)
        {
            _ageQuantLevel = ageQuantLevel;
            Stats = new Dictionary<string, DiscreteStringStatisticElement>
            {
                {_emptyString, new DiscreteStringStatisticElement() {Count = 0}}
            };

            IsDiscrete = true;
        }

        public DiscreteStringStatistics(int howManySamplesToTake, int? ageQuantLevel, string name, CorrelationFactory correlationFactory, Func<Person, int, string> calculateValueForStatistic) : base(howManySamplesToTake, name, correlationFactory, calculateValueForStatistic: null)
        {
            _ageQuantLevel = ageQuantLevel;

            Stats = new Dictionary<string, DiscreteStringStatisticElement>
            {
                {_emptyString, new DiscreteStringStatisticElement() {Count = 0}}
            };

            IsDiscrete = true;
            CalculateValueForStatistic = GetValueAsDouble(calculateValueForStatistic);
        }

        private Func<Person, int, bool, double?> GetValueAsDouble(Func<Person, int, string> calculateValueForStatistic)
        {
            return (p, a, updateIt) =>
            {
                var val = calculateValueForStatistic(p, a);
                var doubleRepres = val.GetValue();

                if (string.IsNullOrEmpty(val))
                {
                    if(updateIt)
                        Stats[_emptyString].Count++;

                    return ValueToUseWhenNull;
                }
                else
                {
                    if (!Stats.ContainsKey(val) && Stats.Keys.Count < _maximumNumberOfStrings && updateIt)
                        Stats.Add(val, new DiscreteStringStatisticElement() {Count = 0, Representation = doubleRepres});

                    if (!Stats.ContainsKey(val))
                        return doubleRepres;

                    if(updateIt)
                        Stats[val].Count++;
                }

                return doubleRepres;
            };
        }

        public void Update(string val)
        {
            var doubleRepres = val.GetValue();

            if (string.IsNullOrEmpty(val))
            {
                Stats[_emptyString].Count++;
            }
            else
            {
                if (!Stats.ContainsKey(val) && Stats.Keys.Count < _maximumNumberOfStrings)
                    Stats.Add(val, new DiscreteStringStatisticElement() { Count = 0, Representation = doubleRepres });

                if (!Stats.ContainsKey(val))
                    return;

                Stats[val].Count++;
            }

            base.Update(doubleRepres);
        }

        public override void SetDistribution()
        {
            ComputeProbabilities();
            base.SetDistribution();
        }

        public string GetValue(double index)
        {
            if (Cdf == null)
                ComputeProbabilities();

            return Cdf.FirstOrDefault(x => x.Item2 >= index).Item1;
        }

        public string Sample(Randomizer randy)
        {
            var p = randy.NextDouble();
            var value = GetValue(p);
            return value == _emptyString ? null : value;
        }

        public string NonNullSample(Randomizer randy)
        {
            int count = 1000;
            while (count-- > 0)
            {
                var val = Sample(randy);
                if (string.IsNullOrEmpty(val))
                    continue;

                return val;
            }
            return null;
        }

        public void ComputeProbabilities()
        {
            if (Cdf != null)
                return;

            Cdf = new List<Tuple<string, double>>();
            Pdf = new List<Tuple<string, double>>();

            var i = 0;
            var tot = (double) Stats.Values.Select(y => y.Count).Sum();

            foreach (var stat in Stats)
            {
                i += stat.Value.Count;

                Cdf.Add(new Tuple<string, double>(stat.Key, i / tot));
                Pdf.Add(new Tuple<string, double>(stat.Key, (stat.Value.Count / tot)));
            }
        }

        public override string ToString()
        {
            var s = "";
            foreach (var element in Pdf)
            {
                s += $"{element.Item1}:{element.Item2}";
            }

            return s;
        }
    }
}