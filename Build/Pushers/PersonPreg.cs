using System.Data.SqlTypes;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public class PersonPreg 
    {
       private readonly Person _person;

        public PersonPreg(Person person)
        {
            _person = person;
            DateWhenNameRegistered = NameModel.GetDateWhenNameRegistered(person);
            MaidenName = person.WithoutLegalCapacity; //"låner" WithoutLegalCapacity, siden det ikke brukes 
            var isFemale = CommonFunctions.IsFemale(person.NIN);
            GenderBackingValue = isFemale.HasValue ? (isFemale.Value ? 0 : 1) : 0; 
            UpdatedDate = PersonWithMetadata.GetLastUpdated(person);
            CreatedDate = PersonWithMetadata.GetCreatedDate(person);
        }

        public string Id  => _person.NIN;

        public string FirstName => _person.GivenName;

        public string MiddleName => _person.MiddleName;

        public string LastName => _person.Sn;

        public string MotherRelationshipNumber => _person.MothersNIN;

        public string FatherRelationshipNumber => _person.FathersNIN;

        public string SpouseRelationshipNumber => _person.SpouseNIN;
        public int MaritalStatusBackingValue => _person.MaritalStatus.Value;
        public int? StatusBackingValue => _person.RegStatus;
        public SqlDateTime? StatusValidFrom =>   _person.DateStatus;
        public int? StatusCountryCode => _person.StatusCountryCode;
        public int GenderBackingValue { get; set; }
        public SqlDateTime? DateOfBirth => _person.DateOfBirth;

        public string MaidenName { get; set; } 
        public SqlDateTime? DateWhenNameRegistered { get; set; }//=> 

        public string SupersededByCensusNumber => _person.NewNIN;

        public string FormerCensusNumber => _person.OldNIN;
        public SqlDateTime? CreatedDate { get; set; } 
        public SqlDateTime? UpdatedDate { get; set; } 
        public SqlDateTime? MaritalStatusValidFrom => _person.DateMaritalStatus;

        public string WithoutLegalCapacityCode => null; //alltid null. måtte låne denne til MaidenName
        public SqlDateTime? WithoutLegalCapacityCodeValidFrom =>_person.DateWithoutLegalCapacity;

        public string WorkPermitCode =>  _person.WorkPermit;
        public SqlDateTime? WorkPermitCodeValidFrom => _person.DateWorkPermit;

        public string DufNo => _person.DufNo;

        public string Citizenship => _person.Citizenship;
        public SqlDateTime? CitizenshipValidFrom => _person.DateCitizenship;

        public string CitizenshipCode =>  _person.CitizenshipCode;

        public string Custody => _person.Custody;
        public SqlDateTime? CustodyValidFrom => _person.DateCustody;
        
    }
    
}
