using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class DiscreteStatistic : Statistic
    {
        public Dictionary<int, int> _stats { get; set; }
        public List<Tuple<int, double>> cdf { get; set; }
        public List<Tuple<int, double>> pdf { get; set; }

        [JsonConstructor, Obsolete]
        public DiscreteStatistic() { }

        public DiscreteStatistic(int howManySamplesToTake, string name) : base(howManySamplesToTake, name)
        {
            _stats = new Dictionary<int, int>();
            IsDiscrete = true;
        }

        public DiscreteStatistic(int howManySamplesToTake, string name, CorrelationFactory correlationFactory, Func<Person, int, double?> calculateValueForStatistic) : base(howManySamplesToTake, name, correlationFactory, calculateValueForStatistic)
        {
            _stats = new Dictionary<int, int>();
            IsDiscrete = true;
        }

        public override void Update(double? val)
        {
            var value = val == null ? (int) ValueToUseWhenNull : (int)val;

            if (_stats.ContainsKey(value))
                _stats[value] += 1;
            else
                _stats[value] = 1;

            base.Update(val);

        }

        public override void SetDistribution()
        {
            ComputeProbabilities();
            base.SetDistribution();
        }

        public void ComputeProbabilities()
        {
            if (cdf != null)
                return;
            cdf = new List<Tuple<int, double>>();
            pdf = new List<Tuple<int, double>>();

            var i = 0;
            var tot = (double)_stats.Values.Sum();
            foreach (var stat in _stats)
            {
                i += stat.Value;
                cdf.Add(new Tuple<int, double>(stat.Key, i / tot));
                pdf.Add(new Tuple<int, double>(stat.Key, (stat.Value / tot)));
            }
        }

        public int GetValue(double index)
        {
            if(cdf == null)
                ComputeProbabilities();
            return cdf.FirstOrDefault(x => x.Item2 >= index).Item1;
        }

        //todo remove -1 when new model is produced (27/06)
        public int? Sample(Randomizer randy)
        {
            var p = randy.NextDouble();
            var sample = GetValue(p);

            return sample == ValueToUseWhenNull || sample == -1 ? (int?) null : sample;
        }

        public int? NonNullSample(Randomizer randy)
        {
            int count = 1000;
            while (count-- > 0)
            {
                var val = Sample(randy);
                if (!val.HasValue)
                    continue;

                return val;
            }
            return null;
        }

        public double? GetProbabilityForValue(int value)
        {
            return pdf.FirstOrDefault(x => x.Item1.Equals(value))?.Item2;
        }

        public List<Tuple<int, double>> GetPDF()
        {
            if(pdf == null)
                ComputeProbabilities();
            return pdf;
        }

        public List<Tuple<int, double>> GetCDF()
        {
            if (cdf == null)
                ComputeProbabilities();
            return cdf;
        }

        public override string ToString()
        {
            var count = (double)_stats.Values.Sum();

            return string.Join("\n", _stats.Select(x => $"Value: {x.Key}, Probability: {(x.Value / count) * 100}"));
        }


    }
}
