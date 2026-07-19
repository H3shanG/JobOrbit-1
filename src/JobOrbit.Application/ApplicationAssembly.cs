using System.Reflection;

namespace JobOrbit.Application;

public static class ApplicationAssembly
{
    public static Assembly Reference { get; } = typeof(ApplicationAssembly).Assembly;
}
