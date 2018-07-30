using System;
using System.Collections.Generic;
using System.Linq;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class AdressStatistics : ISetStatistics
    {
        public BooleanStatistic HasAdressLine1_WhenHasAdressLine12or3 { get; set; }
        public BooleanStatistic HasAdressLine2_WhenHasAdressLine12or3 { get; set; }
        public BooleanStatistic HasAdressLine3_WhenHasAdressLine12or3 { get; set; }
        public BooleanStatistic HasAdressLine12or3 { get; set; }
        public BooleanStatistic HasAppartmentNumber { get; set; }
        public BooleanStatistic HasBasicStatisticalUnit { get; set; }
        public BooleanStatistic HasCadastralNumber { get; set; }
        public BooleanStatistic HasCoAdress { get; set; }
        public BooleanStatistic HasConstituency { get; set; }
        public BooleanStatistic HasCountry { get; set; }
        public BooleanStatistic IsCurrentAdress { get; set; }
        public BooleanStatistic HasDateAdrFrom { get; set; }
        public BooleanStatistic HasDatePostalPlace { get; set; }
        public BooleanStatistic HasDistrictCodeandDistrictName { get; set; }
        public BooleanStatistic HasHouseLetter { get; set; }
        public BooleanStatistic HasHouseNumber { get; set; }
        public BooleanStatistic HasSameNinAsPersonObject { get; set; }
        public BooleanStatistic HasPostalAdress { get; set; }
        public BooleanStatistic HasPostalAddressValidFrom { get; set; }
        public BooleanStatistic HasPostalCode { get; set; }
        public BooleanStatistic HasPostalCodeGivenIsDead { get; set; }
        public BooleanStatistic HasPostalPlace { get; set; }
        public DiscreteStatistic PostalType { get; set; }
        public BooleanStatistic HasPropertyNumber { get; set; }
        public BooleanStatistic HasSchoolDistrict { get; set; }
        public BooleanStatistic HasSt { get; set; }
        public BooleanStatistic HasStreetname { get; set; }
        public BooleanStatistic HasStreetnumber { get; set; }
        public BooleanStatistic HasXcoordYcoord { get; set; }
        public DiscreteStringStatistics GivenHasDnummer_KommuneNr { get; set; } 
        public DiscreteStatistic NumberOfAdressObjects { get; set; }
        public BooleanStatistic GivenMultipleAdressObject_HasSameNinAsPersonObject { get; set; }

        public AdressStatistics(int numberOfSamples)
        {
            NumberOfAdressObjects = new DiscreteStatistic(numberOfSamples, "NumberOfAdressObjects");
            GivenMultipleAdressObject_HasSameNinAsPersonObject = new BooleanStatistic(numberOfSamples, "GivenMultipleAdressObject_HasSameNinAsPersonObject");
            HasAdressLine12or3 = new BooleanStatistic(numberOfSamples, "HasAdressLine12or3");
            HasAdressLine1_WhenHasAdressLine12or3 = new BooleanStatistic(numberOfSamples, "HasAdressLine1_WhenHasAdressLine12or3");
            HasAdressLine2_WhenHasAdressLine12or3 = new BooleanStatistic(numberOfSamples, "HasAdressLine2_WhenHasAdressLine12or3");
            HasAdressLine3_WhenHasAdressLine12or3 = new BooleanStatistic(numberOfSamples, "HasAdressLine3_WhenHasAdressLine12or3");
            HasAppartmentNumber = new BooleanStatistic(numberOfSamples, "HasAppartmentNumber");
            HasBasicStatisticalUnit = new BooleanStatistic(numberOfSamples, "HasBasicStatisticalUnit");
            HasCadastralNumber = new BooleanStatistic(numberOfSamples, "HasCadastralNumber");
            HasCoAdress = new BooleanStatistic(numberOfSamples, "HasCoAdress");
            HasConstituency = new BooleanStatistic(numberOfSamples, "HasConstituency");
            HasCountry = new BooleanStatistic(numberOfSamples, "HasCountry");
            IsCurrentAdress = new BooleanStatistic(numberOfSamples, "IsCurrentAdress");
            HasDateAdrFrom = new BooleanStatistic(numberOfSamples, "HasDateAdrFrom");
            HasDatePostalPlace = new BooleanStatistic(numberOfSamples, "HasDatePostalPlace");
            HasDistrictCodeandDistrictName = new BooleanStatistic(numberOfSamples, "HasDistrictCodeandDistrictName");
            HasHouseLetter = new BooleanStatistic(numberOfSamples, "HasHouseLetter");
            HasHouseNumber = new BooleanStatistic(numberOfSamples, "HasHouseNumber");
            HasSameNinAsPersonObject = new BooleanStatistic(numberOfSamples, "HasSameNinAsPersonObject");
            HasPostalAdress = new BooleanStatistic(numberOfSamples, "HasPostalAdress");
            HasPostalAddressValidFrom = new BooleanStatistic(numberOfSamples, "HasPostalAddressValidFrom");
            HasPostalCode = new BooleanStatistic(numberOfSamples, "HasPostalCode");
            HasPostalCodeGivenIsDead = new BooleanStatistic(numberOfSamples, "HasPostalCodeGivenIsDead");
            HasPostalPlace = new BooleanStatistic(numberOfSamples, "HasPostalPlace");
            PostalType = new DiscreteStatistic(numberOfSamples, "PostalType");
            HasPropertyNumber = new BooleanStatistic(numberOfSamples, "HasPropertyNumber");
            HasSchoolDistrict = new BooleanStatistic(numberOfSamples, "HasSchoolDistrict");
            HasSt = new BooleanStatistic(numberOfSamples, "HasSt");
            HasStreetname = new BooleanStatistic(numberOfSamples, "HasStreetname");
            HasStreetnumber = new BooleanStatistic(numberOfSamples, "HasStreetnumber");
            HasXcoordYcoord = new BooleanStatistic(numberOfSamples, "HasXcoordYcoord");
            GivenHasDnummer_KommuneNr = new DiscreteStringStatistics(numberOfSamples, "GivenHasDnummer_KommuneNr", (int?) null);
        }
        public void FromImport(Person p, int ageQuant)
        {
            NumberOfAdressObjects.Update(p.Addresses?.Length ?? 0);
            if (p.Addresses == null)
                return;

            var adress = GetCurrent(p.Addresses);

            if(CommonFunctions.HasDnummer(p.NIN))
                GivenHasDnummer_KommuneNr.Update(adress.St);

            var hasAdr = IsNotNullOrEmptyOrWhitespace(adress.AddressLine1) || IsNotNullOrEmptyOrWhitespace(adress.AddressLine2) || IsNotNullOrEmptyOrWhitespace(adress.AddressLine3);
            HasAdressLine12or3.Update(hasAdr ? 1 : 0);
            if (hasAdr)
            {
                HasAdressLine1_WhenHasAdressLine12or3.Update(IsNotNullOrEmptyOrWhitespace(adress.AddressLine1) ? 1 : 0);
                HasAdressLine2_WhenHasAdressLine12or3.Update(IsNotNullOrEmptyOrWhitespace(adress.AddressLine2) ? 1 : 0);
                HasAdressLine3_WhenHasAdressLine12or3.Update(IsNotNullOrEmptyOrWhitespace(adress.AddressLine3) ? 1 : 0);
            }
            
            HasAppartmentNumber.Update(IsNotNullOrEmptyOrWhitespace(adress.ApartmentNumber) ? 1 : 0);
            HasCoAdress.Update(IsNotNullOrEmptyOrWhitespace(adress.CoAddress) ? 1 : 0);
            HasCountry.Update(IsNotNullOrEmptyOrWhitespace(adress.Country) ? 1 : 0);
            HasDateAdrFrom.Update(adress.DateAdrFrom.HasValue ? 1 : 0);
            HasDatePostalPlace.Update(adress.DatePostalType.HasValue ? 1 : 0);
            HasHouseLetter.Update(IsNotNullOrEmptyOrWhitespace(adress.HouseLetter) ? 1 : 0);
            HasHouseNumber.Update(adress.HouseNumber.HasValue ? 1 : 0);
            HasPostalAddressValidFrom.Update(adress.PostalAddressValidFrom.HasValue ? 1 : 0);
            HasPostalAdress.Update(IsNotNullOrEmptyOrWhitespace(adress.PostalAddress) ? 1 : 0);
            HasPostalCode.Update(IsNotNullOrEmptyOrWhitespace(adress.PostalCode) ? 1 : 0);
            HasPostalPlace.Update(IsNotNullOrEmptyOrWhitespace(adress.PostalPlace) ? 1 : 0);
            
            PostalType.Update(adress.PostalType);
            HasSameNinAsPersonObject.Update(adress.NIN == p.NIN ? 1 : 0);
            HasSchoolDistrict.Update(IsNotNullOrEmptyOrWhitespace(adress.SchoolDistrict) ? 1 : 0);
            HasSt.Update(IsNotNullOrEmptyOrWhitespace(adress.St) ? 1 : 0);
            HasStreetname.Update(IsNotNullOrEmptyOrWhitespace(adress.StreetName) ? 1 : 0);
            HasStreetnumber.Update(IsNotNullOrEmptyOrWhitespace(adress.StreetNumber) ? 1 : 0);
            HasXcoordYcoord.Update(adress.XCoord.HasValue || adress.YCoord.HasValue ? 1 : 0);

            HasBasicStatisticalUnit.Update(adress.BasicStatisticalUnit.HasValue ? 1 : 0);
            HasCadastralNumber.Update(IsNotNullOrEmptyOrWhitespace(adress.CadastralNumber) ? 1:0);
            HasConstituency.Update(IsNotNullOrEmptyOrWhitespace(adress.Constituency) ? 1 : 0);
            HasDistrictCodeandDistrictName.Update(IsNotNullOrEmptyOrWhitespace(adress.DistrictCode) || IsNotNullOrEmptyOrWhitespace(adress.DistrictName) ? 1 : 0);
            HasPropertyNumber.Update(IsNotNullOrEmptyOrWhitespace(adress.PropertyNumber) ? 1 : 0);

            if (PersonStatistics.IsPersonDead(p) == 1)
                HasPostalCodeGivenIsDead.Update(IsNotNullOrEmptyOrWhitespace(adress.PostalCode) ? 1 : 0);

            if (p.Addresses.Length > 1)
            {
                foreach(var obsoleteAdress in p.Addresses.Where(a => !a.CurrentAddress))
                    GivenMultipleAdressObject_HasSameNinAsPersonObject.Update(obsoleteAdress.NIN == p.NIN ? 1 : 0);
            }
        }

        public void SetDistribution(bool dispose)
        {
            NumberOfAdressObjects.SetDistribution();
            GivenMultipleAdressObject_HasSameNinAsPersonObject.SetDistribution();
            HasAdressLine12or3.SetDistribution();
            HasAdressLine1_WhenHasAdressLine12or3.SetDistribution();
            HasAdressLine2_WhenHasAdressLine12or3.SetDistribution();
            HasAdressLine3_WhenHasAdressLine12or3.SetDistribution();
            HasAppartmentNumber.SetDistribution();
            HasBasicStatisticalUnit.SetDistribution();
            HasCadastralNumber.SetDistribution();
            HasCoAdress.SetDistribution();
            HasConstituency.SetDistribution();
            HasCountry.SetDistribution();
            IsCurrentAdress.SetDistribution();
            HasDateAdrFrom.SetDistribution();
            HasDatePostalPlace.SetDistribution();
            HasDistrictCodeandDistrictName.SetDistribution();
            HasHouseLetter.SetDistribution();
            HasHouseNumber.SetDistribution();
            HasSameNinAsPersonObject.SetDistribution();
            HasPostalAdress.SetDistribution();
            HasPostalAddressValidFrom.SetDistribution();
            HasPostalCode.SetDistribution();
            HasPostalPlace.SetDistribution();
            PostalType.SetDistribution();
            HasPropertyNumber.SetDistribution();
            HasSchoolDistrict.SetDistribution();
            HasSt.SetDistribution();
            HasStreetname.SetDistribution();
            HasStreetnumber.SetDistribution();
            HasXcoordYcoord.SetDistribution();

            if(dispose)
                DisposeSamples();

        }

        public void DisposeSamples()
        {
            NumberOfAdressObjects.DisposeSamples();
            HasAdressLine12or3.DisposeSamples();
            GivenMultipleAdressObject_HasSameNinAsPersonObject.DisposeSamples();
            HasAdressLine1_WhenHasAdressLine12or3.DisposeSamples();
            HasAdressLine2_WhenHasAdressLine12or3.DisposeSamples();
            HasAdressLine3_WhenHasAdressLine12or3.DisposeSamples();
            HasAppartmentNumber.DisposeSamples();
            HasBasicStatisticalUnit.DisposeSamples();
            HasCadastralNumber.DisposeSamples();
            HasCoAdress.DisposeSamples();
            HasConstituency.DisposeSamples();
            HasCountry.DisposeSamples();
            IsCurrentAdress.DisposeSamples();
            HasDateAdrFrom.DisposeSamples();
            HasDatePostalPlace.DisposeSamples();
            HasDistrictCodeandDistrictName.DisposeSamples();
            HasHouseLetter.DisposeSamples();
            HasHouseNumber.DisposeSamples();
            HasSameNinAsPersonObject.DisposeSamples();
            HasPostalAdress.DisposeSamples();
            HasPostalAddressValidFrom.DisposeSamples();
            HasPostalCode.DisposeSamples();
            HasPostalPlace.DisposeSamples();
            PostalType.DisposeSamples();
            HasPropertyNumber.DisposeSamples();
            HasSchoolDistrict.DisposeSamples();
            HasSt.DisposeSamples();
            HasStreetname.DisposeSamples();
            HasStreetnumber.DisposeSamples();
            HasXcoordYcoord.DisposeSamples();
        }

        private bool IsNotNullOrEmptyOrWhitespace(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
        }

        public static Address GetCurrent(Address[] pAddresses)
        {
            try
            {
                if (pAddresses.Length == 1)
                    return pAddresses.First();

                return pAddresses.First(y => y.CurrentAddress);
            }
            catch (Exception e)
            {

                return pAddresses.First();

            }
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
        }
    }
}
