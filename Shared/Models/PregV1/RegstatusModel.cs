using SyntetiskTestdataGen.Shared.Statistics;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class RegstatusModel : BaseModel, IPregModel
    {
        public RegstatusModel(SynteticDataBuilderV1 databuilder) : base(databuilder)
        {
        }

        public void SetProperties(PersonWithMetadata person)
        {
            bool? isDead = person.BooleanSamples.ContainsKey(PersonStatisticKeys.IsDead)
                ? MultivariateBinaryGenerator.Hit(PersonStatisticKeys.IsDead, person.BooleanSamples, false)
                : (bool?) null;

            var stat = _model.Statistics.GetClosestStatisticByAgeQuant(person).RegStatus;

            if (!isDead.HasValue)
            {
                person.Person.RegStatus = stat?.Sample(person.Randy);
            }
            else
            {
                if (isDead.Value)
                    person.Person.RegStatus = 5;
                else
                {
                    int count = 1000;
                    while (count-- > 0)
                    {
                        var sample = stat.Sample(person.Randy);
                        if(sample.HasValue && sample.Value == 5)
                            continue;

                        person.Person.RegStatus = sample;
                        break;
                    }
                }
            }

            person.Person.DateStatus = _model.Statistics.GetClosestStatisticByAgeQuant(person).RegstatusStatistics.DateDifference.GetDateTimeFromModel(person, person.Person.RegStatus);
        }
    }
}
