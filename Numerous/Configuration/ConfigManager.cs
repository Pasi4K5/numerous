using Newtonsoft.Json;
using Numerous.DependencyInjection;

namespace Numerous.Configuration;

public sealed class ConfigManager
{
    private const string Path = "./config.json";

    private readonly object _lock = new();

    public Config Get()
    {
        lock (_lock)
        {
            var defaultConfig = new Config
            {
                GptInstructionsPath = "./instructions.txt",
            };

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
