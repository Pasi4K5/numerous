using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public sealed class UpdateService
{
    private const string Message = "**BREAKING NEWS!**\nGsun changed his me! section!\n\n**New text:**";

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(2);

    private readonly DiscordSocketClient _client;
    private readonly JsonDb _db;
    private readonly OsuApi _osuApi;

    public UpdateService(DiscordSocketClient client, JsonDb db, OsuApi osuApi)
    {
        _client = client;
        _db = db;
        _osuApi = osuApi;

        _client.Ready += RunUpdateTask;
    }

    private Task RunUpdateTask()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var userJson = await _osuApi.RequestAsync("users/32665201");
                var pageSection = userJson["page"]?["raw"]?.ToString();

                if (_db.Data["pageSection"]?.Value<string?>() is null)
                {
                    SavePageSection(pageSection);
                }

                var oldPageSection = _db.Data["pageSection"]?.ToString();

                if (oldPageSection != pageSection)
                {
                    var messages = (pageSection?.ToDiscordMessageStrings() ?? Enumerable.Empty<string>()).ToList();
                    messages.Insert(0, Message);

                    await GetChannels().ForEachAwaitAsync(async channel =>
                    {
                        foreach (var message in messages)
                        {
                            await channel.SendMessageAsync(message);
                        }
                    });
                }

                SavePageSection(pageSection);

                await Task.Delay(_updateInterval);
            }
        });

        return Task.CompletedTask;
    }

    private void SavePageSection(string? pageSection)
    {
        if (pageSection is null)
        {
            return;
        }

        var data = _db.Data;
        data["pageSection"] = pageSection;
        _db.Save(data);
    }

    private async IAsyncEnumerable<SocketTextChannel> GetChannels()
    {
        var channelIds =
            _db.Data["channels"]?.Select(channelId => channelId.ToObject<ChannelInfo>()?.Id ?? 0) ?? Enumerable.Empty<ulong>();

        foreach (var channelId in channelIds)
        {
            var channel = await _client.GetChannelAsync(channelId);

            if (channel is SocketTextChannel textChannel)
            {
                yield return textChannel;
            }
        }
    }
}
