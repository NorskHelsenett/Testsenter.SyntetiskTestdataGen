using System;
using System.Collections.Generic;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class NameStatistics : ISetStatistics
    {
        public BooleanStatistic HasFirstname { get; set; }
        public DiscreteStatistic GivenHasFirstName_HowManyNames { get; set; }
        public BooleanStatistic HasMiddlename { get; set; }
        public DiscreteStatistic GivenHasMiddleName_HowManyNames { get; set; }
        public BooleanStatistic HasSirname { get; set; }
        public DiscreteStatistic GivenHasHasSirname_HowManyNames { get; set; }
        public BooleanStatistic HasMultipleSirname_UseBindestrek { get; set; }

        public NameStatistics(int numberOfSamplesToTake)
        {
            HasFirstname = new BooleanStatistic(numberOfSamplesToTake, "HasFirstname");
            GivenHasFirstName_HowManyNames = new DiscreteStatistic(numberOfSamplesToTake, "GivenHasFirstName_HowManyNames");
            HasMiddlename = new BooleanStatistic(numberOfSamplesToTake, "HasMiddlename");
            GivenHasMiddleName_HowManyNames = new DiscreteStatistic(numberOfSamplesToTake, "GivenHasMiddleName_HowManyNames");
            GivenHasHasSirname_HowManyNames = new DiscreteStatistic(numberOfSamplesToTake, "GivenHasHasSirname_HowManyNames");
            HasSirname = new BooleanStatistic(numberOfSamplesToTake, "HasSirname");
            HasMultipleSirname_UseBindestrek = new BooleanStatistic(numberOfSamplesToTake, "HasMultipleSirname_UseBindestrek");
        }

        public void FromImport(Person p, int ageQuant)
        {
            HasFirstname.Update(string.IsNullOrEmpty(p.GivenName) || string.IsNullOrWhiteSpace(p.GivenName) ? 0 : 1);
            HasMiddlename.Update(string.IsNullOrEmpty(p.MiddleName) || string.IsNullOrWhiteSpace(p.MiddleName) ? 0 : 1);

            if (!string.IsNullOrEmpty(p.GivenName))
                GivenHasFirstName_HowManyNames.Update(p.GivenName.Split(' ').Length);

            if (!string.IsNullOrEmpty(p.MiddleName))
                GivenHasMiddleName_HowManyNames.Update(p.MiddleName.Split(' ').Length);

            HasSirname.Update(string.IsNullOrEmpty(p.Sn) || string.IsNullOrWhiteSpace(p.Sn) ? 0 : 1);
            if (!string.IsNullOrEmpty(p.Sn))
            {
                var numberOfSirnames = Math.Max(p.Sn.Split(' ').Length, p.Sn.Split('-').Length);

                GivenHasHasSirname_HowManyNames.Update(numberOfSirnames);

                if (numberOfSirnames > 1)
                    HasMultipleSirname_UseBindestrek.Update(p.Sn.Contains("-") ? 1 : 0);
            }
        }

        public void AfterImport(Dictionary<string, PregNode> applicablepersons, Dictionary<string, PregNode> allpersons)
        {
        }

        public void SetDistribution(bool dispose)
        {
            HasFirstname.SetDistribution();
            HasMiddlename.SetDistribution();
            HasSirname.SetDistribution();

            if(dispose)
                DisposeSamples();
        }

        public void DisposeSamples()
        {
            HasFirstname.DisposeSamples();
            HasMiddlename.DisposeSamples();
            HasSirname.DisposeSamples();
        }
    }
}
