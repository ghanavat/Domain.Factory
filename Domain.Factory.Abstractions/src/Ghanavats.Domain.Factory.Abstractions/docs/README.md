# Ghanavats.Domain.Factory.Abstractions

## Overview
**Ghanavats.Domain.Factory.Abstraction** is the contract package 
for [Ghanavats.Domain.Factory](https://www.nuget.org/packages/Ghanavats.Domain.Factory), 
defining the core interface and configuration structures used to support factory-based entity creation 
in a Domain-Driven Design (DDD) architecture.

This package is ideal for applications or libraries that want to depend on abstractions 
without coupling to any specific factory implementation.

## 📦 Installation

```bash
dotnet add package Ghanavats.Domain.Factory.Abstraction
```
> ✅ Supports .NET 8 and .NET 9

## 🔧 What’s Included
This package contains the following key elements:

### ✅ IDomainFactory<TRequest, TResponse>
A generic interface that defines the contract for creating domain entity objects in a configurable way.

```csharp
public interface IDomainFactory<in TRequest, out TResponse>
    where TRequest : class
    where TResponse : EntityBase
{
    TResponse? CreateEntityObject(TRequest request, [Optional] Action<DomainFactoryOption> action);
}
```
* **TRequest**: Input request object, typically a command or DTO.
* **TResponse**: A domain entity that inherits from EntityBase.
* **Optional DomainFactoryOption**: Allows the caller to influence how the object is constructed.

### ✅ DomainFactoryOption
A flexible configuration object that lets consumers:

* Ignore specific properties from the request object.
* Inject additional properties into the creation process.
* Customise how the factory interprets the input data.

Example:
```csharp
option.IgnoreProperties(["PropertyA", "PropertyB"]);

option.AddProperties(new Dictionary<string, object>
{
    { "SomeNewProperty", 123 },
    { "CreatedOn", DateTime.UtcNow }
}.ToImmutableDictionary());
```
## 🤝 Why Use the Abstractions Package?
This package follows the dependency inversion principle, 
allowing application layers to depend on abstractions — not implementations.

Use Ghanavats.Domain.Factory.Abstraction when:

* You want to mock or stub the factory for unit testing.
* You are building your own custom factory implementation.
* You want to decouple your application logic from the Domain.Factory implementation.

## 🔗 Related Packages
### [Ghanavats.Domain.Factory](https://www.nuget.org/packages/Ghanavats.Domain.Factory)
The full implementation that uses reflection to create domain entities based on request objects and DomainFactoryOption.

### [Ghanavats.Domain.Primitives](https://www.nuget.org/packages/Ghanavats.Domain.Primitives)
Used to define base types like EntityBase. This package is required in both abstraction and implementation libraries.
