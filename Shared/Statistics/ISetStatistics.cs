using System.Collections.Generic;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public interface ISetStatistics
    {
        void FromImport(Person p, int ageQuant);
        void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons);
        void SetDistribution(bool dispose);
    }
}
