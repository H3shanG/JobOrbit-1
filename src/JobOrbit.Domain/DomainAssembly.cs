using System.Reflection;

namespace JobOrbit.Domain;

public static class DomainAssembly
{
    public static Assembly Reference { get; } = typeof(DomainAssembly).Assembly;
}
