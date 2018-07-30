using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Distributions.Univariate;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public class MultivariateBinaryGenerator
    {
        public double[,] CovarianceMatrix { get; set; }
        public Dictionary<string, int> NameToColumnIndex { get; set; }
        public Dictionary<int, string> ColumnIndexToName { get; set; }

        public void BuildCoverianceMatrix(List<BooleanStatistic> statistics, CorrelationMatrix correlationMatrix, Func<double[,], double[,]> getSigma)
        {
            var taken = new HashSet<string>();
            NameToColumnIndex = new Dictionary<string, int>();
            ColumnIndexToName = new Dictionary<int, string>();

            if (statistics.Any(s => string.IsNullOrEmpty(s.Name)))
                throw new ArgumentException("All statistics.name must be non-null");

            var commonprob = new double[statistics.Count, statistics.Count];

            int outerIndex = -1;
            int innerIndex = -1;
            foreach (var outerStat in statistics)
            {
                outerIndex++;
                NameToColumnIndex.Add(outerStat.Name, outerIndex);
                ColumnIndexToName.Add(outerIndex, outerStat.Name);

                foreach (var innerStat in statistics)
                {
                    innerIndex++;
                    var key = GetKey(outerIndex, innerIndex);
                    if(taken.Contains(key) || taken.Contains(GetKey(innerIndex, outerIndex)))
                        continue;
                    taken.Add(key);

                    if (outerIndex == innerIndex)
                    {
                        if(outerStat.Name != innerStat.Name)
                            throw new ArgumentException("Bug..");

                        commonprob[outerIndex, innerIndex] = outerStat.TrueRatio;
                        continue;
                    }

                    var correlation = correlationMatrix.GetValue(outerStat.Name, innerStat.Name);
                    if(Double.IsNaN(correlation.Value))
                        throw new ArgumentException("Correlation cannot be 0");

                    if (correlation.Value > 1.0)
                        correlation.Value = 1.0;

                    var value = GetPaGivenB(outerStat, innerStat, correlation.Value);
                    commonprob[outerIndex, innerIndex] = value;
                    commonprob[innerIndex, outerIndex] = value;
                }

                innerIndex = -1;
            }

            CovarianceMatrix = getSigma(commonprob);
        }

        public List<BooleanStatistic> SelectApplicableStatistics(List<BooleanStatistic> list)
        {
            return list.Where(l => NameToColumnIndex.ContainsKey(l.Name)).ToList();
        }

        public Dictionary<string, bool> NextRow(PersonStatistics personStatistics)
        {
            var samples = GetSamples(1, BooleanStatistic.GetAll(personStatistics));
            var result = new Dictionary<string, bool>();

            for (int i = 0; i < samples[0].Length; i++)
            {
                result.Add(ColumnIndexToName[i], samples[0][i] > 0.99);
            }

            return result;
        }

        public static bool Hit(string key, Dictionary<string, bool> values, bool backup)
        {
            if (!values.ContainsKey(key))
                return backup;

            return values[key];
        }

        public static string Sample(string key, Dictionary<string, bool> values, DiscreteStringStatistics stats, Randomizer randy)
        {
            if (!values.ContainsKey(key))
                return stats.Sample(randy);

            var hasValue = values[key];
            if (!hasValue)
                return null;

            return stats.NonNullSample(randy);
        }

        public static int? Sample(string key, Dictionary<string, bool> values, DiscreteStatistic stats, Randomizer randy)
        {
            if (!values.ContainsKey(key))
                return stats.Sample(randy);

            var hasValue = values[key];
            if (!hasValue)
                return null;

            return stats.NonNullSample(randy);
        }

        public void Verify(List<BooleanStatistic> statistics, CorrelationMatrix correlationMatrix, double meanConfidence = 0.1, double correlationConfidence = 0.1, bool onlyWarn = false, Dictionary<string, double> diffie = null)
        {
            var samples = GetSamples(1000, statistics);

            var taken = new HashSet<string>();
            foreach (var outerStat in statistics)
            {
                if(!NameToColumnIndex.ContainsKey(outerStat.Name))
                    continue;

                var samplesForOuterStat = GetValue(NameToColumnIndex[outerStat.Name], samples);
                var meanForOuterStat = TrueRatio(samplesForOuterStat);
                ThrowIfExceedsConfidence(meanForOuterStat, outerStat.TrueRatio, meanConfidence, "Mean for " + outerStat.Name, onlyWarn);

                foreach (var innerStat in statistics)
                {
                    var combo = innerStat.Name + outerStat.Name;
                    if (!NameToColumnIndex.ContainsKey(innerStat.Name))
                        continue;

                    if (taken.Contains(combo))
                        continue;

                    if (innerStat.Name == outerStat.Name)
                        continue;

                    if(outerStat.TrueRatio == 100 || outerStat.TrueRatio == 0 || innerStat.TrueRatio == 100 || innerStat.TrueRatio == 0)
                        continue;

                    var samplesForInnerStat = GetValue(NameToColumnIndex[innerStat.Name], samples);
                    var cor = StatisticHelper.CalculateCorrelation(samplesForOuterStat, samplesForInnerStat);
                    var orgCor = correlationMatrix.GetValue(outerStat.Name, innerStat.Name).Value;

                    var realDiff = ThrowIfExceedsConfidence(cor, orgCor, correlationConfidence, "Correlation between " + innerStat.Name + " and " + outerStat.Name, onlyWarn);

                    if (diffie != null)
                    {
                        if (!diffie.ContainsKey(combo))
                            diffie.Add(combo, 0);

                        diffie[combo] += realDiff;
                    }

                    taken.Add(outerStat.Name + innerStat.Name);
                }
            }
        }

        #region Helpers

        private double[][] GetSamples(int howMany, List<BooleanStatistic> statistics)
        {
            var disitrubtion = new NormalDistribution(0, Math.Sqrt(1));
            var mean = new double[CovarianceMatrix.GetLength(0)];
            for (int i = 0; i < CovarianceMatrix.GetLength(0); i++)
            {
                mean[i] = disitrubtion.InverseDistributionFunction(GetMean(i, statistics));
            }

            var mnormaldist = new MultivariateNormalDistribution(mean, CovarianceMatrix);
            var samples = mnormaldist.Generate(howMany);
            samples = Booleanize(samples);

            return samples;
        }

        private string GetKey(int a, int b)
        {
            return a + "_" + b;
        }

        private double ThrowIfExceedsConfidence(double valuea, double valueb, double confidence, string prefix, bool onlyWarn)
        {
            var diff = Math.Abs(valuea - valueb);
            if (diff > confidence)
            {
                var str = prefix + " differs " + (valuea - valueb) + " while max was " + confidence;
                if (onlyWarn)
                    Console.WriteLine("WARNING: " + str);
                else
                     throw new Exception();
            }

            return valuea - valueb;
        }

        private double TrueRatio(double[] a)
        {
            return (double) a.Count(x => x == 1) / a.Count();
        }

        private double[] GetValue(int index, double[][] samples)
        {
            var result = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
                result[i] = samples[i][index];

            return result;
        }

        private double[][] Booleanize(double[][] samples)
        {
            for (int i = 0; i <samples.Length; i++)
            {
                for (int j = 0; j < samples[0].Length; j++)
                {
                    samples[i][j] = samples[i][j] > 0 ? 1 : 0;
                }
            }

            return samples;
        }

        private double GetMean(int index, List<BooleanStatistic> statistics)
        {
            var name = ColumnIndexToName[index];
            var stat = statistics.Single(t => t.Name == name);
            return stat.TrueRatio;
        }

        private double GetPaGivenB(BooleanStatistic outerStat, BooleanStatistic innerStat, double correlation)
        {
            var paGivenB = Double.IsNaN(correlation) ? outerStat.TrueRatio :
                GetPab(outerStat.TrueRatio, innerStat.TrueRatio, outerStat.GetStandardDeviation(), innerStat.GetStandardDeviation(), correlation);

            if (paGivenB < 0 || paGivenB > 1)
                throw new Exception();

            return paGivenB;
        }

        private double GetPab(double pa, double pb, double stda, double stdb, double cor)
        {
            var covariance = cor * stda * stdb;
            var pAandB = covariance + pa * pb;

            var finalValue = pAandB;

            if (finalValue < 0)
                finalValue = Math.Abs(finalValue);

            return finalValue > Math.Min(pa, pb) ? Math.Min(pa, pb) : finalValue;
        }

        
        #endregion
    }
}
