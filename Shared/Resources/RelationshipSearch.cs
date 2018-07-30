using SyntetiskTestdataGen.Shared.Models.PregV1;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class RelationshipSearch
    {
        public bool IsFemale { get; set; }
        public int AgeQuant { get; set; }
        public string NinRef { get; set; }
        public bool Taken { get; set; }
        public int Count { get; set; }
        public bool IsLookingForFemale { get; set; }
        public int IsLookingForAgeQuant { get; set; }

        public string KeyLookingFor(int addThisToAgeQuant = 0)
        {
            return GetKey(IsLookingForFemale, IsLookingForAgeQuant + addThisToAgeQuant);
        }

        public string KeyMe()
        {
            return GetKey(IsFemale, AgeQuant);
        }

        public static string KeyMe(PersonWithMetadata person)
        {
            return GetKey(person.IsFemale, person.AgeQuants);
        }

        private static string GetKey(bool isFemale, int ageQuant)
        {
            return isFemale.ToString() + ageQuant;
        }
    }
}
