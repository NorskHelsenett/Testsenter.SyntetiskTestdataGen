using System;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class FamilyModel : BaseModel
    {
        public FamilyModel(SynteticDataBuilderV1 databuilder) : base(databuilder)
        {
            
        }

        public Tuple<bool, bool, bool> HasFatherHasMotherHasSpouse(PersonWithMetadata person)
        {
            var hasFatherStat = _model.Statistics.GetClosestStatisticByAgeQuant(person).HasFather;
            var hasMotherStat = _model.Statistics.GetClosestStatisticByAgeQuant(person).HasMother;
            var hasSpouseStat = _model.Statistics.GetClosestStatisticByAgeQuant(person).HasSpouse;

            var correlation = CommonFunctions.GetCorrelation(person, _model, "hasFather", "HasMother");

            var hasFatherHasMother = CommonFunctions.GetDependentStatistic(hasFatherStat, hasMotherStat, correlation, person.Randy);
            var hasSpouse = person.Randy.HitPropabilityDecimal(hasSpouseStat.TrueRatio);

            return new Tuple<bool, bool, bool>(hasFatherHasMother.Item1, hasFatherHasMother.Item2, hasSpouse);
        }
    }
}
