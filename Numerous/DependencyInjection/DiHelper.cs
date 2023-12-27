using System.Reflection;

namespace Numerous.DependencyInjection;

public static class DiHelper
{
    public static IEnumerable<(ServiceType serviceType, Type type, Type impl)> GetServices()
    {
        return Assembly.GetExecutingAssembly().GetTypes().Where(t =>
            t.CustomAttributes.Any(attr => attr.AttributeType.IsAssignableTo(typeof(ServiceAttribute)))
        ).Select(t =>
        {
            var attrData = t.CustomAttributes.First(attr =>
                attr.AttributeType.IsAssignableTo(typeof(ServiceAttribute))
            );
            var attr = (ServiceAttribute)Activator.CreateInstance(attrData.AttributeType)!;
            var serviceType = attr.ServiceType;

            var type = attrData.AttributeType.IsGenericType
                ? attrData.AttributeType.GetGenericArguments()[0]
                : t;

            return (serviceType, type, t);
        });
    }
}
