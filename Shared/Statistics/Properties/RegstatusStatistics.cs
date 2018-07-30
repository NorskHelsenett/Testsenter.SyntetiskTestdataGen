using System.Collections.Generic;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class RegstatusStatistics : ISetStatistics
    {
        public DateStatistics DateDifference { get; set; }

        public RegstatusStatistics(int samples)
        {
            DateDifference = new DateStatistics(samples, "RegStatus_DateDifference");
        }

        public void FromImport(Person p, int ageQuant)
        {
            DateDifference.Update(p.RegStatus, p.DateStatus, p.DateOfBirth);
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
            DateDifference.SetDistribution();
        }

        public void DisposeSamples()
        {
            DateDifference.DisposeSamples();
        }

        public void SetDistribution(bool dispose)
        {
            DateDifference.SetDistribution();

            if(dispose)
                DisposeSamples();
        }
    }
}
