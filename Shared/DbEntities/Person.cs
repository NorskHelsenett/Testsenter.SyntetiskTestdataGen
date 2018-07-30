namespace SyntetiskTestdataGen.Shared.DbEntities
{
    public class Person
    {
        
        private Address[] AddressesField;

        
        private string CitizenshipField;

        
        private string CitizenshipCodeField;

        
        private string CustodyField;

        
        private System.Nullable<System.DateTime> DateCitizenshipField;

        
        private System.Nullable<System.DateTime> DateCustodyField;

        
        private System.Nullable<System.DateTime> DateMaritalStatusField;

        
        private System.Nullable<System.DateTime> DateOfBirthField;

        
        private System.Nullable<System.DateTime> DateParentalResponsibilityField;

        
        private System.Nullable<System.DateTime> DateStatusField;

        
        private System.Nullable<System.DateTime> DateWithoutLegalCapacityField;

        
        private System.Nullable<System.DateTime> DateWorkPermitField;

        
        private string DufNoField;

        
        private string FathersNINField;

        
        private string GivenNameField;

        
        private System.Nullable<int> MaritalStatusField;

        
        private string MiddleNameField;

        
        private string MothersNINField;

        
        private string NINField;

        
        private string NewNINField;

        
        private string OldNINField;

        
        private System.Nullable<int> ParentalResponsibilityField;

        
        private System.Nullable<int> RegStatusField;

        
        private string SnField;

        
        private string SpouseNINField;

        
        private System.Nullable<int> StatusCountryCodeField;

        
        private string WithoutLegalCapacityField;

        
        private string WorkPermitField;


        public Address[] Addresses
        {
            get
            {
                return this.AddressesField;
            }
            set
            {
                if ((object.ReferenceEquals(this.AddressesField, value) != true))
                {
                    this.AddressesField = value;
                    this.RaisePropertyChanged("Addresses");
                }
            }
        }

        public string Citizenship
        {
            get
            {
                return this.CitizenshipField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CitizenshipField, value) != true))
                {
                    this.CitizenshipField = value;
                    this.RaisePropertyChanged("Citizenship");
                }
            }
        }

        public string CitizenshipCode
        {
            get
            {
                return this.CitizenshipCodeField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CitizenshipCodeField, value) != true))
                {
                    this.CitizenshipCodeField = value;
                    this.RaisePropertyChanged("CitizenshipCode");
                }
            }
        }

        public string Custody
        {
            get
            {
                return this.CustodyField;
            }
            set
            {
                if ((object.ReferenceEquals(this.CustodyField, value) != true))
                {
                    this.CustodyField = value;
                    this.RaisePropertyChanged("Custody");
                }
            }
        }

        public System.Nullable<System.DateTime> DateCitizenship
        {
            get
            {
                return this.DateCitizenshipField;
            }
            set
            {
                if ((this.DateCitizenshipField.Equals(value) != true))
                {
                    this.DateCitizenshipField = value;
                    this.RaisePropertyChanged("DateCitizenship");
                }
            }
        }

        public System.Nullable<System.DateTime> DateCustody
        {
            get
            {
                return this.DateCustodyField;
            }
            set
            {
                if ((this.DateCustodyField.Equals(value) != true))
                {
                    this.DateCustodyField = value;
                    this.RaisePropertyChanged("DateCustody");
                }
            }
        }

        public System.Nullable<System.DateTime> DateMaritalStatus
        {
            get
            {
                return this.DateMaritalStatusField;
            }
            set
            {
                if ((this.DateMaritalStatusField.Equals(value) != true))
                {
                    this.DateMaritalStatusField = value;
                    this.RaisePropertyChanged("DateMaritalStatus");
                }
            }
        }

        public System.Nullable<System.DateTime> DateOfBirth
        {
            get
            {
                return this.DateOfBirthField;
            }
            set
            {
                if ((this.DateOfBirthField.Equals(value) != true))
                {
                    this.DateOfBirthField = value;
                    this.RaisePropertyChanged("DateOfBirth");
                }
            }
        }

        public System.Nullable<System.DateTime> DateParentalResponsibility
        {
            get
            {
                return this.DateParentalResponsibilityField;
            }
            set
            {
                if ((this.DateParentalResponsibilityField.Equals(value) != true))
                {
                    this.DateParentalResponsibilityField = value;
                    this.RaisePropertyChanged("DateParentalResponsibility");
                }
            }
        }

        public System.Nullable<System.DateTime> DateStatus
        {
            get
            {
                return this.DateStatusField;
            }
            set
            {
                if ((this.DateStatusField.Equals(value) != true))
                {
                    this.DateStatusField = value;
                    this.RaisePropertyChanged("DateStatus");
                }
            }
        }

        public System.Nullable<System.DateTime> DateWithoutLegalCapacity
        {
            get
            {
                return this.DateWithoutLegalCapacityField;
            }
            set
            {
                if ((this.DateWithoutLegalCapacityField.Equals(value) != true))
                {
                    this.DateWithoutLegalCapacityField = value;
                    this.RaisePropertyChanged("DateWithoutLegalCapacity");
                }
            }
        }

        public System.Nullable<System.DateTime> DateWorkPermit
        {
            get
            {
                return this.DateWorkPermitField;
            }
            set
            {
                if ((this.DateWorkPermitField.Equals(value) != true))
                {
                    this.DateWorkPermitField = value;
                    this.RaisePropertyChanged("DateWorkPermit");
                }
            }
        }

        public string DufNo
        {
            get
            {
                return this.DufNoField;
            }
            set
            {
                if ((object.ReferenceEquals(this.DufNoField, value) != true))
                {
                    this.DufNoField = value;
                    this.RaisePropertyChanged("DufNo");
                }
            }
        }

        public string FathersNIN
        {
            get
            {
                return this.FathersNINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.FathersNINField, value) != true))
                {
                    this.FathersNINField = value;
                    this.RaisePropertyChanged("FathersNIN");
                }
            }
        }

        public string GivenName
        {
            get
            {
                return this.GivenNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.GivenNameField, value) != true))
                {
                    this.GivenNameField = value;
                    this.RaisePropertyChanged("GivenName");
                }
            }
        }

        public System.Nullable<int> MaritalStatus
        {
            get
            {
                return this.MaritalStatusField;
            }
            set
            {
                if ((this.MaritalStatusField.Equals(value) != true))
                {
                    this.MaritalStatusField = value;
                    this.RaisePropertyChanged("MaritalStatus");
                }
            }
        }

        public string MiddleName
        {
            get
            {
                return this.MiddleNameField;
            }
            set
            {
                if ((object.ReferenceEquals(this.MiddleNameField, value) != true))
                {
                    this.MiddleNameField = value;
                    this.RaisePropertyChanged("MiddleName");
                }
            }
        }

        public string MothersNIN
        {
            get
            {
                return this.MothersNINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.MothersNINField, value) != true))
                {
                    this.MothersNINField = value;
                    this.RaisePropertyChanged("MothersNIN");
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

        public string NewNIN
        {
            get
            {
                return this.NewNINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.NewNINField, value) != true))
                {
                    this.NewNINField = value;
                    this.RaisePropertyChanged("NewNIN");
                }
            }
        }

        public string OldNIN
        {
            get
            {
                return this.OldNINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.OldNINField, value) != true))
                {
                    this.OldNINField = value;
                    this.RaisePropertyChanged("OldNIN");
                }
            }
        }

        public System.Nullable<int> ParentalResponsibility
        {
            get
            {
                return this.ParentalResponsibilityField;
            }
            set
            {
                if ((this.ParentalResponsibilityField.Equals(value) != true))
                {
                    this.ParentalResponsibilityField = value;
                    this.RaisePropertyChanged("ParentalResponsibility");
                }
            }
        }

        public System.Nullable<int> RegStatus
        {
            get
            {
                return this.RegStatusField;
            }
            set
            {
                if ((this.RegStatusField.Equals(value) != true))
                {
                    this.RegStatusField = value;
                    this.RaisePropertyChanged("RegStatus");
                }
            }
        }

        public string Sn
        {
            get
            {
                return this.SnField;
            }
            set
            {
                if ((object.ReferenceEquals(this.SnField, value) != true))
                {
                    this.SnField = value;
                    this.RaisePropertyChanged("Sn");
                }
            }
        }

        public string SpouseNIN
        {
            get
            {
                return this.SpouseNINField;
            }
            set
            {
                if ((object.ReferenceEquals(this.SpouseNINField, value) != true))
                {
                    this.SpouseNINField = value;
                    this.RaisePropertyChanged("SpouseNIN");
                }
            }
        }

        public System.Nullable<int> StatusCountryCode
        {
            get
            {
                return this.StatusCountryCodeField;
            }
            set
            {
                if ((this.StatusCountryCodeField.Equals(value) != true))
                {
                    this.StatusCountryCodeField = value;
                    this.RaisePropertyChanged("StatusCountryCode");
                }
            }
        }

        public string WithoutLegalCapacity
        {
            get
            {
                return this.WithoutLegalCapacityField;
            }
            set
            {
                if ((object.ReferenceEquals(this.WithoutLegalCapacityField, value) != true))
                {
                    this.WithoutLegalCapacityField = value;
                    this.RaisePropertyChanged("WithoutLegalCapacity");
                }
            }
        }

        public string WorkPermit
        {
            get
            {
                return this.WorkPermitField;
            }
            set
            {
                if ((object.ReferenceEquals(this.WorkPermitField, value) != true))
                {
                    this.WorkPermitField = value;
                    this.RaisePropertyChanged("WorkPermit");
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
