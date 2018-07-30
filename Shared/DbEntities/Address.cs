namespace SyntetiskTestdataGen.Shared.DbEntities
{
    public class Address
    {
        private string AddressLine1Field;

        
        private string AddressLine2Field;

        
        private string AddressLine3Field;

        
        private string ApartmentNumberField;

        
        private System.Nullable<int> BasicStatisticalUnitField;

        
        private string CadastralNumberField;

        
        private string CoAddressField;

        
        private string ConstituencyField;

        
        private string CountryField;

        
        private bool CurrentAddressField;

        
        private System.Nullable<System.DateTime> DateAdrFromField;

        
        private System.Nullable<System.DateTime> DatePostalTypeField;

        
        private string DistrictCodeField;

        
        private string DistrictNameField;

        
        private string HouseLetterField;

        
        private System.Nullable<int> HouseNumberField;

        
        private string MunicipalityField;

        
        private string NINField;

        
        private string PostalAddressField;

        
        private System.Nullable<System.DateTime> PostalAddressValidFromField;

        
        private string PostalCodeField;

        
        private string PostalPlaceField;

        
        private System.Nullable<int> PostalTypeField;

        
        private string PropertyNumberField;

        
        private string SchoolDistrictField;

        
        private string StField;

        
        private string StreetNameField;

        
        private string StreetNumberField;

        
        private System.Nullable<decimal> XCoordField;

        
        private System.Nullable<decimal> YCoordField;

       
        public string AddressLine1
        {
            get
            {
                return this.AddressLine1Field;
            }
            set
            {
                if ((object.ReferenceEquals(this.AddressLine1Field, value) != true))
                {
                    this.AddressLine1Field = value;
                    this.RaisePropertyChanged("AddressLine1");
                }
            }
        }

        
        public string AddressLine2
        {
            get
            {
                return this.AddressLine2Field;
            }
            set
            {
                if ((object.ReferenceEquals(this.AddressLine2Field, value) != true))
                {
                    this.AddressLine2Field = value;
                    this.RaisePropertyChanged("AddressLine2");
                }
            }
        }

        
        public string AddressLine3
        {
            get
            {
                return this.AddressLine3Field;
            }
            set
            {
                if ((object.ReferenceEquals(this.AddressLine3Field, value) != true))
                {
                    this.AddressLine3Field = value;
                    this.RaisePropertyChanged("AddressLine3");
                }
            }
        }

        
        public string ApartmentNumber
        {
            get
            {
                return this.ApartmentNumberField;
            }
            set
            {
                if ((object.ReferenceEquals(this.ApartmentNumberField, value) != true))
                {
                    this.ApartmentNumberField = value;
                    this.RaisePropertyChanged("ApartmentNumber");
                }
            }
        }

        
        public System.Nullable<int> BasicStatisticalUnit
        {
            get
            {
                return this.BasicStatisticalUnitField;
            }
            set
            {
                if ((this.BasicStatisticalUnitField.Equals(value) != true))
                {
                    this.BasicStatisticalUnitField = value;
                    this.RaisePropertyChanged("BasicStatisticalUnit");
                }
            }
        }

        
        public string CadastralNumber
        {
            get
            {
                return this.CadastralNumberField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CadastralNumberField, value) != true))
                {
                    this.CadastralNumberField = value;
                    this.RaisePropertyChanged("CadastralNumber");
                }
            }
        }

        
        public string CoAddress
        {
            get
            {
                return this.CoAddressField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CoAddressField, value) != true))
                {
                    this.CoAddressField = value;
                    this.RaisePropertyChanged("CoAddress");
                }
            }
        }

        
        public string Constituency
        {
            get
            {
                return this.ConstituencyField;
            }
            set
            {
                if ((object.ReferenceEquals(this.ConstituencyField, value) != true))
                {
                    this.ConstituencyField = value;
                    this.RaisePropertyChanged("Constituency");
                }
            }
        }

        
        public string Country
        {
            get
            {
                return this.CountryField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CountryField, value) != true))
                {
                    this.CountryField = value;
                    this.RaisePropertyChanged("Country");
                }
            }
        }

        
        public bool CurrentAddress
        {
            get
            {
                return this.CurrentAddressField;
            }
            set
            {
                if ((this.CurrentAddressField.Equals(value) != true))
                {
                    this.CurrentAddressField = value;
                    this.RaisePropertyChanged("CurrentAddress");
                }
            }
        }

        
        public System.Nullable<System.DateTime> DateAdrFrom
        {
            get
            {
                return this.DateAdrFromField;
            }
            set
            {
                if ((this.DateAdrFromField.Equals(value) != true))
                {
                    this.DateAdrFromField = value;
                    this.RaisePropertyChanged("DateAdrFrom");
                }
            }
        }

        
        public System.Nullable<System.DateTime> DatePostalType
        {
            get
            {
                return this.DatePostalTypeField;
            }
            set
            {
                if ((this.DatePostalTypeField.Equals(value) != true))
                {
                    this.DatePostalTypeField = value;
                    this.RaisePropertyChanged("DatePostalType");
                }
            }
        }

        
        public string DistrictCode
        {
            get
            {
                return this.DistrictCodeField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DistrictCodeField, value) != true))
                {
                    this.DistrictCodeField = value;
                    this.RaisePropertyChanged("DistrictCode");
                }
            }
        }

        
        public string DistrictName
        {
            get
            {
                return this.DistrictNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DistrictNameField, value) != true))
                {
                    this.DistrictNameField = value;
                    this.RaisePropertyChanged("DistrictName");
                }
            }
        }

        
        public string HouseLetter
        {
            get
            {
                return this.HouseLetterField;
            }
            set
            {
                if ((object.ReferenceEquals(this.HouseLetterField, value) != true))
                {
                    this.HouseLetterField = value;
                    this.RaisePropertyChanged("HouseLetter");
                }
            }
        }

        
        public System.Nullable<int> HouseNumber
        {
            get
            {
                return this.HouseNumberField;
            }
            set
            {
                if ((this.HouseNumberField.Equals(value) != true))
                {
                    this.HouseNumberField = value;
                    this.RaisePropertyChanged("HouseNumber");
                }
            }
        }

        
        public string Municipality
        {
            get
            {
                return this.MunicipalityField;
            }
            set
            {
                if ((object.ReferenceEquals(this.MunicipalityField, value) != true))
                {
                    this.MunicipalityField = value;
                    this.RaisePropertyChanged("Municipality");
                }
            }
        }

        
        public string NIN
        {
            get
            {
                return this.NINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.NINField, value) != true))
                {
                    this.NINField = value;
                    this.RaisePropertyChanged("NIN");
                }
            }
        }

        
        public string PostalAddress
        {
            get
            {
                return this.PostalAddressField;
            }
            set
            {
                if ((object.ReferenceEquals(this.PostalAddressField, value) != true))
                {
                    this.PostalAddressField = value;
                    this.RaisePropertyChanged("PostalAddress");
                }
            }
        }

        
        public System.Nullable<System.DateTime> PostalAddressValidFrom
        {
            get
            {
                return this.PostalAddressValidFromField;
            }
            set
            {
                if ((this.PostalAddressValidFromField.Equals(value) != true))
                {
                    this.PostalAddressValidFromField = value;
                    this.RaisePropertyChanged("PostalAddressValidFrom");
                }
            }
        }

        
        public string PostalCode
        {
            get
            {
                return this.PostalCodeField;
            }
            set
            {
                if ((object.ReferenceEquals(this.PostalCodeField, value) != true))
                {
                    this.PostalCodeField = value;
                    this.RaisePropertyChanged("PostalCode");
                }
            }
        }

        
        public string PostalPlace
        {
            get
            {
                return this.PostalPlaceField;
            }
            set
            {
                if ((object.ReferenceEquals(this.PostalPlaceField, value) != true))
                {
                    this.PostalPlaceField = value;
                    this.RaisePropertyChanged("PostalPlace");
                }
            }
        }

        
        public System.Nullable<int> PostalType
        {
            get
            {
                return this.PostalTypeField;
            }
            set
            {
                if ((this.PostalTypeField.Equals(value) != true))
                {
                    this.PostalTypeField = value;
                    this.RaisePropertyChanged("PostalType");
                }
            }
        }

        
        public string PropertyNumber
        {
            get
            {
                return this.PropertyNumberField;
            }
            set
            {
                if ((object.ReferenceEquals(this.PropertyNumberField, value) != true))
                {
                    this.PropertyNumberField = value;
                    this.RaisePropertyChanged("PropertyNumber");
                }
            }
        }

        
        public string SchoolDistrict
        {
            get
            {
                return this.SchoolDistrictField;
            }
            set
            {
                if ((object.ReferenceEquals(this.SchoolDistrictField, value) != true))
                {
                    this.SchoolDistrictField = value;
                    this.RaisePropertyChanged("SchoolDistrict");
                }
            }
        }

        
        public string St
        {
            get
            {
                return this.StField;
            }
            set
            {
                if ((object.ReferenceEquals(this.StField, value) != true))
                {
                    this.StField = value;
                    this.RaisePropertyChanged("St");
                }
            }
        }

        
        public string StreetName
        {
            get
            {
                return this.StreetNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.StreetNameField, value) != true))
                {
                    this.StreetNameField = value;
                    this.RaisePropertyChanged("StreetName");
                }
            }
        }

        
        public string StreetNumber
        {
            get
            {
                return this.StreetNumberField;
            }
            set
            {
                if ((object.ReferenceEquals(this.StreetNumberField, value) != true))
                {
                    this.StreetNumberField = value;
                    this.RaisePropertyChanged("StreetNumber");
                }
            }
        }

        
        public System.Nullable<decimal> XCoord
        {
            get
            {
                return this.XCoordField;
            }
            set
            {
                if ((this.XCoordField.Equals(value) != true))
                {
                    this.XCoordField = value;
                    this.RaisePropertyChanged("XCoord");
                }
            }
        }

        
        public System.Nullable<decimal> YCoord
        {
            get
            {
                return this.YCoordField;
            }
            set
            {
                if ((this.YCoordField.Equals(value) != true))
                {
                    this.YCoordField = value;
                    this.RaisePropertyChanged("YCoord");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
