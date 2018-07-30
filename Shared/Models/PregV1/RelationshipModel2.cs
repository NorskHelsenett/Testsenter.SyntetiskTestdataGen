using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

// ReSharper disable InconsistentNaming

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class RelationshipModel2 : BaseModel, IPregModel
    {
        #region Constructor etc
        private readonly Func<PersonWithMetadata, double> _pParentNinExists_GivenHaveParentNin;
        private readonly Func<PersonWithMetadata, double> _pParentSameSex_GivenHaveValidParentNin;
        private readonly Func<PersonWithMetadata, Tuple<bool, bool, bool>> _hasFatherHasMotherHasSpouse;
        private readonly Func<PersonWithMetadata, int> _getAgeDifferenceInYearsToParent;
        private readonly Func<PersonWithMetadata, int> _numberOfChildrenGivenHaveChildren;

        private readonly Func<PersonWithMetadata, int> _getSpouseAgeDifferential;
        private readonly Func<PersonWithMetadata, double> _pSpouseExists_GivenHasSpouse;
        private readonly Func<PersonWithMetadata, double> _pSpouseSameSex_GivenHasValidSpouse;
        private readonly Func<PersonWithMetadata, double> _pSpouseDuplex_GivenHasValidSpouse;
        private readonly Func<PersonWithMetadata, double> _pParentsAreMarried_GivenhaveMotherAndFather;

        private DiscreteStringStatistics _numberOfChildren;

        public Dictionary<string, List<RelationshipSearch>> SpouseSearchNonDuplex { get; set; }
        public Dictionary<string, List<RelationshipSearch>> SpouseSearchDuplex { get; set; }
        public Stack<ParentRelationshipSearch> ParentSearch { get; set; }

        public RelationshipModel2(
            SynteticDataBuilderV1 databuilder,
            Func<PersonWithMetadata, double> pParentNinExists_GivenHaveParentNin,
            Func<PersonWithMetadata, double> pParentSameSex_GivenHaveValidParentNin,
            Func<PersonWithMetadata, Tuple<bool, bool, bool>> hasFatherHasMotherHasSpouse,
            Func<PersonWithMetadata, int> getAgeDifferenceInYearsToParent,
            Func<PersonWithMetadata, int> numberOfChildrenGivenHaveChildren,
            Func<PersonWithMetadata, int> getSpouseAgeDifferential,
            Func<PersonWithMetadata, double> pSpouseExists_GivenHasSpouse,
            Func<PersonWithMetadata, double> pSpouseSameSex_GivenHasValidSpouse,
            Func<PersonWithMetadata, double> pSpouseDuplex_GivenHasValidSpouse,
            Func<PersonWithMetadata, double> pParentsAreMarried_GivenhaveMotherAndFather
            ) : base(databuilder)
        {
            _pParentNinExists_GivenHaveParentNin = pParentNinExists_GivenHaveParentNin;
            _pParentSameSex_GivenHaveValidParentNin = pParentSameSex_GivenHaveValidParentNin;
            _hasFatherHasMotherHasSpouse = hasFatherHasMotherHasSpouse;
            _getAgeDifferenceInYearsToParent = getAgeDifferenceInYearsToParent;
            _numberOfChildrenGivenHaveChildren = numberOfChildrenGivenHaveChildren;
            _getSpouseAgeDifferential = getSpouseAgeDifferential;
            _pSpouseExists_GivenHasSpouse = pSpouseExists_GivenHasSpouse;
            _pSpouseSameSex_GivenHasValidSpouse = pSpouseSameSex_GivenHasValidSpouse;
            _pSpouseDuplex_GivenHasValidSpouse = pSpouseDuplex_GivenHasValidSpouse;
            _pParentsAreMarried_GivenhaveMotherAndFather = pParentsAreMarried_GivenhaveMotherAndFather;

            Reset();
        }

        public void Reset()
        {
            _numberOfChildren = new DiscreteStringStatistics(100000, "NumberOfChildren", null);
            SpouseSearchNonDuplex = new Dictionary<string, List<RelationshipSearch>>();
            SpouseSearchDuplex = new Dictionary<string, List<RelationshipSearch>>();
            ParentSearch = new Stack<ParentRelationshipSearch>();
        }
        #endregion
        public void SetProperties(PersonWithMetadata person)
        {
            var values = _hasFatherHasMotherHasSpouse(person);

            RegisterParentSearch(person, values.Item1, values.Item2, person.Randy);
            RegisterSpouseSearch(person, values.Item3, person.Randy);

            SetCustody(person, values.Item1 || values.Item2);
        }

        private void SetCustody(PersonWithMetadata person, bool hasParentNin)
        {
            bool? hasCustody = person.BooleanSamples.ContainsKey(PersonStatisticKeys.HasCustody) ? person.BooleanSamples[PersonStatisticKeys.HasCustody] : (bool?)null;
            if (hasCustody.HasValue && !hasCustody.Value)
                return;

            if (hasCustody.HasValue)
            {
                    person.Person.Custody = hasParentNin? 
                    _model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.Custody_HaveParentNin.NonNullSample(person.Randy) :
                    _model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.Custody_HaveNotParentNin.NonNullSample(person.Randy);
            }
            else
            {
                person.Person.Custody = hasParentNin ?
                    _model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.Custody_HaveParentNin.Sample(person.Randy) :
                    _model.Statistics.GetClosestStatisticByAgeQuant(person).RelationshipStatistics.Custody_HaveNotParentNin.Sample(person.Randy);
            }
            
        }

        public void BringPeopleTogether(Dictionary<string, PersonWithMetadata> persons, Randomizer randy, Action<string> outputter)
        {
            var coupleFinder = GetCoupleFinder(persons);

            var stopw = new Stopwatch();

            stopw.Start();
            var nonDuplexStats = BringSpouseNonDuplexTogether(persons, randy, outputter);
            stopw.Stop();
            outputter("BringSpouseNonDuplexTogether took " + stopw.Elapsed.TotalMinutes + " minutes");

            stopw.Restart();
            var duplexStats = BringSpouseDuplexTogether(persons, coupleFinder, randy, outputter);
            stopw.Stop();
            outputter("BringSpouseDuplexTogether took " + stopw.Elapsed.TotalMinutes + " minutes");

            stopw.Restart();
            BringChildrenAndParentsTogether(persons, coupleFinder, randy, outputter);
            stopw.Stop();
            outputter("BringChildrenAndParentsTogether took " + stopw.Elapsed.TotalMinutes + " minutes");
        }

        public CoupleFinder GetCoupleFinder(Dictionary<string, PersonWithMetadata> persons)
        {
            return new CoupleFinder(persons, _numberOfChildrenGivenHaveChildren);
        }
        public void BringChildrenAndParentsTogether(Dictionary<string, PersonWithMetadata> persons, CoupleFinder coupleFinder, Randomizer randy, Action<string> outputter)
        {
            var orgCount = ParentSearch.Count;
            while (ParentSearch.Count > 0)
            {
                var next = ParentSearch.Pop();

                if (!persons.ContainsKey(next.ChildNin))
                    continue;

                var child = persons[next.ChildNin];
                coupleFinder.SetParents(next, child, randy);

                if (randy.Hit(AdressModel.PSameAdressAsParents(child)))
                    SetChildAdressToParents(persons, child, randy);

                if(child.Person.FathersNIN != null && child.Person.MothersNIN != null && randy.Hit(AdressModel.PParentsSameAdress))
                    SetParentsSameAdress(persons, child, randy);
            }

            var marriedSuccessRate = coupleFinder.MarriedSuccessRate;
            var nonmarriedSuccessRate = coupleFinder.NonMarriedSuccessRate;
            var parentsAreMarried = (coupleFinder.MarriedSuccess.Count * 100) / orgCount;

            outputter("BringChildrenAndParentsTogether.marriedSuccessRate=" + marriedSuccessRate);
            outputter("BringChildrenAndParentsTogether.nonmarriedSuccessRate=" + nonmarriedSuccessRate);
        }

        private void SetParentsSameAdress(Dictionary<string, PersonWithMetadata> persons, PersonWithMetadata child, Randomizer randy)
        {
            var ninMaster = randy.Hit(50) ? child.Person.FathersNIN : child.Person.MothersNIN;
            var ninSlave = ninMaster == child.Person.MothersNIN ? child.Person.FathersNIN : child.Person.MothersNIN;

            if (!persons.ContainsKey(ninMaster) || !persons.ContainsKey(ninSlave) || persons[ninMaster].Person?.Addresses?.Any(x => x.CurrentAddress) == null)
                return;

            var adress = AdressModel.Clone(persons[ninMaster].Person.Addresses.First(x => x.CurrentAddress));
            adress.NIN = ninSlave;
            var indexOfCurrentAdress = AdressModel.IndexOfCurrentAdress(persons[ninSlave].Person.Addresses);
            if (indexOfCurrentAdress == null)
                persons[ninSlave].Person.Addresses = new[] { adress };
            else
            {
                persons[ninSlave].Person.Addresses[indexOfCurrentAdress.Value] = adress;
            }
        }

        private void SetChildAdressToParents(Dictionary<string, PersonWithMetadata> persons, PersonWithMetadata child, Randomizer randy)
        {
            var takeFathers = child.Person.FathersNIN != null && randy.Hit(50);
            var ninToTake = takeFathers ? child.Person.FathersNIN : child.Person.MothersNIN;

            if (ninToTake != null && persons.ContainsKey(ninToTake) && persons[ninToTake].Person?.Addresses?.Any(x => x.CurrentAddress) != null)
            {
                var adress = AdressModel.Clone(persons[ninToTake].Person.Addresses.First(x => x.CurrentAddress));
                adress.NIN = child.Person.NIN;

                var indexOfCurrentAdress = AdressModel.IndexOfCurrentAdress(child.Person.Addresses);
                if (indexOfCurrentAdress == null)
                    child.Person.Addresses = new[] { adress };
                else
                {
                    child.Person.Addresses[indexOfCurrentAdress.Value] = adress;
                }
            }
        }

        public Tuple<int, int> BringSpouseNonDuplexTogether(Dictionary<string, PersonWithMetadata> persons, Randomizer randy, Action<string> outputter)
        {
            var howmanyTotal = SpouseSearchNonDuplex.Sum(x => x.Value.Count);
            var successrate = BringSpousesTogether(persons, SpouseSearchNonDuplex, false, null, randy);
            var howmanySet = (howmanyTotal * successrate) / 100;

            outputter("BringSpouseNonDuplexTogether successrate=" + successrate);

            return new Tuple<int, int>(howmanySet, howmanyTotal);
        }

        public Tuple<int, int> BringSpouseDuplexTogether(Dictionary<string, PersonWithMetadata> persons, CoupleFinder coupleFinder, Randomizer randy, Action<string> outputter)
        {
            var howmanyTotal = SpouseSearchDuplex.Sum(x => x.Value.Count);
            var successrate = BringSpousesTogether(persons, SpouseSearchDuplex, true, coupleFinder, randy);
            var howmanySet = (howmanyTotal * successrate) / 100;
            coupleFinder.RegisterNumberofChildrenToMarried(_numberOfChildren);
            outputter("BringSpouseDuplexTogether successrate=" + successrate);

            return new Tuple<int, int>(howmanySet, howmanyTotal);
        }

        private int BringSpousesTogether(Dictionary<string, PersonWithMetadata> persons, Dictionary<string, List<RelationshipSearch>> searchDic, bool duplex, CoupleFinder coupleFinder, Randomizer randy)
        {
            var success = new List<bool>();
            while (true)
            {
                if (!searchDic.Any())
                    break;

                var thisSearchGroup = searchDic.First();
                if (thisSearchGroup.Value == null || !thisSearchGroup.Value.Any())
                {
                    searchDic.Remove(thisSearchGroup.Key);
                    continue;
                }

                var thisSearch = thisSearchGroup.Value.FirstOrDefault(t => !t.Taken);

                if (thisSearch == null)
                {
                    searchDic.Remove(thisSearchGroup.Key);
                    continue;
                }

                thisSearch.Taken = true;
                var thisSearchNin = thisSearch.NinRef;
                if (persons.All(p => p.Key != thisSearchNin))
                    continue;

                int increment = 0;
                RelationshipSearch spouseSearch = null;
                while(true) //(thisSearch.IsLookingForAgeQuant - increment > 0) && (thisSearch.IsLookingForAgeQuant + increment < 30)
                {
                    bool conductedSearch = false;

                    if (thisSearch.IsLookingForAgeQuant + increment < 30)
                    {
                        var lookingForKey = thisSearch.KeyLookingFor(increment);

                        spouseSearch = searchDic.ContainsKey(lookingForKey) ?
                                searchDic[lookingForKey].FirstOrDefault(y => y.Taken == false && persons.ContainsKey(y.NinRef) && y.NinRef != thisSearchNin && (!duplex || y.KeyLookingFor() == thisSearch.KeyMe())) :
                                null;

                        if (spouseSearch != null)
                            break;

                        conductedSearch = true;
                    }

                    if (thisSearch.IsLookingForAgeQuant - increment > 0)
                    {
                        var lookingForKey = thisSearch.KeyLookingFor((-1) * increment);

                        spouseSearch = searchDic.ContainsKey(lookingForKey) ?
                            searchDic[lookingForKey].FirstOrDefault(y => y.Taken == false && persons.ContainsKey(y.NinRef) && y.NinRef != thisSearchNin && (!duplex || y.KeyLookingFor() == thisSearch.KeyMe())) :
                            null;

                        if (spouseSearch != null)
                            break;

                        conductedSearch = true;
                    }

                    increment++;

                    if (!conductedSearch)
                        break;
                }

                if (spouseSearch == null)
                {
                    success.Add(false);
                    continue;
                }

                success.Add(true);

                persons[thisSearchNin].Person.SpouseNIN = persons[spouseSearch.NinRef].Person.NIN;

                if (duplex)
                {
                    spouseSearch.Taken = true;
                    persons[spouseSearch.NinRef].Person.SpouseNIN = persons[thisSearchNin].Person.NIN;

                    if (persons[thisSearchNin].Person.Addresses != null && persons[thisSearchNin].Person.Addresses.Any(x => x.CurrentAddress) && randy.Hit(AdressModel.PSpousesSameAdress(persons[thisSearchNin])))
                    {
                        var adress = AdressModel.Clone(persons[thisSearchNin].Person.Addresses.First(x => x.CurrentAddress));
                        adress.NIN = spouseSearch.NinRef;
                        var spouseAdressIndexToReplace = AdressModel.IndexOfCurrentAdress(persons[spouseSearch.NinRef].Person.Addresses);

                        if (spouseAdressIndexToReplace == null)
                            persons[spouseSearch.NinRef].Person.Addresses = new[] {adress};
                        else
                        {
                            persons[spouseSearch.NinRef].Person.Addresses[spouseAdressIndexToReplace.Value] = adress;
                        }
                    }

                    coupleFinder?.RegisterMarried(persons[thisSearchNin], persons[spouseSearch.NinRef]);
                }
            }

            return (100 * success.Count(t => t)) / success.Count;
        }

        private void RegisterSpouseSearch(PersonWithMetadata person, bool hasSpouse, Randomizer randy)
        {
            if (!hasSpouse)
                return;

            person.Married = true;
            var spouseSameSex = randy.Hit(_pSpouseSameSex_GivenHasValidSpouse(person));
            var spouseAgeDifferential = Try(_getSpouseAgeDifferential, person);
            var spouseAge = person.GetAge() + spouseAgeDifferential;
            var spouseExists = randy.Hit(_pSpouseExists_GivenHasSpouse(person));

            if (!spouseExists)
            {
                var spouseIsFemale = (person.IsFemale && spouseSameSex) || (!person.IsFemale && !spouseSameSex);

                person.Person.SpouseNIN = NinModel.GetBirthdayAndNin(spouseAge, spouseIsFemale, false, person.Randy).Item2;
                return;
            }

            var spouseDuplex = randy.Hit(_pSpouseDuplex_GivenHasValidSpouse(person));

            var spouseSearch = new RelationshipSearch()
            {
                NinRef = person.Person.NIN,
                IsFemale = person.IsFemale,
                AgeQuant = GetAgeQuant(person.GetAge()),

                IsLookingForFemale = (person.IsFemale && spouseSameSex) || (!person.IsFemale && !spouseSameSex),
                IsLookingForAgeQuant = GetAgeQuant(spouseAge)
            };

            AddSearchToDic(spouseSearch.KeyMe(), spouseSearch, spouseDuplex ? SpouseSearchDuplex : SpouseSearchNonDuplex);
        }

        public static int Try(Func<PersonWithMetadata, int> func, PersonWithMetadata person, int def = 0)
        {
            try
            {
                return func(person);
            }
            catch (Exception)
            {
                return def;
            }
        }

        private void AddSearchToDic<T>(string key, T search, Dictionary<string, List<T>> dic)
        {
            if (!dic.ContainsKey(key))
                dic.Add(key, new List<T>());

            dic[key].Add(search);
        }

        private void RegisterParentSearch(PersonWithMetadata person, bool hasFather, bool hasMother, Randomizer randy)
        {
            bool? hasValidFather = hasFather ? randy.Hit(_pParentNinExists_GivenHaveParentNin(person)) : (bool?)null;
            bool? hasValidMother = hasMother ? randy.Hit(_pParentNinExists_GivenHaveParentNin(person)) : (bool?)null;
            var motherAndFatherIsFemale = GetFatherAndMotherSex(person, hasValidFather, hasValidMother, randy);

            var married = hasValidFather.HasValue && hasValidFather.Value && hasValidMother.HasValue && hasValidMother.Value
                && randy.Hit(_pParentsAreMarried_GivenhaveMotherAndFather(person));

            var personAge = person.GetAge();
            var fatherAge = hasFather ? personAge + _getAgeDifferenceInYearsToParent(person) : 0;
            var motherAge = hasMother ? (hasFather && married ? fatherAge + _getSpouseAgeDifferential(new PersonWithMetadata { AgeQuants = GetAgeQuant(fatherAge), Randy = randy}) : personAge + _getAgeDifferenceInYearsToParent(person)) : 0;

            var fatherParentSearch = SetParent_ReturnLookingForParent(person, hasFather, hasValidFather, motherAndFatherIsFemale.Item1, true, fatherAge);
            var motherParentSearch = SetParent_ReturnLookingForParent(person, hasMother, hasValidMother, motherAndFatherIsFemale.Item2, false, motherAge);

            if (fatherParentSearch != null || motherParentSearch != null)
            {
                var next = new ParentRelationshipSearch
                {
                    ChildNin = person.Person.NIN,
                    Father = fatherParentSearch,
                    Mother = motherParentSearch,
                    Married = married
                };

                ParentSearch.Push(next);
                _numberOfChildren.Update(next.KeyParents());
            }
        }

        private static RelationshipSearch SetParent_ReturnLookingForParent(PersonWithMetadata person, bool hasThisParent, bool? hasValidThisParent, bool isFemale, bool setFathersNin, int age)
        {
            if (!hasThisParent)
                return null;

            if (hasValidThisParent.HasValue && hasValidThisParent.Value)
            {
                return new RelationshipSearch
                {
                    IsFemale = person.IsFemale,
                    AgeQuant = GetAgeQuant(person.GetAge()),
                    NinRef = person.Person.NIN,
                    IsLookingForFemale = isFemale,
                    IsLookingForAgeQuant = GetAgeQuant(age)
                };
            }
            else
            {
                if(setFathersNin)
                    person.Person.FathersNIN = NinModel.GetBirthdayAndNin(age, isFemale, false, person.Randy).Item2;
                else
                    person.Person.MothersNIN = NinModel.GetBirthdayAndNin(age, isFemale, false, person.Randy).Item2;
            }

            return null;
        }

        private Tuple<bool, bool> GetFatherAndMotherSex(PersonWithMetadata person, bool? hasValidFather, bool? hasValidMother, Randomizer randy)
        {
            var sameSex = randy.Hit(_pParentSameSex_GivenHaveValidParentNin(person));
            if (sameSex && hasValidFather.HasValue && hasValidFather.Value && hasValidMother.HasValue && hasValidMother.Value)
            {
                var isFemale = randy.Hit(50);
                return new Tuple<bool, bool>(isFemale, isFemale);
            }

            return new Tuple<bool, bool>(false, true);
        }
    }
}
