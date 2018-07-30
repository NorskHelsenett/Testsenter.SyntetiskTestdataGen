using System;
using System.Data.SqlTypes;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models.PregV1;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public class AddressPreg
    {
        private Address _address;

        public AddressPreg(Address address)
        {
            _address = address;
            CreatedDate = PersonWithMetadata.GetCreatedDate(address) ?? DateTime.Now.AddDays(-1);
            UpdatedDate = PersonWithMetadata.GetUpdatedDate(address) ?? DateTime.Now;
        }

        public long Id { get; set; }


        public string PersonId => _address.NIN;

        public bool CurrentAddress => _address.CurrentAddress;

        public string StreetName => _address.StreetName;

        public string HouseLetter => _address.HouseLetter;

        public string ZipCode => _address.PostalCode;

        public string MunicipalityCode => _address.St;

        public string AddressLine1 => _address.AddressLine1;

        public string AddressLine2 => _address.AddressLine2;

        public string AddressLine3 => _address.AddressLine3;

        public string CoAddress => _address.CoAddress;

        public string Country => _address.Country;

        public string DistrictCode => _address.DistrictCode;

        public string DistrictName => _address.DistrictName;

        public string StreetNumber => _address.StreetNumber;

        public string ApartmentNumber => _address.ApartmentNumber;

        public string CadastralNumber => _address.CadastralNumber;

        public string PropertyNumber => _address.PropertyNumber;

        public int? HouseNumber => _address.HouseNumber;

        public SqlDateTime? ValidFrom => _address.DateAdrFrom;
        public int? Code => _address.PostalType;

        public SqlDateTime? CodeValidFrom => _address.DatePostalType;

        public decimal? XCoord => _address.XCoord;
        public decimal? YCoord => _address.YCoord;

        public SqlDateTime CreatedDate { get; set; }
        public SqlDateTime UpdatedDate { get; set; }

        public SqlDateTime? AddressLineValidFrom => _address.PostalAddressValidFrom;

        public string SchoolDistrict => _address.SchoolDistrict;

        public string Constituency => _address.Constituency;

        public int? BasicStatisticalUnit => _address.BasicStatisticalUnit;
    }

}