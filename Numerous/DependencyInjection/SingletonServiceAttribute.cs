using JetBrains.Annotations;

namespace Numerous.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SingletonServiceAttribute : ServiceAttribute
{
    public override ServiceType ServiceType => ServiceType.Singleton;
}

[AttributeUsage(AttributeTargets.Class)]
[UsedImplicitly]
[MeansImplicitUse]
public sealed class SingletonServiceAttribute<T> : SingletonServiceAttribute where T : class;
