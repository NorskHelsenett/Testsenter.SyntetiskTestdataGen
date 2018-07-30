using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class StatisticsContainer<T> : ISetStatistics where T : ISetStatistics 
    {
        public int NumberOfSamplesToTake { get; set; }
        public int NumberOfPersons { get; set; }
        public int AgeQuants { get; set; }
        public int? AgeQuantLevel { get; set; }
        public string SessionId { get; set; }
        public Dictionary<int, T> StatisticsByAgeQuants { get; set; }
        public MultivariateBinaryGenerator BinaryGenerator { get; set; }
        public CorrelationMatrix Correlations { get; set; }
        [JsonIgnore]
        public CorrelationFactory CorrelationFactory { get; set; }
        [JsonIgnore]
        public Dictionary<string, Statistic> Statistics { get; set; }
        [JsonIgnore]
        public Dictionary<string, ISetStatistics> CustomModels { get; set; }

        private int _howManyTaken;

        public StatisticsContainer(string sessionId, int numberOfSamplesToTake, int ageQuants, int? ageQuantLevel)
        {
            AgeQuants = ageQuants;
            AgeQuantLevel = ageQuantLevel;
            SessionId = sessionId;
            NumberOfSamplesToTake = numberOfSamplesToTake;
            CorrelationFactory = new CorrelationFactory();
            Statistics = new Dictionary<string, Statistic>();
            CustomModels = new Dictionary<string, ISetStatistics>();
            _howManyTaken = 0;
        }

        public void FromImport(Person p, int ageQuant)
        {
            _howManyTaken++;

            foreach (var stat in Statistics)
                stat.Value.Update(p, ageQuant);

            foreach(var customStat in CustomModels.Where(c => c.Value != null))
                customStat.Value.FromImport(p, ageQuant);

            if (StatisticsByAgeQuants != null && StatisticsByAgeQuants.ContainsKey(ageQuant))
                StatisticsByAgeQuants[ageQuant].FromImport(p, ageQuant);
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
            NumberOfPersons += applicablepersons.Count;

            foreach (var customStat in CustomModels.Where(c => c.Value != null))
                customStat.Value.AfterImport(applicablepersons, allpersons);

            if (StatisticsByAgeQuants != null)
            {
                foreach (var ageQuant in applicablepersons.Values.Where(y => y.Confirmed).GroupBy(x => x.AgeQuants))
                {
                    if (StatisticsByAgeQuants.ContainsKey(ageQuant.Key))
                        StatisticsByAgeQuants[ageQuant.Key].AfterImport(ageQuant.ToDictionary(k => k.Nin, v => v), allpersons);
                }
            }
        }

        public void SetDistribution(bool dispose)
        {
            foreach (var stat in Statistics)
                stat.Value.SetDistribution();

            foreach (var customStat in CustomModels.Where(c => c.Value != null))
                customStat.Value.SetDistribution(dispose);

            if(StatisticsByAgeQuants != null)
                foreach(var stat in StatisticsByAgeQuants)
                    stat.Value.SetDistribution(dispose);
        }
    }
}
