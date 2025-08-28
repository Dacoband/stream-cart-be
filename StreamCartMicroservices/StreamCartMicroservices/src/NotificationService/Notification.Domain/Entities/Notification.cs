using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Domain.Entities
{
    public class Notifications 

    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public string RecipientUserID { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string? OrderCode { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? ProductId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? VariantId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? LivestreamId { get; set; }

        [BsonElement("Type")]
        public string? Type { get; set; }

        public string Message { get; set; }

        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; protected set; } =DateTime.UtcNow;
        public string CreatedBy { get; protected set; } = string.Empty;
        public DateTime? LastModifiedAt { get; protected set; }
        public string? LastModifiedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }
        

        /// <summary>
        /// Sets the creator of the entity. Will only set if not already set.
        /// </summary>
        /// <param name="creator">The name/identifier of the creator</param>
        /// <exception cref="ArgumentNullException">Thrown when creator is null</exception>
        public void SetCreator(string creator)
        {
            if (creator == null)
                throw new ArgumentNullException(nameof(creator), "Creator cannot be null");

            if (string.IsNullOrWhiteSpace(creator))
                throw new ArgumentException("Creator cannot be empty", nameof(creator));

            if (string.IsNullOrWhiteSpace(CreatedBy))
            {
                CreatedBy = creator;
            }
        }

        /// <summary>
        /// Sets the modifier of the entity and updates the modified timestamp.
        /// </summary>
        /// <param name="modifier">The name/identifier of the modifier</param>
        /// <exception cref="ArgumentNullException">Thrown when modifier is null</exception>
        public void SetModifier(string modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier), "Modifier cannot be null");

            if (string.IsNullOrWhiteSpace(modifier))
                throw new ArgumentException("Modifier cannot be empty", nameof(modifier));

            LastModifiedBy = modifier;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the entity as deleted (soft delete).
        /// </summary>
        /// <param name="modifier">Optional. The name/identifier of who performed the delete operation</param>
        public void Delete(string? modifier = null)
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                LastModifiedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(modifier))
                {
                    LastModifiedBy = modifier;
                }
            }
        }

        /// <summary>
        /// Restores a previously deleted entity.
        /// </summary>
        /// <param name="modifier">Optional. The name/identifier of who performed the restore operation</param>
        public void Restore(string? modifier = null)
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                LastModifiedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(modifier))
                {
                    LastModifiedBy = modifier;
                }
            }
        }

        /// <summary>
        /// Validates that the entity is in a valid state.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            // Base validation rules
            if (Id == Guid.Empty)
                return false;

            if (CreatedAt == default)
                return false;

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

     
    }
}
