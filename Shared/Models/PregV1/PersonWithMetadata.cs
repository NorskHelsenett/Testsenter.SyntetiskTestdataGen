using System;
using System.Collections.Generic;
using System.Linq;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Models.PregV1
{
    public class PersonWithMetadata
    {
        #region v2

        public RelationshipSearch FatherSearch { get; set; }
        public RelationshipSearch MotherSearch { get; set; }


        #endregion

        public Dictionary<string, bool> BooleanSamples { get; set; }
        public bool HasDnummer { get; set; }
        public int Age { get; set; }
        public int AgeQuants { get; set; }
        public bool Married { get; set; }
        public bool Taken { get; set; }
        public Person Person { get; set; }
        public Randomizer Randy { get; set; }
        public int? PredefinedAge { get; set; }
        public bool? PrefinedIsFemale { get; set; }
        public string PredefinedSpouseNin { get; set; }
        public bool IsFemale { get; set; }
        public bool CreatedAsChild { get; set; }
        public bool CreatedAsOtherParent { get; set; }
        public bool CreatedAsSpouse { get; set; }
        public bool AddedToQueue { get; set; }
        public int NumberOfKids { get; set; }
        public void AddToQueueIfNotAlreadyAdded(Queue<PersonWithMetadata> q)
        {
            if (AddedToQueue)
                return;

            q.Enqueue(this);
            AddedToQueue = true;
        }

        public bool HasParents()
        {
            return !string.IsNullOrEmpty(Person.FathersNIN) || !string.IsNullOrEmpty(Person.MothersNIN);
        }

        public static DateTime? GetLastUpdated(Person person)
        {
            if (person.DateStatus.HasValue)
                return person.DateStatus;

            if (person.DateCustody.HasValue)
                return person.DateStatus;

            return null;
        }

        public static DateTime? GetCreatedDate(Person person)
        {
            return person.DateOfBirth;
        }

        public static DateTime? GetCreatedDate(Address address)
        {
            return address.DateAdrFrom;
        }

        public static DateTime? GetUpdatedDate(Address address)
        {
            return address.PostalAddressValidFrom;
        }

        public List<PersonWithMetadata> PreviousParentWithCommonKids { get; set; }

        public void AddParentToCommonKid(PersonWithMetadata person)
        {
            if(PreviousParentWithCommonKids == null)
                PreviousParentWithCommonKids = new List<PersonWithMetadata>();

            if(PreviousParentWithCommonKids.All(p => p.Person.NIN != person.Person.NIN))
                PreviousParentWithCommonKids.Add(person);
        }

        public int GetAge()
        {
            return CommonFunctions.GetAge(Person.DateOfBirth.Value);
        }

        //her kan man fylle ut metadata
    }
}
