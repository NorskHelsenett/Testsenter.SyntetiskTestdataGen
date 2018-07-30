using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Shared.DataProviders
{
    public class LocalFile : IPregDataProvider
    {
        private readonly StreamReader _file;

        public LocalFile(string filePath)
        {
            _file = new StreamReader(filePath);
        }

        public async Task<Person> GetNextPerson()
        {
            if (!HasMore())
                return null;

            var nextLine = await _file.ReadLineAsync();
            try
            {
                return JsonConvert.DeserializeObject<Person>(nextLine);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool HasMore()
        {
            return !_file.EndOfStream;
        }
    }
}
