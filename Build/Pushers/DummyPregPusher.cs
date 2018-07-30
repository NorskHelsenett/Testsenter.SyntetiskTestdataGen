using System;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public class DummyPregPusher : QueuePregPusher
    {
        public override Task Save()
        {
            return Task.Run(async () =>
            {
                while (!_queue.IsCompleted)
                {
                    Person item;
                    if (!_queue.TryTake(out item, TimeSpan.FromMilliseconds(10)))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(200));
                    }
                }
            });
        }

        public override void DisposeDb()
        {
        }
    }
}
