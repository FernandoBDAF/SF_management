using System.Runtime.Serialization;

namespace SFManagement.Domain.Exceptions;

/// <summary>
/// Base exception for business logic errors
/// </summary>
[Serializable]
public class BusinessException : Exception
{
    public string Code { get; }
    public new object? Data { get; }

    public BusinessException(string message, string code = "BUSINESS_ERROR", object? data = null) 
        : base(message)
    {
        Code = code;
        Data = data;
    }

    public BusinessException(string message, Exception innerException, string code = "BUSINESS_ERROR", object? data = null) 
        : base(message, innerException)
    {
        Code = code;
        Data = data;
    }

    protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Code = info.GetString(nameof(Code)) ?? "BUSINESS_ERROR";
        Data = info.GetValue(nameof(Data), typeof(object));
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Code), Code);
        info.AddValue(nameof(Data), Data);
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
[Serializable]
public class ValidationException : BusinessException
{
    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(string message, List<ValidationError> validationErrors) 
        : base(message, "VALIDATION_ERROR", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(List<ValidationError> validationErrors) 
        : base("Validation failed", "VALIDATION_ERROR", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ValidationErrors = (List<ValidationError>)info.GetValue(nameof(ValidationErrors), typeof(List<ValidationError>))!;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ValidationErrors), ValidationErrors);
    }
}

/// <summary>
/// Exception for entity not found errors
/// </summary>
[Serializable]
public class EntityNotFoundException : BusinessException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId) 
        : base($"{entityType} with ID {entityId} was not found", "ENTITY_NOT_FOUND", new { EntityType = entityType, EntityId = entityId })
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        EntityType = info.GetString(nameof(EntityType)) ?? "";
        EntityId = info.GetValue(nameof(EntityId), typeof(object))!;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EntityType), EntityType);
        info.AddValue(nameof(EntityId), EntityId);
    }
}

/// <summary>
/// Exception for duplicate entity errors
/// </summary>
[Serializable]
public class DuplicateEntityException : BusinessException
{
    public string EntityType { get; }
    public string Field { get; }
    public object Value { get; }

    public DuplicateEntityException(string entityType, string field, object value) 
        : base($"{entityType} with {field} '{value}' already exists", "DUPLICATE_ENTITY", new { EntityType = entityType, Field = field, Value = value })
    {
        EntityType = entityType;
        Field = field;
        Value = value;
    }

    protected DuplicateEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        EntityType = info.GetString(nameof(EntityType)) ?? "";
        Field = info.GetString(nameof(Field)) ?? "";
        Value = info.GetValue(nameof(Value), typeof(object))!;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EntityType), EntityType);
        info.AddValue(nameof(Field), Field);
        info.AddValue(nameof(Value), Value);
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
[Serializable]
public class BusinessRuleException : BusinessException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message) 
        : base(message, "BUSINESS_RULE_VIOLATION", new { RuleName = ruleName })
    {
        RuleName = ruleName;
    }

    protected BusinessRuleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        RuleName = info.GetString(nameof(RuleName)) ?? "";
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(RuleName), RuleName);
    }
}

/// <summary>
/// Validation error details
/// </summary>
[Serializable]
public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string? Code { get; set; }

    public ValidationError(string field, string message, string? code = null)
    {
        Field = field;
        Message = message;
        Code = code;
    }
} 