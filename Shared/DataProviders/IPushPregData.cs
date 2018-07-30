using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Shared.DataProviders
{
    public interface IPushPregData : IDisposable
    {
        void AddToSaveQueue(IEnumerable<Person> set);
        bool IsDone();
        int QueueLength();
        int NumberOfCreated();
        int NumberOfWritten();
        Task Save();
        void DonePushing();
        void DisposeDb();
    }
}
