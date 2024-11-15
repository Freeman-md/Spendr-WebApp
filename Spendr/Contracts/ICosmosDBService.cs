using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Spendr.Contracts
{
    public interface ICosmosDBService
    {
        Task<Database> GetOrCreateDatabaseAsync();
        Task<Container> GetOrCreateContainerAsync(string containerName, string partitionKeyPath);
    }
}
