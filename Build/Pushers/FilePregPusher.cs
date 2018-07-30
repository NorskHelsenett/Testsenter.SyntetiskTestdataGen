using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public class FilePregPusher : QueuePregPusher
    {
        private readonly StreamWriter _sw;
        private readonly FileStream _fs;
        public string Filepath { get; set; }

        public FilePregPusher(string filepath)
        {
            Filepath = filepath;
            _fs = File.Open(filepath, FileMode.CreateNew);
            _sw = new StreamWriter(_fs);
        }

        public override Task Save()
        {
            return Task.Run(async () =>
            {
                int counter = 0;
                while (!_queue.IsCompleted)
                {
                    Person item;
                    if (!_queue.TryTake(out item, TimeSpan.FromMilliseconds(10)))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(200));
                    }
                    else
                    {
                        await SaveToDb(item);
                    }

                    if (counter++ % 1000 == 0)
                        Outputter.WriteLine("Currentqueuesize is " + _queue.Count);
                }
            });
        }

        public override void DisposeDb()
        {
            Outputter.WriteLine("Written file to " + Filepath);
            try
            {
                _sw.Close();
                _fs.Close();
            }
            catch (Exception e)
            {
                Outputter.WriteLine(e.Message);
            }
        }

        private async Task<bool> SaveToDb(Person person)
        {
            var json = JsonConvert.SerializeObject(person);
            await _sw.WriteLineAsync(json);

            return true;
        }
    }
}
