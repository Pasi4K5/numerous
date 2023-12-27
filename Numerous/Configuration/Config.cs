using Newtonsoft.Json;

namespace Numerous.Configuration;

[JsonObject(MemberSerialization.OptOut)]
public record struct Config(
    string BotToken,
    string MongoConnectionString,
    string MongoDatabaseName,
    uint OsuClientId,
    string OsuClientSecret,
    string OpenAiApiKey,
    string GptInstructionsPath,
    bool DevMode,
    ulong DevGuildId
);
