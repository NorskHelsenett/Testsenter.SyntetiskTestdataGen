using System;
using System.Collections.Generic;
using System.Linq;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

// ReSharper disable InconsistentNaming

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class RelationshipStatistics : ISetStatistics
    {
        public BooleanStatistic ParentninExists_GivenHaveParentNin { get; set; }
        public BooleanStatistic ParentSameSex_GivenHaveValidParentNin { get; set; }
        public BooleanStatistic ParentsAreMarried { get; set; }

        public BooleanStatistic SpouseExists_GivenHasSpouse { get; set; }
        public BooleanStatistic SpouseSameSex_GivenHasValidSpouse { get; set; }
        public BooleanStatistic SpouseDuplex_GivenHasValidSpouse { get; set; }

        public Statistic SpouseAgeDifferentialInYears { get; set; }

        public BooleanStatistic HasChildren_GivenHasSpouseNin { get; set; }
        public BooleanStatistic HasChildren_GivenHasNotSpouseNin { get; set; }
        public BooleanStatistic HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren { get; set; }
        public BooleanStatistic HaveMoreThanOneKidTogether_GivenHaveOneKidTogether { get; set; }
        public Statistic NumberOfChildren_GivenHaveChildren { get; set; }
        public Statistic AgeDifferentalInYearsToParent { get; set; }
        public DiscreteStringStatistics Custody_HaveParentNin { get; set; }
        public DiscreteStringStatistics Custody_HaveNotParentNin { get; set; }


        public int MaxNumberOfChildren_GivenHaveChildren { get; set; }

        public RelationshipStatistics(int numberOfSamplesToTake)
        {
            ParentninExists_GivenHaveParentNin = new BooleanStatistic(numberOfSamplesToTake, "ParentninExists_GivenHaveParentNin");
            SpouseExists_GivenHasSpouse = new BooleanStatistic(numberOfSamplesToTake, "SpouseExists_GivenHasSpouse");
            ParentsAreMarried = new BooleanStatistic(numberOfSamplesToTake, "ParentsAreMarried");
            SpouseSameSex_GivenHasValidSpouse = new BooleanStatistic(numberOfSamplesToTake, "SpouseSameSex_GivenHasValidSpouse");
            SpouseDuplex_GivenHasValidSpouse = new BooleanStatistic(numberOfSamplesToTake, "SpouseDuplex_GivenHasValidSpouse");
            ParentSameSex_GivenHaveValidParentNin = new BooleanStatistic(numberOfSamplesToTake, "ParentSameSex_GivenHaveValidParentNin");
            HasChildren_GivenHasSpouseNin = new BooleanStatistic(numberOfSamplesToTake, "HasChildren_GivenHasSpouseNin");
            HasChildren_GivenHasNotSpouseNin = new BooleanStatistic(numberOfSamplesToTake, "HasChildren_GivenHasNotSpouseNin");
            HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren = new BooleanStatistic(numberOfSamplesToTake, "HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren");
            HaveMoreThanOneKidTogether_GivenHaveOneKidTogether = new BooleanStatistic(numberOfSamplesToTake, "HaveMoreThanOneKidTogether_GivenHaveOneKidTogether");
            NumberOfChildren_GivenHaveChildren = new Statistic(numberOfSamplesToTake, "NumberOfChildren_GivenHaveChildren");
            AgeDifferentalInYearsToParent = new Statistic(numberOfSamplesToTake, "AgeDifferentalInYearsToParent");
            Custody_HaveParentNin = new DiscreteStringStatistics(numberOfSamplesToTake, "Custody_HaveParentNin", null);
            Custody_HaveNotParentNin = new DiscreteStringStatistics(numberOfSamplesToTake, "Custody_HaveNotParentNin", null);
            SpouseAgeDifferentialInYears = new Statistic(numberOfSamplesToTake, "SpouseAgeDifferentialInYears");
        }

        public void FromImport(Person p, int ageQuant)
        {
            if(!string.IsNullOrEmpty(p.FathersNIN) || !string.IsNullOrEmpty(p.MothersNIN))
                Custody_HaveParentNin.Update(p.Custody);
            else
                Custody_HaveNotParentNin.Update(p.Custody);
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
            if (applicablepersons == null || allpersons == null) return;
            foreach (var person in applicablepersons)
            {
                if (!person.Value.Confirmed)
                    continue;

                SetParentStatistics(person.Value, allpersons);
                var spouse = SetSpouseStatistics(person.Value, allpersons);
                SetChildrenStatistics(person.Value, allpersons, spouse);
            }
        }

        public void SetDistribution(bool dispose)
        {
            ParentninExists_GivenHaveParentNin.SetDistribution();
            SpouseExists_GivenHasSpouse.SetDistribution();
            SpouseSameSex_GivenHasValidSpouse.SetDistribution();
            SpouseDuplex_GivenHasValidSpouse.SetDistribution();
            ParentSameSex_GivenHaveValidParentNin.SetDistribution();
            HasChildren_GivenHasSpouseNin.SetDistribution();
            HasChildren_GivenHasNotSpouseNin.SetDistribution();
            HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren.SetDistribution();
            HaveMoreThanOneKidTogether_GivenHaveOneKidTogether.SetDistribution();
            NumberOfChildren_GivenHaveChildren.SetDistribution();
            AgeDifferentalInYearsToParent.SetDistribution();
            Custody_HaveParentNin.SetDistribution();
            Custody_HaveNotParentNin.SetDistribution();
            ParentsAreMarried.SetDistribution();
            SpouseAgeDifferentialInYears.SetDistribution();

            if(dispose)
                DisposeSamples();
        }

        public void DisposeSamples()
        {
            ParentninExists_GivenHaveParentNin.DisposeSamples();
            SpouseExists_GivenHasSpouse.DisposeSamples();
            SpouseSameSex_GivenHasValidSpouse.DisposeSamples();
            SpouseDuplex_GivenHasValidSpouse.DisposeSamples();
            ParentSameSex_GivenHaveValidParentNin.DisposeSamples();
            HasChildren_GivenHasSpouseNin.DisposeSamples();
            HasChildren_GivenHasNotSpouseNin.DisposeSamples();
            HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren.DisposeSamples();
            HaveMoreThanOneKidTogether_GivenHaveOneKidTogether.DisposeSamples();
            NumberOfChildren_GivenHaveChildren.DisposeSamples();
            AgeDifferentalInYearsToParent.DisposeSamples();
            Custody_HaveParentNin.DisposeSamples();
            Custody_HaveNotParentNin.DisposeSamples();
            ParentsAreMarried.DisposeSamples();
            SpouseAgeDifferentialInYears.DisposeSamples();
        }

        public int GetNumberOfChildrenSample()
        {
            int tries = 20;
            do
            {
                var s = (int) NumberOfChildren_GivenHaveChildren.GenerateSamples(1).First();
                s = Math.Abs(s);

                if (s <= MaxNumberOfChildren_GivenHaveChildren)
                    return s == 0 ? 1 : s;
            } while (tries-- > 0);

            throw new ArgumentException("Could not generate numberofchildren after 20 tries. Something buggy in the code");
        }

        private void SetChildrenStatistics(PregNode person, Dictionary<string, PregNode> allpersons, PregNode spouse)
        {
            var kids = person.ChildNins.Where(nin => allpersons.ContainsKey(nin) && allpersons[nin].Confirmed).Select(a => allpersons[a]).ToList();
            if(kids.Count >0)
                NumberOfChildren_GivenHaveChildren.Update(kids.Count);

            if (kids.Count > MaxNumberOfChildren_GivenHaveChildren)
                MaxNumberOfChildren_GivenHaveChildren = kids.Count;

            if (string.IsNullOrEmpty(person.MarriedNin))
                HasChildren_GivenHasNotSpouseNin.Update(kids.Any() ? 1: 0);
            else
                HasChildren_GivenHasSpouseNin.Update(kids.Any() ? 1 : 0);

            if (!kids.Any())
                return;

            if (spouse != null)
            {
                HasChildrenWithBetterHalf_GivenValidSpouseNinAndHaveChildren.Update(kids.Any(kid => kid.DadNin == spouse.Nin || kid.MomNin == spouse.Nin) ? 1 : 0);
            }

            var kidsTogetherWithOther = kids.Where(kid => kid.DadNin != null && kid.MomNin != null).ToList();
            if (kidsTogetherWithOther.Any())
            {
                var allParentNins = new List<string>();
                foreach (var kid in kidsTogetherWithOther)
                {
                    if(kid.DadNin != person.Nin)
                        allParentNins.Add(kid.DadNin);
                    if (kid.MomNin != person.Nin)
                        allParentNins.Add(kid.MomNin);
                }

                var duplicateKeys = allParentNins.GroupBy(x => x)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

                HaveMoreThanOneKidTogether_GivenHaveOneKidTogether.Update(duplicateKeys.Any() ? 1 : 0);
            }

            if (!person.BirthDay.HasValue)
                return;

            foreach (var kid in kids.Where(k => k.BirthDay.HasValue))
            {
                var value = kid.BirthDay.Value.Subtract(person.BirthDay.Value).TotalDays / 365;
                AgeDifferentalInYearsToParent.Update(value);
            }
        }

        private void SetParentStatistics(PregNode person, Dictionary<string, PregNode> allpersons)
        {
            var hasDad = !string.IsNullOrEmpty(person.DadNin);
            var hasMom = !string.IsNullOrEmpty(person.MomNin);
            var dad = (hasDad && allpersons.ContainsKey(person.DadNin) && allpersons[person.DadNin].Confirmed) ? allpersons[person.DadNin] : null;
            var mom = (hasMom && allpersons.ContainsKey(person.MomNin) && allpersons[person.MomNin].Confirmed) ? allpersons[person.MomNin] : null;

            if(hasDad)
                ParentninExists_GivenHaveParentNin.Update(dad != null ? 1 : 0);
            if (hasMom)
                ParentninExists_GivenHaveParentNin.Update(mom != null ? 1 : 0);

            if (dad != null && mom != null)
            {
                ParentSameSex_GivenHaveValidParentNin.Update(dad.Kjonn == mom.Kjonn ? 1:0);
            }

            if(hasDad && hasMom)
                ParentsAreMarried.Update(dad != null && mom!=null && (dad.MarriedNin == mom.Nin || mom.MarriedNin == dad.Nin) ? 1 : 0);
        }

        private PregNode SetSpouseStatistics(PregNode person, Dictionary<string, PregNode> allpersons)
        {
            if (string.IsNullOrEmpty(person.MarriedNin))
                return null;
            
            var spouse = allpersons.ContainsKey(person.MarriedNin) && allpersons[person.MarriedNin].Confirmed ? allpersons[person.MarriedNin] : null;
            SpouseExists_GivenHasSpouse.Update(spouse != null ? 1 : 0);

            if (spouse == null)
                return null;

            SpouseSameSex_GivenHasValidSpouse.Update(spouse.Kjonn == person.Kjonn ? 1 : 0);
            SpouseDuplex_GivenHasValidSpouse.Update(spouse.MarriedNin == person.Nin ? 1 : 0);

            if (person.BirthDay.HasValue && spouse.BirthDay.HasValue)
                SpouseAgeDifferentialInYears.Update(Math.Abs(person.BirthDay.Value.Subtract(spouse.BirthDay.Value).TotalDays / 365));

            return spouse;
        }
    }
}
