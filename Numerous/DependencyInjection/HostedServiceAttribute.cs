using JetBrains.Annotations;

namespace Numerous.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class HostedServiceAttribute : ServiceAttribute
{
    public override ServiceType ServiceType => ServiceType.Hosted;
}

[AttributeUsage(AttributeTargets.Class)]
[UsedImplicitly]
[MeansImplicitUse]
public sealed class HostedServiceAttribute<T> : HostedServiceAttribute where T : class;
