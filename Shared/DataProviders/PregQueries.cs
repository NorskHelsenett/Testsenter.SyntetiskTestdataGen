namespace SyntetiskTestdataGen.Shared.DataProviders
{
    public class PregQueries
    {

        public static string PersonTable = "dbo.Persons";
        public static string AddressTable = "dbo.Addresses";
        public static string PersonAddressTable = "PersonAddress";

        public static string GetAllPersonsQuery = $"SELECT count(*) FROM {PersonAddressTable}";
    
        public static string GetNextPersonPageQuery(int offset, int offsetInterval)
        { return $@"SELECT * from {PersonAddressTable}
        ORDER BY nin
        OFFSET {offset} ROWS
        FETCH NEXT {offsetInterval} ROWS ONLY";
        }


        public static string CreatePersonAddressQuery = $@"
             SELECT p.Id as nin
              ,FirstName
              ,MiddleName
              ,LastName
              ,MotherRelationshipNumber
              ,FatherRelationshipNumber
              ,SpouseRelationshipNumber
              ,MaritalStatusBackingValue
              ,StatusBackingValue
              ,StatusValidFrom
              ,StatusCountryCode
              ,GenderBackingValue
              ,DateOfBirth
              ,MaidenName
              ,DateWhenNameRegistered
              ,SupersededByCensusNumber
              ,FormerCensusNumber
              ,MaritalStatusValidFrom
              ,WithoutLegalCapacityCode
              ,WithoutLegalCapacityCodeValidFrom
              ,WorkPermitCode
              ,WorkPermitCodeValidFrom
              ,DufNo
              ,Citizenship
              ,CitizenshipValidFrom
              ,CitizenshipCode
              ,Custody
              ,CustodyValidFrom
	          ,a.PersonId AS addressNinKey
              ,CurrentAddress
              ,StreetName
              ,HouseLetter
              ,ZipCode
              ,MunicipalityCode
              ,AddressLine1
              ,AddressLine2
              ,AddressLine3
              ,CoAddress
              ,Country
              ,DistrictCode
              ,DistrictName
              ,StreetNumber
              ,ApartmentNumber
              ,CadastralNumber
              ,PropertyNumber
              ,HouseNumber
              ,ValidFrom
              ,Code
              ,CodeValidFrom
              ,XCoord
              ,YCoord
              ,ComputedStreetAddressLine
              ,AddressLineValidFrom
              ,SchoolDistrict
              ,Constituency
              ,BasicStatisticalUnit
	      INTO dbo.{PersonAddressTable}
          FROM {PersonTable} p 
          left JOIN {AddressTable} a on a.PersonId = p.Id";
    }
}