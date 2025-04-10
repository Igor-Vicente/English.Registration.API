using English.Registration.API.Models;

namespace English.Registration.API.Repositories.Contracts
{
    public interface IModuleRepository
    {
        Task AddAsync(Module module);
        Task<IEnumerable<Module>> GetAsync();
    }
}
