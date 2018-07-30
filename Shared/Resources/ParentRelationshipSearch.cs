using SyntetiskTestdataGen.Shared.Models.PregV1;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class ParentRelationshipSearch
    {
        public bool Married { get; set; }
        public string ChildNin { get; set; }
        public RelationshipSearch Mother { get; set; }
        public RelationshipSearch Father { get; set; }

        public string KeyParents()
        {
            var str = Married ? "M_" : "S_";

            str += Mother == null ? "null" : Mother.KeyLookingFor();
            str += "_";

            str += Father == null ? "null" : Father.KeyLookingFor();

            return str;
        }

        public static string KeyMarriedParents(PersonWithMetadata father, PersonWithMetadata mother)
        {
            var pc = new ParentRelationshipSearch();
            pc.Mother = new RelationshipSearch
            {
                IsLookingForAgeQuant = mother.AgeQuants,
                IsLookingForFemale = mother.IsFemale
            };
            pc.Father = new RelationshipSearch()
            {
                IsLookingForAgeQuant = father.AgeQuants,
                IsLookingForFemale = father.IsFemale
            };
            pc.Married = true;

            return pc.KeyParents();
        }
    }
}
