using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DbEntities;

namespace SyntetiskTestdataGen.Shared.DataProviders
{
    public interface IPregDataProvider
    {
        Task<Person> GetNextPerson();
        bool HasMore();
    }
}