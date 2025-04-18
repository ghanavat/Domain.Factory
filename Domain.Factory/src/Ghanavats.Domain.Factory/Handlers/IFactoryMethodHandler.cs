using System.Reflection;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Primitives;

namespace Ghanavats.Domain.Factory.Handlers;

public interface IFactoryMethodHandler
{
    public MethodInfo? GetFactoryMethod(Type type);
}
