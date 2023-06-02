using Newtonsoft.Json;

namespace GsunUpdates;

public sealed class Config
{
    private const string Path = "./config.json";

    private static readonly object _lock = new();

    public string BotToken { get; init; } = "";
    public uint OsuClientId { get; init; }
    public string OsuClientSecret { get; init; } = "";
    public string OpenAiApiKey { get; init; } = "";

    public string GptInstructions { get; init; } = "";

    public static Config Get()
    {
        lock (_lock)
        {
            var defaultConfig = new Config();

            if (!File.Exists(Path))
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
            }

            var text = File.ReadAllText(Path);
            var config = JsonConvert.DeserializeObject<Config>(text);

            if (config is null)
            {
                throw new JsonException(
                    "Failed to deserialize config. Please ensure that ./config.json is valid JSON.\n"
                    + "Delete the file to reset."
                );
            }

            return config;
        }
    }
}
