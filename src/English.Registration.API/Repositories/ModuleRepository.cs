using English.Registration.API.Factories;
using English.Registration.API.Models;
using English.Registration.API.Repositories.Contracts;
using MongoDB.Driver;

namespace English.Registration.API.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IMongoCollection<Module> _modulesCollection;

        public ModuleRepository(MongoDbCollectionFactory collectionFactory)
        {
            _modulesCollection = collectionFactory.GetCollection<Module>(MongoDbCollectionFactory.MODULES_COLLECTION);
        }

        public async Task AddAsync(Module module)
        {
            await _modulesCollection.InsertOneAsync(module);
        }

        public async Task<IEnumerable<Module>> GetAsync()
        {
            return await _modulesCollection.Find(x => true).ToListAsync();
        }
    }
}
