using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Build.Pushers
{
    public abstract class QueuePregPusher : IPushPregData
    {
        public static bool UnexceptedQuit { get; set; }
        protected BlockingCollection<Person> _queue;
        private int _addcount;
        private int _removedcount;

        protected QueuePregPusher()
        {
            _queue = new BlockingCollection<Person>();
            UnexceptedQuit = false;
        }

        public void DonePushing()
        {
            _queue.CompleteAdding();
        }


        public abstract Task Save();

        public void AddToSaveQueue(IEnumerable<Person> set)
        {
            foreach (var obj in set)
            {
                while (!_queue.TryAdd(obj, TimeSpan.FromMilliseconds(10)))
                {
                    Console.WriteLine("Bin is full, retrying...");
                }

                Interlocked.Increment(ref _addcount);
            }
        }

        protected void Consumed() => Interlocked.Increment(ref _removedcount);

        public abstract void DisposeDb();

        public bool IsDone()
        {
            return _queue.IsCompleted;
        }

        public int QueueLength()
        {
            return _queue.Count;
        }

        public int NumberOfCreated()
        {
            return _addcount;
        }

        public int NumberOfWritten()
        {
            return _removedcount;
        }

        public void Dispose()
        {
            DisposeDb();
            _queue = null;
        }
    }
}
