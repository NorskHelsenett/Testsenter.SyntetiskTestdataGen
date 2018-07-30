using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Build.Pushers;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Build
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var filename = "pregmodel_withsigma.json";
            var generatedFileName = @"C:\temp\createdDb_" + Guid.NewGuid().ToString();

            var config = new PregEngineConfiguration
            {
                AgeQuants = 5,
                HowManyPersonsToConstruct = 10000,
                ModelFilePath = @"..\..\" + filename,
                LogfilePath = generatedFileName + "_" + Guid.NewGuid() + ".txt"
            };

            if(!File.Exists(config.ModelFilePath))
                throw new ArgumentException("Could not find model in path " + config.ModelFilePath);

            var engine = new PregEngine(config);
            var pusher = GetPusher(generatedFileName);
            engine.SetPusher(pusher);

            Outputter.WriteLine("Generating " + config.HowManyPersonsToConstruct + " persons with ageQ: " + config.AgeQuants);

            var stopw = new Stopwatch();
            stopw.Start();
            var howManyCreated = engine.Do(config.HowManyPersonsToConstruct).GetAwaiter().GetResult();
            stopw.Stop();

            Outputter.WriteLine("Generating " + howManyCreated + " took " + stopw.Elapsed.TotalMinutes + " minutes");
            pusher.DisposeDb();

            Console.WriteLine();
            Console.WriteLine("Click any button to exit");
            Console.ReadKey();
        }

        private static IPushPregData GetPusher(string pathToModel)
        {
            return new FilePregPusher(pathToModel);
        }
    }
}
