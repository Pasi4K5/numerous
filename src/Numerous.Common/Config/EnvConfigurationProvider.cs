// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Numerous.Common.Config;

public sealed partial class EnvConfigurationProvider(IConfigurationRoot configurationRoot) : ConfigurationProvider
{
    [GeneratedRegex(@"\${([A-Z0-9_]+)}")]
    private static partial Regex EnvVarRegex();

    public override void Load()
    {
        foreach (var (key, value) in configurationRoot.AsEnumerable())
        {
            if (value is null)
            {
                continue;
            }

            var newValue = value;

            foreach (var match in EnvVarRegex().Matches(value).AsEnumerable())
            {
                var envVarName = match.Groups[1].Value;
                var envValue = Environment.GetEnvironmentVariable(envVarName);

                newValue = !string.IsNullOrEmpty(envValue)
                    ? newValue.Replace(match.Groups[0].Value, envValue)
                    : Data.TryGetValue(key, out var existingValue)
                        ? existingValue
                        : newValue;
            }

            Data[key] = newValue;
        }
    }
}
