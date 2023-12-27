using JetBrains.Annotations;

namespace Numerous.Util;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public sealed class InitAttribute : Attribute;
