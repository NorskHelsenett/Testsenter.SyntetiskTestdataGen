using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Distributions.Univariate;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public static class StatisticHelper
    {
        private static void TestChol()
        {
            var a = new double[5, 5]
            {
                {1, -1, -1, -1, -1},
                {-1, 2, 0, 0, 0},
                {-1, 0, 3, 1, 1},
                {-1, 0, 1, 4, 2},
                {-1, 0, 1, 2, 5}
            };

            var chol = new CholeskyDecomposition(a);

            var c = chol.LeftTriangularFactor;
            var cmerket = c.Transpose();

            var aa = c.Dot(cmerket);
        }

        public static void GenerateValues2(int howMany)
        {
            var disitrubtion = new NormalDistribution(0, Math.Sqrt(1));
            var randomValues = disitrubtion.Generate(howMany * 2);

            var meanA = 0.7;
            var meanB = 0.2;
            var sigma = new double[2, 2];
            sigma[0, 0] = 1;
            sigma[0, 1] = 0.7;
            sigma[1, 0] = 0.7;
            sigma[1, 1] = 1;

            var randomValuesMatrix = new double[howMany, 2];
            var verticalIndex = 0;
            for (int i = 0; i < howMany; i = i + 2)
            {
                randomValuesMatrix[verticalIndex, 0] = randomValues.ElementAt(i) - meanA;
                randomValuesMatrix[verticalIndex, 1] = randomValues.ElementAt(i + 1)- meanB;

                randomValuesMatrix[verticalIndex, 0] = randomValuesMatrix[verticalIndex, 0] > 0 ? 1 : 0;
                randomValuesMatrix[verticalIndex, 1] = randomValuesMatrix[verticalIndex, 1] > 0 ? 1 : 0;

                verticalIndex++;
            }

            var randomValuesMatrixCov = randomValuesMatrix.Covariance(); //new [] {meanA, meanB}
            var cholCovX = new CholeskyDecomposition(randomValuesMatrixCov).LeftTriangularFactor.Transpose();
            var invCholCovX = cholCovX.Inverse();

            var dottedInverse = randomValuesMatrix.Dot(invCholCovX);
            var result = dottedInverse.Dot(new CholeskyDecomposition(sigma).LeftTriangularFactor.Transpose());

            var resultSigma = result.Covariance();
            verticalIndex = 0;
            for (int i = 0; i < howMany; i++)
            {
                result[verticalIndex, 0] = result[verticalIndex, 0] > 0 ? 1 : 0;
                result[verticalIndex, 1] = result[verticalIndex, 1] > 0 ? 1 : 0;

                verticalIndex++;
            }

            var booleansigma = result.Covariance();

            int randomValueCount = 0;

            //for (int i = 0; i < howMany; i++)
            //{
            //    //generating one sample
            //    var z = new[] { randomValues[randomValueCount++]-meanA, randomValues[randomValueCount++]-meanB };
            //    var product = z.Dot(R);
            //    var y = mean.Add(product);
            //    var samples = new double[] { y[0] > 0 ? 1 : 0, y[1] > 0 ? 1 : 0 };

            //    aSamples[i] = samples[0];
            //    bSamples[i] = samples[1];

            //    Trace.WriteLine($"{aSamples[i]}, {bSamples[i]}");
            //}
        }

        public static void GenerateValues2(int howMany, double meanA, double meanB, double meanC, double[,] R)
        {
            var disitrubtion = new NormalDistribution(0, Math.Sqrt(1));
            var mean = new double[] { disitrubtion.InverseDistributionFunction(meanA), disitrubtion.InverseDistributionFunction(meanB), disitrubtion.InverseDistributionFunction(meanC) };

            var mnormaldist = new MultivariateNormalDistribution(mean, R);
            var samples = mnormaldist.Generate(howMany);

            var aSamples = new double[howMany];
            var bSamples = new double[howMany];
            var cSamples = new double[howMany];

            for (int i = 0; i < samples.Length; i++)
            {
                aSamples[i] = samples[i][0] > 0 ? 1 : 0;
                bSamples[i] = samples[i][1] > 0 ? 1 : 0;
                cSamples[i] = samples[i][2] > 0 ? 1 : 0;
            }

            var resultCorrAb = CalculateCorrelation(aSamples, bSamples);
            var resultCorrAc = CalculateCorrelation(aSamples, cSamples);
            var resultCorrBc = CalculateCorrelation(bSamples, cSamples);
        }

        public static void GenerateValues(int howMany, double meanA, double meanB, double meanC, double[,] R)
        {
            TestChol();
            var disitrubtion = new NormalDistribution(0, Math.Sqrt(1));

            var mean = new double[] { disitrubtion.InverseDistributionFunction(meanA), disitrubtion.InverseDistributionFunction(meanB), disitrubtion.InverseDistributionFunction(meanC) };

            var randomValues = disitrubtion.Generate(howMany * 3);

            var aSamples = new double[howMany];
            var bSamples = new double[howMany];
            var cSamples = new double[howMany];

            int randomValueCount = 0;

            for (int i = 0; i < howMany; i++)
            {
                //generating one sample
                var z = new[] { randomValues[randomValueCount++], randomValues[randomValueCount++], randomValues[randomValueCount++] };
                var product = z.Dot(R);
                var y = mean.Add(product);
                var samples = new double[] { y[0] > 0 ? 1 : 0, y[1] > 0 ? 1 : 0, y[2] > 0 ? 1 : 0 };

                aSamples[i] = samples[0];
                bSamples[i] = samples[1];
                cSamples[i] = samples[2];

                Trace.WriteLine($"{aSamples[i]}, {bSamples[i]}, {cSamples[i]}");
            }

            var resultCorrAb = CalculateCorrelation(aSamples, bSamples);
            var resultCorrAc = CalculateCorrelation(aSamples, cSamples);
            var resultCorrBc = CalculateCorrelation(bSamples, cSamples);
        }

        public static double CalculateCovariance(double[] set1, double[] set2, bool regulate = true)
        {
            var x = NormalDistribution.Estimate(set1, new NormalOptions() { Regularization = regulate ? 1 : 0 });
            var y = NormalDistribution.Estimate(set2, new NormalOptions() { Regularization = regulate ? 1 : 0 });

            double totalSum = 0;
            for (int i = 0; i < set1.Length; i++)
                totalSum += (set1[i] - x.Mean) * (set2[i] - y.Mean);

            totalSum /= (set1.Length - 1);

            return totalSum;
        }

        public static double CalculateCorrelation(double[] set1, double[] set2, bool regulate = true)
        {
            var x = NormalDistribution.Estimate(set1, new NormalOptions() { Regularization = regulate ? 1 : 0 });
            var y = NormalDistribution.Estimate(set2, new NormalOptions() { Regularization = regulate ? 1 : 0 });

            double totalSum = 0;
            for (int i = 0; i < set1.Length; i++)
                totalSum += (set1[i] - x.Mean) * (set2[i] - y.Mean);

            totalSum /= (set1.Length - 1);

            var corr = totalSum / (x.StandardDeviation * y.StandardDeviation);
            return corr;
        }

        public static double? GetValue(this string v)
        {
            if(string.IsNullOrEmpty(v))
                return null;

            double n;
            bool isNumeric = double.TryParse(v, out n);
            if (isNumeric)
                return n;

            try
            {
                int maxDoubleSize = 20;
                string doubleRep = "";
                var asCharArray = v.ToCharArray();
                int index = 0;
                while (doubleRep.Length <= maxDoubleSize && index < asCharArray.Length)
                {
                    var inty = (int) asCharArray[index++];
                    doubleRep += inty;
                }

                return double.Parse(doubleRep.Substring(0, Math.Min(doubleRep.Length, 20)), CultureInfo.InvariantCulture);
            }
            catch { }

            return -100000000;
        }

        public static double? GetValue(this DateTime? v)
        {
            if (!v.HasValue)
                return null;

            var toString = v.Value.ToString("yyyy");

            return double.Parse(toString, CultureInfo.InvariantCulture);
        }

        public static double? GetValue(this int? v)
        {
            if (!v.HasValue)
                return null;

            return (double)v.Value;
        }

        public static double? GetValue(this int v)
        {
            return (double)v;
        }

        public static double? GetValue(this bool v)
        {
            return v ? 1 : 0;
        }
    }
}
