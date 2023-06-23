using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public sealed class UpdateService
{
    private const string Message = "**BREAKING NEWS!**\nGsun changed his me! section!\n\n**Here's what changed:**";

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(20);

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
                    var message = GetDiff(oldPageSection, pageSection);
                    var messages = message.ToDiscordMessageStrings().ToList();
                    messages.Insert(0, Message);

                    await GetChannels().ForEachAwaitAsync(async channel =>
                    {
                        foreach (var msg in messages)
                        {
                            await channel.SendMessageAsync(msg);
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

    private static string GetDiff(string? oldText, string? newText)
    {
        oldText ??= "";
        newText ??= "";

        var diff = InlineDiffBuilder.Diff(oldText, newText);
        var diffString = "";

        foreach (var line in diff.Lines)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    diffString += "🟩 " + line.Text + "\n";

                    break;
                case ChangeType.Deleted:
                    diffString += "🟥 " + line.Text + "\n";

                    break;
            }
        }

        return diffString;
    }
}
