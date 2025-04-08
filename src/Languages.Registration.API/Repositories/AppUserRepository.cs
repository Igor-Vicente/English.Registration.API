using Languages.Registration.API.Factories;
using Languages.Registration.API.Models;
using Languages.Registration.API.Repositories.Contracts;
using Languages.Registration.API.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Languages.Registration.API.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly IMongoCollection<AppUser> _applicationUsersCollection;

        public AppUserRepository(MongoDbCollectionFactory collectionFactory)
        {
            _applicationUsersCollection = collectionFactory.GetCollection<AppUser>(MongoDbCollectionFactory.APPLICATION_USERS_COLLECTION);
        }

        public async Task AddAsync(AppUser user)
        {
            await _applicationUsersCollection.InsertOneAsync(user);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _applicationUsersCollection.DeleteOneAsync(u => u.Id == id);
        }

        public async Task<AppUser> GetAsync(ObjectId id)
        {
            return await _applicationUsersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<AppUser>> GetAsync(int index, int quantity)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(AppUser user)
        {
            await _applicationUsersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<IEnumerable<AppUser>> GetUsersInRange(Coordinates coordinates, int range)
        {
            var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                   new GeoJson2DGeographicCoordinates(coordinates.Longitude, coordinates.Latitude)
               );

            var filter = Builders<AppUser>.Filter.NearSphere(
                field: x => x.Location,
                point: point,
                maxDistance: range // meters
            );

            return await _applicationUsersCollection.Find(filter).ToListAsync();
        }
    }
}
