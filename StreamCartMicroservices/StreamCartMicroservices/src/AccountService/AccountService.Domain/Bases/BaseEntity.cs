using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Bases
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public string CreatedBy { get; protected set; } = string.Empty;
        public DateTime? LastModifiedAt { get; protected set; }
        public string? LastModifiedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        protected BaseEntity(Guid id) 
        {
            Id = id != Guid.Empty 
                ? id 
                : throw new ArgumentException("Entity ID cannot be empty", nameof(id));
                
            CreatedAt = DateTime.UtcNow;
        }

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

        public static bool operator ==(BaseEntity? left, BaseEntity? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(BaseEntity? left, BaseEntity? right)
        {
            return !(left == right);
        }
    }
}
