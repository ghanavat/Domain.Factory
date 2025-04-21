namespace Ghanavats.Domain.Factory.Abstractions.Responses;

public sealed class DomainFactoryResponseModel<TResponse>
{
    public TResponse? Value { get; private init; }
    public object? Cache { get; private init; }
    public string ErrorMessage { get; private init; } = string.Empty;
    
    public static DomainFactoryResponseModel<TResponse> Success(TResponse value, object? cache)
        => new()
        {
            Value = value,
            Cache = cache,
            ErrorMessage = string.Empty
        };

    public static DomainFactoryResponseModel<TResponse> Failure(string errorMessage)
        => new()
        {
            Value = default,
            Cache = null,
            ErrorMessage = errorMessage
        };
}
