namespace Ghanavats.Domain.Factory.Abstractions.Responses;

public class DomainFactoryResponseModel<TResponse>
{
    public TResponse? Value { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
