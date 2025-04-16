using System.Collections.Immutable;

namespace Ghanavats.Domain.Factory.Abstractions.Responses;

public class DomainFactoryResponseModel<TResponse>
{
    public TResponse? Value { get; private init; }
    public ImmutableArray<string> Cache { get; init; } = [];
    public string ErrorMessage { get; private init; } = string.Empty;
    
    public static DomainFactoryResponseModel<TResponse> Success(TResponse value, IEnumerable<string> cache)
        => new()
        {
            Value = value,
            Cache = [..cache],
            ErrorMessage = string.Empty
        };

    public static DomainFactoryResponseModel<TResponse> Failure(string errorMessage)
        => new()
        {
            Value = default,
            Cache = [],
            ErrorMessage = errorMessage
        };
}
