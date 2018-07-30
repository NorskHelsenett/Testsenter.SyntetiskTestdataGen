using System.Data.SqlClient;
using SyntetiskTestdataGen.Shared.CodeResolver;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Shared.Mapper
{
    public static class ModelMapper
    {
        public static Person MapToPerson(this SqlDataReader reader)
        {
            return new Person
            {
                NIN = reader["nin"] as string,
                DateOfBirth = reader.GetNullableDateTime("DateOfBirth"),
                GivenName = reader["FirstName"] as string,
                MiddleName = reader["MiddleName"] as string,
                Sn = reader["LastName"] as string,
                Citizenship = reader["Citizenship"] as string,
                CitizenshipCode = reader["CitizenshipCode"] as string,
                Custody = reader["Custody"] as string,
                DateCitizenship = reader.GetNullableDateTime("CitizenshipValidFrom"),
                DateCustody = reader.GetNullableDateTime("CustodyValidFrom"),
                DateMaritalStatus = reader.GetNullableDateTime("MaritalStatusValidFrom"),
                DateWithoutLegalCapacity = reader.GetNullableDateTime("WithoutLegalCapacityCodeValidFrom"),
                DateWorkPermit = reader.GetNullableDateTime("WorkPermitCodeValidFrom"),
                DufNo = reader["DufNo"] as string,
                FathersNIN = reader["FatherRelationshipNumber"] as string,
                MothersNIN = reader["MotherRelationshipNumber"] as string,
                SpouseNIN = reader["SpouseRelationshipNumber"] as string,
                WorkPermit = reader["WorkPermitCode"] as string,
                MaritalStatus = reader.GetInt32(reader.GetOrdinal("MaritalStatusBackingValue")),
                DateStatus = reader.GetNullableDateTime("StatusValidFrom"),
                WithoutLegalCapacity = reader["WithoutLegalCapacityCode"] as string,
                StatusCountryCode = reader.GetNullableInt("StatusCountryCode"),
                RegStatus = reader.GetNullableInt("StatusBackingValue"),
                OldNIN = reader["FormerCensusNumber"] as string,
                NewNIN = reader["SupersededByCensusNumber"] as string,
            };
        }

        public static Address MapToAddress(this SqlDataReader reader)
        {
            //object testsn = reader["ZipCode"].ToString();
            //var value = reader["ZipCode"] as string;


            return new Address
            {
                AddressLine1 = reader["AddressLine1"] as string,
                AddressLine2 = reader["AddressLine2"] as string,
                AddressLine3 = reader["AddressLine3"] as string,
                ApartmentNumber = reader["ApartmentNumber"] as string,
                BasicStatisticalUnit = reader.GetNullableInt("BasicStatisticalUnit"),
                CadastralNumber = reader["CadastralNumber"] as string,
                CoAddress = reader["CoAddress"] as string,
                Constituency = reader["Constituency"] as string,
                Country = reader["Country"] as string,
                DistrictCode = reader["DistrictCode"] as string,
                DistrictName = reader["DistrictName"] as string,
                Municipality = MunicipalityCodeResolver.GetNameForCode(reader["MunicipalityCode"] as string),
                St = reader["MunicipalityCode"] as string,
                PostalAddress = reader["ComputedStreetAddressLine"] as string,
                PostalCode = reader["ZipCode"] as string,
                PostalPlace = ZipCodeResolver.GetNameForCode(reader["ZipCode"] as string),
                PropertyNumber = reader["PropertyNumber"] as string,
                SchoolDistrict = reader["SchoolDistrict"] as string,
                StreetName = reader["StreetName"] as string,
                StreetNumber = reader["StreetNumber"] as string,
                HouseLetter = reader["HouseLetter"] as string,
                CurrentAddress = reader.GetBooleanTrueFalse("CurrentAddress"),
                HouseNumber = reader.GetNullableInt("HouseNumber"),
                XCoord = reader.GetNullableDecimal("XCoord"),
                YCoord = reader.GetNullableDecimal("YCoord"),
                PostalAddressValidFrom = reader.GetNullableDateTime("AddressLineValidFrom"),
                DateAdrFrom = reader.GetNullableDateTime("ValidFrom"),
                NIN = reader["addressNinKey"] as string,
                PostalType = reader.GetNullableInt("Code"),
                DatePostalType = reader.GetNullableDateTime("CodeValidFrom")
            };
        }
    }
}
