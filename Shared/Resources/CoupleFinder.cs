using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Distributions.Univariate;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class CoupleFinder
    {
        //stats
        private readonly Func<PersonWithMetadata, int> _numberOfChildrenGivenHaveChildren;
        private readonly Func<PersonWithMetadata, int> _numberOfOtherChildrenForMarriedCouples;

        //props
        private readonly Dictionary<int, List<PersonWithMetadata>> _nonMarriedPersonsGroupedByAgeQuant;
        private readonly Dictionary<string, List<Parent>>  _parents;
        public readonly Dictionary<string, List<Couple>> CoupleCache;
        public List<bool> MarriedSuccess = new List<bool>();
        public List<bool> NonMarriedSuccess = new List<bool>();
        public int MarriedSuccessRate => (100 * MarriedSuccess.Count(t => t == true)) / MarriedSuccess.Count;
        public int NonMarriedSuccessRate => (100 * NonMarriedSuccess.Count(t => t == true)) / NonMarriedSuccess.Count;

        public CoupleFinder(Dictionary<string, PersonWithMetadata> persons, Func<PersonWithMetadata, int> numberOfChildrenGivenHaveChildren)
        {
            _nonMarriedPersonsGroupedByAgeQuant = persons.Values.Where(t => !t.Married).GroupBy(y => y.AgeQuants).ToDictionary(k => k.Key, v => v.Select(t => t).ToList());
            _parents = new Dictionary<string, List<Parent>>();
            CoupleCache = new Dictionary<string, List<Couple>>();
            _numberOfChildrenGivenHaveChildren = numberOfChildrenGivenHaveChildren;
            _numberOfOtherChildrenForMarriedCouples = numberOfChildrenGivenHaveChildren;
        }

        public bool SetParents(ParentRelationshipSearch next, PersonWithMetadata child, Randomizer randy)
        {
            if (next.Married)
            {
                var success = SetCouple(next, child, GetNewMarriedCouple, randy);
                MarriedSuccess.Add(success);
                return success;
            }
            else
            {
                var success = SetCouple(next, child, GetNewCouple, randy);
                NonMarriedSuccess.Add(success);
                return success;
            }
        }

        public void RegisterMarried(PersonWithMetadata parent1, PersonWithMetadata parent2)
        {
            var father = parent1;
            var mother = parent2;
            if (parent1.IsFemale && !parent2.IsFemale)
            {
                father = parent2;
                mother = parent1;
            }

            var howManyOtherChildrenFather = _numberOfOtherChildrenForMarriedCouples(father);
            var howManyOtherChildrenMother = _numberOfOtherChildrenForMarriedCouples(mother);
            
            var couple = new Couple
            {
                FatherNin = father.Person.NIN,
                MotherNin = mother.Person.NIN,
                FatherSn = father.Person.Sn,
                MotherSn = mother.Person.Sn
            };

            AddToCoupleCache(couple, ParentRelationshipSearch.KeyMarriedParents(father, mother), false);

            if (howManyOtherChildrenFather > 0)
                 AddToParentCacheReturnKey(howManyOtherChildrenFather, father);

            if (howManyOtherChildrenMother > 0)
                AddToParentCacheReturnKey(howManyOtherChildrenMother, mother);

        }

        public void RegisterNumberofChildrenToMarried(DiscreteStringStatistics childrenCount)
        {
            foreach (var parentKey in childrenCount.Stats.Keys)
            {
                var numberOfChildrenToDistribute = childrenCount.Stats[parentKey].Count;
                var couplesInThisGroup = CoupleCache.ContainsKey(parentKey) ? CoupleCache[parentKey] : null;
                if(couplesInThisGroup == null || numberOfChildrenToDistribute == 0)
                    continue;

                var buckets = DistributeToBuckets(couplesInThisGroup.Count, numberOfChildrenToDistribute);

                var index = 0;
                foreach (var couple in couplesInThisGroup)
                {
                    var value = buckets[index++];
                    couple.RemainingNumberOfChildren = value >= 0 ? value : 0;
                }
            }
        }

        private int[] DistributeToBuckets(int numberOfBuckets, int total)
        {
            if (numberOfBuckets == 0)
                return null;

            var result = new int[numberOfBuckets];

            while (total > 0)
            {
                var distributionFunction = new NormalDistribution(mean: (double)total / numberOfBuckets);
                var samples = distributionFunction.Generate(numberOfBuckets);

                for (int i = 0; i < numberOfBuckets; i++)
                {
                    result[i] += (int) samples[i];
                    total -= result[i];
                }
            }

            return result;
        }

        private bool SetCouple(ParentRelationshipSearch next, PersonWithMetadata child, Func<ParentRelationshipSearch, PersonWithMetadata, Randomizer, Couple> getNewCouple, Randomizer randy)
        {
            var couple = GetFromCache(next.KeyParents()) ?? getNewCouple(next, child, randy);

            if (couple == null)
                return false;

            SetNins(couple.FatherNin, couple.MotherNin, child);

            if (randy.Hit(NameModel.PHasSamesirnameAsParents(child)))
            {
                var takeFathers = couple.FatherSn != null && randy.Hit(50);
                child.Person.Sn = takeFathers ? couple.FatherSn : (couple.MotherSn ?? child.Person.Sn);
            }

            

            return true;
        }

        private Couple GetNewMarriedCouple(ParentRelationshipSearch next, PersonWithMetadata child, Randomizer randu)
        {
            return null; // all married couples are already in cache
        }

        private Couple GetNewCouple(ParentRelationshipSearch next, PersonWithMetadata child, Randomizer randy)
        {
            var mother = next.Mother == null ? null : (GetParentNinFromCache(next.Mother) ?? GetNewParent(next.Mother, married: false));
            var father = next.Father == null ? null : (GetParentNinFromCache(next.Father, mother) ?? GetNewParent(next.Father, married: false));

            if (mother == null && father == null)
                return null;

            var howManyChildrenTogether = GetNumberOfChildsTogether(mother?.RemainingNumberOfChildren, father?.RemainingNumberOfChildren, randy);

            int? beforeFather = father?.RemainingNumberOfChildren;
            int? beforeMother = mother?.RemainingNumberOfChildren;

            //usikker på om dette er mulig .. 
            if (mother != null)
                mother.RemainingNumberOfChildren = mother.RemainingNumberOfChildren - howManyChildrenTogether;
            if(father != null)
                father.RemainingNumberOfChildren = father.RemainingNumberOfChildren - howManyChildrenTogether;

            var couple = new Couple
            {
                FatherNin = father?.Nin,
                MotherNin = mother?.Nin,
                FatherSn = father?.Sn,
                MotherSn = mother?.Sn,
                RemainingNumberOfChildren = howManyChildrenTogether - 1
            };

            AddToCoupleCache(couple, next.KeyParents());

            return couple;
        }

        private void AddToCoupleCache(Couple couple, string key, bool onlyIfRemainingNumberOfChildrenAbove0 = true)
        {
            if (couple.RemainingNumberOfChildren > 0 || !onlyIfRemainingNumberOfChildrenAbove0)
            {
                if (!CoupleCache.ContainsKey(key))
                    CoupleCache.Add(key, new List<Couple>());

                CoupleCache[key].Add(couple);
            }
        }

        private int GetNumberOfChildsTogether(int? numberParent1, int? numberParent2, Randomizer randy)
        {
            try
            {
                if (!numberParent1.HasValue && !numberParent2.HasValue)
                    return 0;

                if (numberParent1.HasValue && numberParent1 == 0)
                    return 0;

                if (numberParent2.HasValue && numberParent2 == 0)
                    return 0;

                var maxNumberOfChildsTogether = Math.Min(numberParent1 ?? 1000, numberParent2 ?? 1000);
                return randy.Next(maxNumberOfChildsTogether + 1);
            }
            catch (Exception e)
            {
                throw new Exception($"Feil ved utregning av antall barn sammen gitt numberParent1: {numberParent1} og numberParent2: {numberParent2}");
            }
        }

        private Parent GetNewParent(RelationshipSearch parentSearch, bool married)
        {
            if (!_nonMarriedPersonsGroupedByAgeQuant.ContainsKey(parentSearch.IsLookingForAgeQuant))
                return null;

            var match = _nonMarriedPersonsGroupedByAgeQuant[parentSearch.IsLookingForAgeQuant]
                .FirstOrDefault(pers => !pers.Taken && pers.Married == married && pers.IsFemale == parentSearch.IsLookingForFemale);

            if (match == null)
                return null;

            match.Taken = true;

            var howManyChildren = _numberOfChildrenGivenHaveChildren(match);
            var keyMe = AddToParentCacheReturnKey(howManyChildren, match);

            return _parents[keyMe].First(y => y.RemainingNumberOfChildren > 0);
        }

        private string AddToParentCacheReturnKey(int howManyChildren, PersonWithMetadata match)
        {
            var keyMe = RelationshipSearch.KeyMe(match);

            if (!_parents.ContainsKey(keyMe))
                _parents.Add(keyMe, new List<Parent>());

            _parents[keyMe].Add(new Parent { Nin = match.Person.NIN, Sn = match.Person.Sn, RemainingNumberOfChildren = howManyChildren });

            return keyMe;
        }

        private Parent GetParentNinFromCache(RelationshipSearch parentSearch, Parent not = null)
        {
            var c = _parents.ContainsKey(parentSearch.KeyLookingFor()) && _parents[parentSearch.KeyLookingFor()] != null
                ? _parents[parentSearch.KeyLookingFor()].FirstOrDefault(y => y.RemainingNumberOfChildren > 0 && (not == null || not.Nin != y.Nin))
                : null;

            return c;
        }

        private void SetNins(string fatherNin, string motherNin, PersonWithMetadata child)
        {
            child.Person.FathersNIN = fatherNin;
            child.Person.MothersNIN = motherNin;
        }

        private Couple GetFromCache(string keyParents)
        {
            var c = CoupleCache.ContainsKey(keyParents) && CoupleCache[keyParents] != null
                ? CoupleCache[keyParents].FirstOrDefault(y => y.RemainingNumberOfChildren > 0) : null;

            if (c == null)
                return null;

            c.RemainingNumberOfChildren--;
            if (c.RemainingNumberOfChildren == 0)
                CoupleCache[keyParents].Remove(c);

            return c;
        }
    }

    public class Couple
    {
        public int RemainingNumberOfChildren { get; set; }
        public string FatherNin { get; set; }
        public string MotherNin { get; set; }
        public string FatherSn { get; set; }
        public string MotherSn { get; set; }
    }

    public class Parent
    {
        public int RemainingNumberOfChildren { get; set; }
        public string Nin { get; set; }
        public string Sn { get; set; }
    }
}
