using FluentValidation.Results;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace English.Registration.API.Models
{
    public abstract class Entity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonIgnore]
        public ValidationResult ValidationResult { get; protected set; }

        protected Entity(ObjectId? id = null)
        {
            Id = id ?? ObjectId.GenerateNewId();
            ValidationResult = new();
        }

        public abstract bool IsValid();
    }
}
