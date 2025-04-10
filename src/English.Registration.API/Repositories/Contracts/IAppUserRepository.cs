using English.Registration.API.Models;
using English.Registration.API.ViewModels;
using MongoDB.Bson;

namespace English.Registration.API.Repositories.Contracts
{
    public interface IAppUserRepository
    {
        Task AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(ObjectId id);
        Task<AppUser> GetAsync(ObjectId id);
        Task<IEnumerable<AppUser>> GetAsync(int index, int quantity);
        Task<IEnumerable<AppUser>> GetUsersInRange(Coordinates coordinates, int range);
    }
}
