using System;
using System.Globalization;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Accord.Statistics.Distributions;
using Accord.Statistics.Distributions.Univariate;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Shared.Statistics.Properties
{
    public class Statistic
    {
        protected readonly int _howManySamplesToTake;
        private int _index;
        public int TotalCount { get; set; }
        public int NumberOfNulls { get; set; }
        [JsonIgnore]
        public double[] Samples { get; set; }
        public int NumberOfSamples { get; set; }
        public bool IsBoolean { get; set; }
        public bool IsDiscrete { get; set; }
        public string DistributionfunctionString { get; set; }

        [JsonIgnore]
        public CorrelationFactory CorrelationFactory { get; set; }
        [JsonIgnore]
        public Func<Person, int, bool, double?> CalculateValueForStatistic { get; set; }
        [JsonIgnore]
        private ISampleableDistribution<double> _distributionFunction;
        [JsonIgnore]
        public ISampleableDistribution<double> DistributionFunction
        {
            get { return _distributionFunction;  }
            set
            {
                _distributionFunction = value;
                DistributionfunctionString = value.ToString();
            }
        }

        public string Name { get; set; }
        public bool CanGenerateSamples { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Variance { get; set; }
        public double StDeviation { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }

        [JsonConstructor]
        public Statistic( ) { }

        public Statistic(int howManySamplesToTake, string name)
        {
            Name = name;
            _howManySamplesToTake = howManySamplesToTake;
            Samples = new double[howManySamplesToTake];
        }

        public Statistic(int howManySamplesToTake, string name, CorrelationFactory correlationFactory, Func<Person, int, double?> calculateValueForStatisticx)
            : this(howManySamplesToTake, name)
        {
            CorrelationFactory = correlationFactory;
            CalculateValueForStatistic = (p, a, b) => calculateValueForStatisticx(p, a); 
        }

        public Statistic(int howManySamplesToTake, string name, CorrelationFactory correlationFactory, Func<Person, int, bool, double?> calculateValueForStatistic)
            : this(howManySamplesToTake, name)
        {
            CorrelationFactory = correlationFactory;
            CalculateValueForStatistic = calculateValueForStatistic;
        }

        public bool ApplicableForCorrelation()
        {
            return CorrelationFactory != null;
        }

        public void DisposeSamples()
        {
            Samples = null;
        }

        public double[] GetSamples()
        {
            if (NumberOfNulls == TotalCount)
                return null;

            return Samples;
        }

        public static double DontUpdateWhenThisValue = double.MaxValue;
        public static double ValueToUseWhenNull = -1000;

        public void Update(Person p, int ageQuant)
        {
            var value = GetValue(p, ageQuant, true);
            if (!value.HasValue)
                return;

            var valueToInsert = value.Value;
            CorrelationFactory?.Round1(Name, valueToInsert);

            Update(value);
        }

        public double? GetValue(Person p, int ageQuant, bool updateStats)
        {
            var value = CalculateValueForStatistic(p, ageQuant, updateStats);
            if (value.HasValue && value.Value == DontUpdateWhenThisValue)
                return null;

            return value ?? ValueToUseWhenNull;
        }

        public virtual void Update(double? value)
        {
            var valueToInsert = value ?? ValueToUseWhenNull;
            TotalCount++;
            NumberOfSamples++;

            if (value == ValueToUseWhenNull)
                NumberOfNulls++;

            if (Samples.Length <= _index)
                return;

            Samples[_index++] = valueToInsert;
        }

        public virtual void CustomUpdate(double? value) { }

        public virtual void SetDistribution()
        {
            if (NumberOfNulls == TotalCount)
                return;

            if (_howManySamplesToTake > 0 && NumberOfSamples < _howManySamplesToTake)
            {
                var temp = new double[NumberOfSamples];
                for (int i = 0; i < NumberOfSamples; i++)
                    temp[i] = Samples[i];

                Samples = temp;
            }

            if (!IsBoolean && !IsDiscrete)
                DistributionFunction = FindBestDistribution(Samples, IsBoolean);

            StDeviation = Samples.StandardDeviation();
            Mean = Samples.Mean();
            Variance = Samples.Variance();
            Median = Samples.Median();

            foreach (var sample in Samples)
            {
                Min = Min == null ? sample : Math.Min(Min.Value, sample);
                Max = Max == null ? sample : Math.Max(Max.Value, sample);
            }
        }

        public static ISampleableDistribution<double> FindBestDistribution(double[] samples, bool isBoolean)
        {
            var analysis = new DistributionAnalysis();
            analysis.Learn(samples);
            return analysis.GoodnessOfFit[0].Distribution as ISampleableDistribution<double>;
        }

        public string SetDistributionForBuilding(string name, int? ageQuant)
        {
            if (string.IsNullOrEmpty(Name))
                Name = name;

            if (DistributionFunction == null && DistributionfunctionString != null)
                SetDistributionFunctionFromString();

            if (DistributionFunction != null)
            {
                CanGenerateSamples = true;
                return string.Empty;
            }

            if (NumberOfSamples == 0 || StDeviation == 0)
            {
                CanGenerateSamples = false;
                return "Stat with name " + (Name) + " and ageQuant= " + (ageQuant ?? -1) + "is marked as CanGenerateSamples=false due to lacking samples";
            }

            try
            {
                DistributionFunction = new NormalDistribution(Mean, StDeviation);
                CanGenerateSamples = true;
                return "Warning: Found no distribution-function for " + (Name) + " and ageQuant= " + (ageQuant ?? -1) + $". Create one using NormalDistribution using mean={Mean} and stddev={StDeviation}";
            }
            catch (Exception e)
            {
                CanGenerateSamples = false;
                return "Stat with name " + (Name) + " and ageQuant= " + (ageQuant ?? -1) + " is marked as CanGenerateSamples=false due failure to create NormalDistribution: " + e.Message;
            }
        }

        public double[] GenerateSamples(int howMany)
        {
            var resArray = new double[howMany];

            if (DistributionFunction == null)
                return new double[howMany];

            var res = DistributionFunction.Generate(howMany, resArray);
            return res;
        }

        private void SetDistributionFunctionFromString()
        {
            if (DistributionfunctionString.StartsWith("N"))
            {
                DistributionFunction = new NormalDistribution(Mean, StDeviation);
            }
            else if (DistributionfunctionString.StartsWith("Poisson"))
            {
                DistributionfunctionString = EnsureOnlyOneComma(DistributionfunctionString, 1);

                var lambda = double.Parse((DistributionfunctionString.Split('=')[1]).Replace(")", "").Trim(), CultureInfo.InvariantCulture);
                DistributionFunction = new PoissonDistribution(lambda);
            }
            else if (DistributionfunctionString.StartsWith("U"))
            {
                DistributionfunctionString = EnsureOnlyOneComma(DistributionfunctionString, 2);

                var a = double.Parse((DistributionfunctionString.Split('=')[1]).Split(',')[0].Trim(), CultureInfo.InvariantCulture);
                var b = double.Parse((DistributionfunctionString.Split('=')[2]).Split(')')[0].Trim(), CultureInfo.InvariantCulture);

                DistributionFunction = new UniformContinuousDistribution(a, b);
            }
            else if (DistributionfunctionString.Contains("x; k ="))
            {
                DistributionfunctionString = EnsureOnlyOneComma(DistributionfunctionString, 2);

                var k = double.Parse((DistributionfunctionString.Split('=')[1]).Split(',')[0].Trim(), CultureInfo.InvariantCulture);
                var teta = double.Parse((DistributionfunctionString.Split('=')[2]).Split(')')[0].Trim(), CultureInfo.InvariantCulture);

                DistributionFunction = new GammaDistribution(teta, k);
            }
            else if (DistributionfunctionString.Contains("Gumbel"))
            {
                DistributionfunctionString = EnsureOnlyOneComma(DistributionfunctionString, 2);

                var u = double.Parse((DistributionfunctionString.Split('=')[1]).Split(',')[0].Trim(), CultureInfo.InvariantCulture);
                var b = double.Parse((DistributionfunctionString.Split('=')[2]).Split(')')[0].Trim(), CultureInfo.InvariantCulture);

                DistributionFunction = new GumbelDistribution(u, b);
            }

            else
            {
                throw new Exception("Unregnoized disitrubtionfunction " + DistributionfunctionString);
            }
        }

        private string EnsureOnlyOneComma(string distributionfunctionString, int numberOfArgs)
        {
            if (distributionfunctionString.Split(',').Length == 1 || distributionfunctionString.Split(',').Length == numberOfArgs)
                return distributionfunctionString;

            if (distributionfunctionString.Split(',').Length != numberOfArgs * 2)
                throw new ArgumentException("Cannot parse distributionfunctionString. Expected either one comma (separating for instance a,b values) or three commas where the first and the last is decimal indicators");

            distributionfunctionString = ReplaceFirst(distributionfunctionString, ",", ".");

            if (numberOfArgs > 1)
                distributionfunctionString = ReplaceLast(distributionfunctionString, ",", ".");

            return distributionfunctionString;
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private string ReplaceLast(string text, string search, string replace)
        {
            int pos = text.LastIndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public override string ToString()
        {
            return $"Distribution: {DistributionFunction?.GetType()}, Mean: {Mean}, STD: {StDeviation}";
        }
    }
}
