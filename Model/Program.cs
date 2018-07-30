using System;
using System.IO;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.Models;

namespace SyntetiskTestdataGen.Model
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var dataProvider = GetDataProvider();

                var builder = new SynteticModelBuilder(dataProvider);

                var sessionId = Guid.NewGuid().ToString();
                Console.WriteLine("SessionId: " + sessionId);

                SynteticModel model = null;
                try
                {
                    model = builder.BuildModel(sessionId).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine("In program:" + e.ToString());
                    Console.ReadKey();
                }

                var filename = @"C:\temp\pregmodel_" + sessionId + @"_withoutcorr.json";
                File.WriteAllText(filename, JsonConvert.SerializeObject(model));
                Console.WriteLine("Temporarily written model withouth correlations to file " + filename);

                Console.WriteLine("Now calculating correlations");
                builder.SetNewDataProvder(GetDataProvider());
                builder.BuildCorrelationMatrix(model, sessionId).GetAwaiter().GetResult();

                filename = @"C:\temp\pregmodel_" + sessionId + @".json";
                var asJson = JsonConvert.SerializeObject(model);

                File.WriteAllText(filename, asJson);
                Console.WriteLine("Written model to file " + filename);

                Console.WriteLine();
                Console.WriteLine("Click any button to exit");
                Console.ReadKey();
            }

            catch (Exception e)
            {
                Console.WriteLine("In program:" + e.ToString());
                Console.ReadKey();
                throw;
            }
        }

        private static IPregDataProvider GetDataProvider()
        {
            return new LocalFile(@"..\..\synteticpregdb.json");
        }
    }
}
