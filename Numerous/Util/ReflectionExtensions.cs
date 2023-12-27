using System.Reflection;

namespace Numerous.Util;

public static class ReflectionExtensions
{
    public static void Init(this object o)
    {
        var methods = o.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<InitAttribute>() is not null);

        foreach (var method in methods)
        {
            method.Invoke(o, null);
        }
    }
}
