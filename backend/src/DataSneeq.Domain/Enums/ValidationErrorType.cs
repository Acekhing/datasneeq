namespace DataSneeq.Domain.Enums;

public enum ValidationErrorType
{
    RequiredField,
    InvalidDataType,
    InvalidNumber,
    InvalidDate,
    ForeignKeyNotResolvable,
    DuplicateRow,
    ValueTooLong,
    Unknown
}
