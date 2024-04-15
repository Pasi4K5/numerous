// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Configuration;

public sealed class ConfigManager
{
    private const string Path = "./config.json";

    private readonly object _lock = new();

    public Config Get()
    {
        lock (_lock)
        {
            var defaultConfig = new Config();

            if (!File.Exists(Path))
            {
                Save(defaultConfig);

                return defaultConfig;
            }

            var text = File.ReadAllText(Path);
            var config = JsonConvert.DeserializeObject<Config?>(text);

            if (config is null)
            {
                throw new JsonException(
                    $"Failed to deserialize file \"{Path}\". "
                    + $"Please ensure that the file contains valid JSON.\n"
                    + "Delete the file to reset."
                );
            }

            Save(config.Value);

            return config.Value;
        }
    }

    private static void Save(Config obj)
    {
        var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);
        File.WriteAllText(Path, jsonString);
    }
}
