# Ghanavats.Domain.Factory

## 💡 Why Ghanavats.Domain.Factory?
In Domain-Driven Design, creating complex object graphs — especially aggregates — 
should not be the responsibility of the entities themselves. According to DDD principles:

> “Complex object creation is a responsibility of the domain layer, yet that task does not belong to the objects that express the model…”

Ghanavats.Domain.Factory helps solve this by:

* Creating entire aggregates as a piece, enforcing their invariants.
* Providing an interface that reflects the goals of the client and hides the complexity of instantiation.
* Making it easier to manage optional and required properties when creating an entity.

## 🧱 How It Works
At its core, Ghanavats.Domain.Factory provides a generic interface through Ghanavats.Domain.Factory.Abstractions:

```csharp
TResponse? CreateEntityObject(TRequest request, [Optional] Action<DomainFactoryOption> action);
```
* TRequest: A simple request object (usually a command or DTO).
* TResponse: A domain entity that inherits from EntityBase.
* The optional DomainFactoryOption lets you fine-tune how the entity is created — such as ignoring or injecting specific properties.

Under the hood, it uses reflection to copy values from the request object to the target domain entity, 
respecting the customisation provided via the options. It is done by scanning an input request object 
and matching its properties to those of the target domain type.

All of this happens without exposing the concrete type or structure of the domain object to the consumer,
in line with Domain-Driven Design principles.

## ⚙️ Performance
Although this library uses .NET reflection to dynamically construct domain entities, 
it is built with performance in mind.

Under the hood, it uses a static, thread-safe cache to eliminate redundant reflection calls:

```csharp
protected static ConcurrentDictionary<string, MethodInfo> CachedMethodInfoCollection { get; set; } = new();
```

This caching mechanism ensures that each method or constructor lookup is performed only once per unique key,
and is reused on subsequent factory calls:

```csharp
var method = CachedMethodInfoCollection.TryGetValue(cacheKey, out var result)
    ? result
    : GetMethod();

if (method is null)
{
    return null;
}

CachedMethodInfoCollection[cacheKey] = method;
```

> ✅ This design minimises the performance impact of reflection and ensures the
> factory remains efficient and scalable.

## 🚀 Usage
### ✅ Basic Usage

First, register the Domain Factory dependency in your application:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomainFactory();
```

Then, a simple usage would be like this:

```csharp
public class CreateSomethingHandler : ICommandHandler<CreateSomethingCommand, SomeResult<int>>
{
    private readonly IDomainFactory<CreateSomethingCommand, EntityClass> _domainFactory;

    public CreateSomethingHandler(IDomainFactory<CreateSomethingCommand, EntityClass> domainFactory)
    {
        _domainFactory = domainFactory.CheckForNull();
    }

    public async Task<Result<int>> Handle(CreateSomethingCommand request, CancellationToken cancellationToken)
    {
        var myEntity = _domainFactory.CreateEntityObject(request);

        // Use your domain entity...
    }
}
```
### ✨ Advanced Usage with Customisation

```csharp
public class CreateSomethingHandler : ICommandHandler<CreateSomethingCommand, SomeResult<int>>
{
    private readonly IDomainFactory<CreateSomethingCommand, EntityClass> _domainFactory;

    public CreateSomethingHandler(IDomainFactory<CreateSomethingCommand, EntityClass> domainFactory)
    {
        _domainFactory = domainFactory.CheckForNull();
    }

    public async Task<Result<int>> Handle(CreateSomethingCommand request, CancellationToken cancellationToken)
    {
        var propertyValue = "ExampleValueForNewProperty";

        var myEntity = _domainFactory.CreateEntityObject(request, option =>
        {
            option.IgnoreProperties([
                nameof(request.SomePropertyOne),
                nameof(request.SomePropertyTwo)
            ]);

            option.AddProperties(new Dictionary<string, object>
            {
                { "SomePropertyThree", propertyValue },
                { "SomePropertyFour", 2025 }
            }.ToImmutableDictionary());
        });

        // Use your customised domain entity...
    }
}
```

## 🔗 Related Packages
### [Ghanavats.Domain.Factory.Abstractions](https://www.nuget.org/packages/Ghanavats.Domain.Factory)
Contains the IDomainFactory interface and configuration types like DomainFactoryOption.

> Use this package if you want to define your own factory implementation or work against abstractions only.

### [Ghanavats.Domain.Primitives](https://www.nuget.org/packages/Ghanavats.Domain.Primitives)
Provides core DDD primitives like EntityBase, as well as helpful attributes like [AggregateRoot].

> This is a transitive dependency of Ghanavats.Domain.Factory, so you don’t need to install it separately.

## 🤝 Feedback Welcome
This package is public and evolving. If you find it useful — or think it's missing something — 
feel free to share your thoughts, open an issue, or suggest improvements. 
Your feedback helps shape future releases.

## 📄 License
MIT License
