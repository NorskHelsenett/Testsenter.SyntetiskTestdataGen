using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class DependentDiscreteStatistic : ISetStatistics
    {
        private readonly int _howManySamplesToTake;
        private readonly string _name;

        [JsonIgnore]
        private readonly Func<Person, int, Tuple<double?, double?>> _getValuesToSet;
        public Dictionary<int, DiscreteStatistic> _stats;

        [JsonConstructor, Obsolete]
        public DependentDiscreteStatistic() { }

        public DependentDiscreteStatistic(int howManySamplesToTake, string name, Func<Person, int, Tuple<double?, double?>> setter) 
        {
            _howManySamplesToTake = howManySamplesToTake;
            _name = name;
            _getValuesToSet = setter;
            _stats = new Dictionary<int, DiscreteStatistic>();
        }

        public void Update(double? val1, double? val2)
        {
            var value1 = GetIntFromValue(val1);
            var value2 = GetIntFromValue(val2);

            if (!_stats.ContainsKey(value1))
                _stats.Add(value1, new DiscreteStatistic(_howManySamplesToTake, _name));

            _stats[value1].Update(value2);
        }

        public void SetDistribution(bool dispose)
        {
            foreach (var item in _stats)
            {
                item.Value.ComputeProbabilities();
                item.Value.SetDistribution();
            }
        }

        private int GetIntFromValue(double? val)
        {
            return val == null ? (int) Statistic.ValueToUseWhenNull : (int) val;
        }

        public int? Sample(int? value1, Randomizer randy)
        {
            var key = value1 ?? (int)Statistic.ValueToUseWhenNull;

            if (!_stats.ContainsKey(key))
                return null;

            return _stats[key].Sample(randy);
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            foreach (var item in _stats)
            {
                s.AppendLine(item.Value.ToString());
            }

            return s.ToString();
        }

        public void FromImport(Person p, int ageQuant)
        {
            var values = _getValuesToSet(p, ageQuant);
            Update(values.Item1, values.Item2);
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
        }
    }
}
