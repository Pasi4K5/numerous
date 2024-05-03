// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;

namespace Numerous.Bot.DependencyInjection;

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
