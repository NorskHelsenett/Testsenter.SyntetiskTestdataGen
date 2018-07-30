using System;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Models.PregV1;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public static class CommonFunctions
    {
        public static bool HasDnummer(string nin)
        {
            return nin.StartsWith("4") || nin.StartsWith("5") || nin.StartsWith("6") || nin.StartsWith("7");
        }

        public static string NinToDnummer(string nin)
        {
            var firstDigit = Int32.Parse(nin.Substring(0, 1));
            firstDigit += 4;

            return firstDigit + nin.Substring(1, nin.Length - 1);
        }

        public static string DnummerToNin(string dnummer)
        {
            var firstDigit = Int32.Parse(dnummer.Substring(0, 1));
            firstDigit -= 4;

            return firstDigit + dnummer.Substring(1, dnummer.Length - 1);
        }

        public static int? GetKjonn(string nin)
        {
            if (String.IsNullOrEmpty(nin) || nin.Length < 9)
                return null;

            int kjonnChar;
            if (Int32.TryParse(nin[8].ToString(), out kjonnChar))
                return (kjonnChar % 2) == 0 ? 2 : 3;

            return null;
        }

        public static bool? IsFemale(string nin)
        {
            var kjonn = GetKjonn(nin);
            if (kjonn == null)
                return null;

            return kjonn == 2;
        }

        public static int GetYearFromNin(string personNin)
        {
            int year = Int32.Parse(personNin.Substring(4, 2));
            year = year <= DateTime.Today.Year % 100 ? year + 2000 : year + 1900;

            return year;
        }

        public static DateTimeOffset? GetBirthdayFromNin(string personNin)
        {
            if (String.IsNullOrEmpty(personNin))
                return null;

            try
            {
                int day = Int32.Parse(personNin.Substring(0, 2));
                //D-nummer
                if (day > 40)
                    day -= 40;

                int month = Int32.Parse(personNin.Substring(2, 2));
                int year = GetYearFromNin(personNin);

                return new DateTimeOffset(new DateTime(year, month, day));
            }
            catch (ArgumentException ae) when (ae.Message == "Year, Month, and Day parameters describe an un-representable DateTime.")
            {
                return null;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not get birthday from nin given nin " + personNin, e);
            }
        }

        public static int GetAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

            return (a - b) / 10000;
        }

        public static string GetAgeQuantDescription(int ageQuants, int? ageQuantLevel)
        {
            if (!ageQuantLevel.HasValue)
                return String.Empty;

            var min = ageQuantLevel.Value * ageQuants;
            var max = ((ageQuantLevel.Value+1) * ageQuants) -1;

            return "mellom " + min + " og " + max;
        }

        public static double GetCorrelation(PersonWithMetadata person, SynteticModel model, string valueString1, string valueString2)
        {
            if (model.Statistics.GetClosestStatisticByAgeQuant(person)?.Correlations == null)
                return model.Statistics.Correlations.GetValue(valueString1, valueString2).Value;

            return model.Statistics.GetClosestStatisticByAgeQuant(person).Correlations.GetValue(valueString1, valueString2).Value;
        }

        public static Tuple<bool, bool> GetDependentStatistic(BooleanStatistic aStat, BooleanStatistic bStat, double correlation, Randomizer randy)
        {
            var aStd = aStat.StDeviation;
            var bStd = bStat.StDeviation;
            var covariance = correlation * aStd * bStd;
            var pA = aStat.TrueRatio;
            var pB = bStat.TrueRatio;

            var pAandB = covariance + pA * pB;
            var pBgivenA = pAandB / pA;

            var pBandNotA = -covariance + (pB * (1 - pA));
            var pBgivenNotA = pBandNotA / (1 - pA);
            var resA = randy.HitPropabilityDecimal(pA);

            var resB = resA ? randy.HitPropabilityDecimal(pBgivenA) : randy.HitPropabilityDecimal(pBgivenNotA);

            return new Tuple<bool, bool>(resA, resB);
        }

        public static bool GetDependentStatisticGivenA(bool resA, BooleanStatistic aStat, BooleanStatistic bStat, double correlation, Randomizer randy)
        {
            var aStd = aStat.StDeviation;
            var bStd = bStat.StDeviation;
            var covariance = correlation * aStd * bStd;
            var pA = aStat.TrueRatio;
            var pB = bStat.TrueRatio;

            var pAandB = covariance + pA * pB;
            var pBgivenA = pAandB / pA;

            var pBandNotA = -covariance + (pB * (1 - pA));
            var pBgivenNotA = pBandNotA / (1 - pA);

            var resB = resA ? randy.HitPropabilityDecimal(pBgivenA) : randy.HitPropabilityDecimal(pBgivenNotA);

            return resB;
        }
    }
}
