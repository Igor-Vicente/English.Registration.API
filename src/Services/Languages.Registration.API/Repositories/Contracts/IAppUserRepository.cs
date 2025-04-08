using Languages.Registration.API.Models;
using Languages.Registration.API.ViewModels;
using MongoDB.Bson;

namespace Languages.Registration.API.Repositories.Contracts
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
