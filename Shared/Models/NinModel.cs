using System;
using System.Linq;
using SyntetiskTestdataGen.Shared.Resources;

namespace SyntetiskTestdataGen.Shared.Models
{
    public static class NinModel
    {
        public static readonly int[] ControllSiffer1 = { 3, 7, 6, 1, 8, 9, 4, 5, 2, 1 };
        public static readonly int[] ControllSiffer2 = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 1 };

        public static Tuple<DateTime, string> GetBirthdayAndNin(int age, bool isFemale, bool asDnummer, Randomizer randy)
        {
            var bday = GetBirthday(age, randy);
            var firstPartOfNin = bday.ToString("ddMMyy");
            if (asDnummer)
                firstPartOfNin = CommonFunctions.NinToDnummer(firstPartOfNin);

            string nin = null;

            while (nin == null)
            {
                var secondPartOfNin = GetRandomSiffer(null, randy) + GetRandomSiffer(null, randy) + GetRandomSiffer(isFemale, randy);

                nin = firstPartOfNin + secondPartOfNin;
                nin = AppendKontrollSiffer(nin, ControllSiffer1);
                nin = AppendKontrollSiffer(nin, ControllSiffer2);
            }

            return new Tuple<DateTime, string>(bday, nin);
        }

        public static string GetRandomSiffer(bool? isFemale, Randomizer randy)
        {
            while (true)
            {
                var s = randy.Next(10);
                if (!isFemale.HasValue)
                    return s.ToString();

                var correctKjonn = isFemale.Value == true == (s % 2 == 0);

                if (correctKjonn)
                    return s.ToString();
            }
        }

        public static int GetRandomAge(int importedPersonAgeQuants, int ageQuantMultiplier, Randomizer randy)
        {
            var minimumAge = ageQuantMultiplier * importedPersonAgeQuants;
            var ageAboveMinimum = randy.Next(ageQuantMultiplier);

            return minimumAge + ageAboveMinimum;
        }

        public static DateTime GetBirthday(int ageInYears, Randomizer randy)
        {
            var from = new DateTime(DateTime.Now.Year - ageInYears - 1, 1, 1).AddDays(DateTime.Now.DayOfYear);
            var to = new DateTime(DateTime.Now.Year - ageInYears, 1, 1).AddDays(DateTime.Now.DayOfYear);

            return GetRandomDate(from, to.Date, randy).Date;
        }

        public static DateTime GetRandomDate(DateTime from, DateTime unto, Randomizer randy)
        {
            if (from > unto)
            {
                throw new ArgumentException("from must be less than unto datetime");
            }
            var range = new TimeSpan(unto.Ticks - from.Ticks);
            return from + new TimeSpan((long)(range.Ticks * randy.NextDouble()));
        }

        public static string AppendKontrollSiffer(string personnr, int[] x)
        {
            if (personnr == null)
                return null;
            int[] allNumbers = new int[11];
            int index = 0;
            foreach (var item in personnr)
                allNumbers[index++] = int.Parse(item.ToString());

            int sum = 0;
            for (int i = 0; i < x.Length - 1; i++)
            {
                sum += allNumbers[i] * x[i];
            }

            int kontrollSiffer = 0;
            while (true)
            {
                var thisSum = sum + x.Last() * kontrollSiffer;
                if (thisSum % 11 == 0)
                    return personnr + kontrollSiffer;

                kontrollSiffer++;
                if (kontrollSiffer > 9)
                    return null;
            }
        }

        public static bool IsValidNin(string nin)
        {
            try
            {
                if (nin.Length != 11)
                    return false;

                var withNoControllSiffer = nin.Substring(0, 9);
                var withOneControllSiffer = nin.Substring(0, 10);

                var controlSiffer1 = AppendKontrollSiffer(withNoControllSiffer, ControllSiffer1);
                if (controlSiffer1 != withOneControllSiffer)
                    return false;

                var controlSiffer2 = AppendKontrollSiffer(withOneControllSiffer, ControllSiffer2);
                if (controlSiffer2 != nin)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

