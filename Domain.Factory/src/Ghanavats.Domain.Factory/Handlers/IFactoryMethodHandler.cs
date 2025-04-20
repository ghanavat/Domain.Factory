using System.Reflection;

namespace Ghanavats.Domain.Factory.Handlers;

public interface IFactoryMethodHandler
{
    public MethodInfo? GetFactoryMethod(Type type);
}
