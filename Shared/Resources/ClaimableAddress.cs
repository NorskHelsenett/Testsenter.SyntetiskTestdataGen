using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models.PregV1;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class ClaimableAddress : Address
    {
        public bool Claimed { get; set; }
        public AdressElement AdressElement { get; set; }
    }
}
