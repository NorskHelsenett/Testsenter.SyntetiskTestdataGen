using System;
using System.Collections.Generic;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class PregNode
    {
        public PregNode()
        {
            ChildNins = new List<string>();
        }

        public string MomNin { get; set; }
        public string DadNin { get; set; }
        public List<string> ChildNins { get; set; }
        public int AgeQuants { get; set; }
        public int Kjonn { get; set; }
        public string MarriedNin { get; set; }
        public bool Confirmed { get; set; }
        public string Nin { get; set; }
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public DateTime? BirthDay { get; set; } 
        public string DebugId { get; set; }
        


    }
}