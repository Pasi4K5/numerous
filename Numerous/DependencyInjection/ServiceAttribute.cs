namespace Numerous.DependencyInjection;

public abstract class ServiceAttribute : Attribute
{
    public abstract ServiceType ServiceType { get; }
}
