using System;
using System.Collections.Generic;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class SivilstatusModel : BaseModel, IPregModel
    {
        private Func<PersonWithMetadata, Tuple<int?, DateTime?>> _getMaritialstatusAndDate;

        public SivilstatusModel(SynteticDataBuilderV1 databuilder) : base(databuilder)
        {
            _getMaritialstatusAndDate = (person) =>
            {
                var statistics = _model.Statistics.GetClosestStatisticByAgeQuant(person).SivilstatusStatistics;

                var status = person.Person.SpouseNIN != null ?
                    statistics.Sivilstatus_GivenHasSpouse.GetValue(person.Randy.NextDouble()) :
                    statistics.Sivilstatus_GivenHasNotSpouse.GetValue(person.Randy.NextDouble());

                var date = statistics.DateDifference.GetDateTimeFromModel(person, status);

                return new Tuple<int?, DateTime?>(status, date);
            };
        }

        public SivilstatusModel(Func<PersonWithMetadata, Tuple<int?, DateTime?>> function)
        {
            _getMaritialstatusAndDate = function;
        }

        public void SetProperties(Dictionary<string, PersonWithMetadata> persons)
        {
            foreach(var person in persons.Values)
                SetProperties(person);
        }

        public void SetProperties(PersonWithMetadata person)
        {
            var result = _getMaritialstatusAndDate(person);

            person.Person.MaritalStatus = result.Item1;
            person.Person.DateMaritalStatus = result.Item2;
        }
    }
}
