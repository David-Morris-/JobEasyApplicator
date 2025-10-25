using System.Runtime.Serialization;

namespace Jobs.EasyApply.Infrastructure.Repositories.Exceptions
{
    /// <summary>
    /// Base exception for repository operations
    /// </summary>
    [Serializable]
    public class RepositoryException : Exception
    {
        public RepositoryException() { }

        public RepositoryException(string message) : base(message) { }

        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }

        protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when an entity is not found
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : RepositoryException
    {
        public Type EntityType { get; }
        public object EntityId { get; }

        public EntityNotFoundException() { }

        public EntityNotFoundException(Type entityType, object entityId)
            : base($"Entity of type {entityType.Name} with id {entityId} was not found.")
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityNotFoundException(Type entityType, object entityId, string message)
            : base(message)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityNotFoundException(string message) : base(message) { }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when a duplicate entity is found
    /// </summary>
    [Serializable]
    public class DuplicateEntityException : RepositoryException
    {
        public Type EntityType { get; }
        public object EntityId { get; }

        public DuplicateEntityException() { }

        public DuplicateEntityException(Type entityType, object entityId)
            : base($"Entity of type {entityType.Name} with id {entityId} already exists.")
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public DuplicateEntityException(string message) : base(message) { }

        public DuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }

        protected DuplicateEntityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when repository validation fails
    /// </summary>
    [Serializable]
    public class RepositoryValidationException : RepositoryException
    {
        public string PropertyName { get; }
        public object PropertyValue { get; }

        public RepositoryValidationException() { }

        public RepositoryValidationException(string propertyName, object propertyValue, string message)
            : base(message)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public RepositoryValidationException(string message) : base(message) { }

        public RepositoryValidationException(string message, Exception innerException) : base(message, innerException) { }

        protected RepositoryValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when database concurrency issues occur
    /// </summary>
    [Serializable]
    public class ConcurrencyException : RepositoryException
    {
        public Type EntityType { get; }
        public object EntityId { get; }

        public ConcurrencyException() { }

        public ConcurrencyException(Type entityType, object entityId)
            : base($"Concurrency conflict for entity of type {entityType.Name} with id {entityId}.")
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public ConcurrencyException(string message) : base(message) { }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }

        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
