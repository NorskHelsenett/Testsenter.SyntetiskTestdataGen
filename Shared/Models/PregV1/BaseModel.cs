using System;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class BaseModel
    {
        protected SynteticDataBuilderV1 _databuilder;
        protected SynteticModel _model => _databuilder.Model;

        public BaseModel() { }

        public BaseModel(SynteticDataBuilderV1 databuilder)
        {
            _databuilder = databuilder;
        }

        protected static int GetAgeQuant(DateTime? personDateOfBirth, int ageQuant = 5)
        {
            if (!personDateOfBirth.HasValue)
                return 0;
            return (DateTime.Now.Year - personDateOfBirth.Value.Year) / ageQuant;
        }

        public static int GetAgeQuant(int age, int ageQuant = 5)
        {
            return age / ageQuant;
        }
    }
}
