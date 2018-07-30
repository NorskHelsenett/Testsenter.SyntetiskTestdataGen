using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace Correlation
{
    public class Program
    {
        /// <summary>
        /// Generates a matrix for projecting new boolean values from a Multivariate Normal Distribution
        /// according to paper: http://epub.wu.ac.at/286/1/document.pdf
        /// Must install R from https://www.r-project.org/
        /// Must install external packages:
        ///     - install.packages('bindata', dependencies=TRUE)
        ///     - install.packages('mvtnorm', dependencies=TRUE)
        ///     - install.packages('Matrix', dependencies=TRUE)
        /// 
        /// This matrix is set on the model, and only needs to be generated once; i.e., once 
        /// the matrix is generated, unlimited builds (BuildPreg) may be executed
        /// </summary>
        /// <param name="args"></param>

        public static void Main(string[] args)
        {
            var filepathToModel = @"C:\temp\pregmodel_62e2a398-6276-4a96-bd96-10cc66a46fba.json";

            var filepathWithoutJson = filepathToModel.Replace(".json", "");
            var model = JsonConvert.DeserializeObject<SynteticModel>(File.ReadAllText(filepathToModel));

            RIntegration.StartEngine();

            try
            {
                var corMatrix = model.Statistics.Correlations;
                SetSigma(model.Statistics, corMatrix);

                var diffieDic = new Dictionary<string, double>();
                foreach (var ageQuant in model.Statistics.StatisticsByAgeQuants.Values)
                {
                    Console.WriteLine();
                    Console.WriteLine("Age quant: " + ageQuant.AgeQuantLevel);
                    var allBooleans = model.Statistics.BinaryGenerator.SelectApplicableStatistics(BooleanStatistic.GetAll(ageQuant));
                    model.Statistics.BinaryGenerator.Verify(allBooleans, corMatrix, meanConfidence: 0.1, correlationConfidence: 0.15, onlyWarn: true, diffie: diffieDic);
                    Console.WriteLine("-------------------------------");
                }
            }
            finally
            {
                RIntegration.StopEngine();
            }

            Console.WriteLine("Done. Writing to file");
            var filename = @filepathWithoutJson + @"_withsigma.json";
            var asJson = JsonConvert.SerializeObject(model);

            File.WriteAllText(filename, asJson);
            Console.WriteLine("Written model to file " + filename);

            Console.WriteLine();
            Console.WriteLine("Click any button to exit");
            Console.ReadKey();
        }

        private static void Merge(string target, string takeSigmaFromThisOne)
        {
            var filepathWithoutJson = target.Replace(".json", "");

            var modelTakeSigmaFromThisOne = JsonConvert.DeserializeObject<SynteticModel>(File.ReadAllText(takeSigmaFromThisOne));
            var targetModel = JsonConvert.DeserializeObject<SynteticModel>(File.ReadAllText(target));

            targetModel.Statistics.BinaryGenerator = modelTakeSigmaFromThisOne.Statistics.BinaryGenerator;

            Console.WriteLine("Done. Writing to file");
            var filename = @filepathWithoutJson + @"_withsigma.json";
            var asJson = JsonConvert.SerializeObject(targetModel);

            File.WriteAllText(filename, asJson);
        }

        private static void SetSigma(PersonStatistics personStats, CorrelationMatrix correlationMatrix)
        {
            var allBooleans = BooleanStatistic.GetAll(personStats);
            allBooleans = ExcludeNonvariantStatistcs(allBooleans);

            var sigma = new MultivariateBinaryGenerator();
            sigma.BuildCoverianceMatrix(allBooleans, correlationMatrix, RIntegration.GetSigma);

            personStats.BinaryGenerator = sigma;
        }

        private static List<BooleanStatistic> ExcludeNonvariantStatistcs(List<BooleanStatistic> all)
        {
            return all.Where(b => b.NumberOfTrues != 0 && b.NumberOfFalse != 0).ToList();
        }
    }
}
