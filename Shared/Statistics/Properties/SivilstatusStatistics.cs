using System.Collections.Generic;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

// ReSharper disable InconsistentNaming

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class SivilstatusStatistics : ISetStatistics
    {
        public DiscreteStatistic Sivilstatus_GivenHasSpouse { get; set; }
        public DiscreteStatistic Sivilstatus_GivenHasNotSpouse { get; set; }
        public DateStatistics DateDifference { get; set; }

        public SivilstatusStatistics(int numberOfSamplesToTake)
        {
            Sivilstatus_GivenHasSpouse = new DiscreteStatistic(numberOfSamplesToTake, "Sivilstatus_GivenHasSpouse");
            Sivilstatus_GivenHasNotSpouse = new DiscreteStatistic(numberOfSamplesToTake, "Sivilstatus_GivenHasNotSpouse");
            DateDifference = new DateStatistics(numberOfSamplesToTake, "Sivilstatus_DateDifference");
        }

        public void FromImport(Person p, int ageQuant)
        {
            if (!string.IsNullOrEmpty(p.SpouseNIN))
            {
                Sivilstatus_GivenHasSpouse.Update(p.MaritalStatus);
            }
            else
            {
                Sivilstatus_GivenHasNotSpouse.Update(p.MaritalStatus);
            }

            DateDifference.Update(p.MaritalStatus, p.DateMaritalStatus, p.DateOfBirth);
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
        }

        public void SetDistribution(bool dispose)
        {
            Sivilstatus_GivenHasNotSpouse.SetDistribution();
            Sivilstatus_GivenHasSpouse.SetDistribution();
            DateDifference.SetDistribution();

            if(dispose)
                DisposeSamples();
        }

        public void DisposeSamples()
        {
            Sivilstatus_GivenHasNotSpouse.DisposeSamples();
            Sivilstatus_GivenHasSpouse.DisposeSamples();
            DateDifference.DisposeSamples();
        }
    }
}
