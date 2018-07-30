using System.Collections.Generic;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class ExportablePregNode
    {
        public string MomId { get; set; }
        public string DadId { get; set; }
        public List<string> ChildIds { get; set; }

        public int AgeQuants { get; set; }
        public bool IsFemale { get; set; }
        public string AdressHash { get; set; }
        public string Id { get; set; }
    }
}