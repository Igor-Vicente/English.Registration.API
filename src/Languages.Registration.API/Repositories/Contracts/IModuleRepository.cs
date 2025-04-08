using Languages.Registration.API.Models;

namespace Languages.Registration.API.Repositories.Contracts
{
    public interface IModuleRepository
    {
        Task AddAsync(Module module);
        Task<IEnumerable<Module>> GetAsync();
    }
}
