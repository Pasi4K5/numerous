using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace GsunUpdates;

public sealed class UpdateService
{
    private const string PageMessage = "**BREAKING NEWS!**\nGsun changed his me! section!\n\n**Here's what changed:**";
    private const string TopScoreMessage = "**BREAKING NEWS!**\nGsun has a new top play: ";

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(10);

    private readonly DiscordSocketClient _client;
    private readonly JsonDb _db;
    private readonly OsuApi _osuApi;
    private readonly Timer _timer = new();

    public UpdateService(DiscordSocketClient client, JsonDb db, OsuApi osuApi)
    {
        _client = client;
        _db = db;
        _osuApi = osuApi;

        _client.Ready += RunUpdateTask;
    }

    private Task RunUpdateTask()
    {
        _timer.Interval = _updateInterval.TotalMilliseconds;
        _timer.Elapsed += async (_, _) =>
        {
            await RunPageUpdater();
            await RunScoreUpdater();
        };
        _timer.Start();

        return Task.CompletedTask;
    }

    // TODO: Those methods are almost identical.
    private async Task RunPageUpdater()
    {
        var userJson = await _osuApi.RequestAsync("users/32665201");

        if (userJson is null)
        {
            return;
        }

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
            messages.Insert(0, PageMessage);

            await GetChannels().ForEachAwaitAsync(async channel =>
            {
                foreach (var msg in messages)
                {
                    await channel.SendMessageAsync(msg);
                }
            });
        }

        SavePageSection(pageSection);
    }

    private async Task RunScoreUpdater()
    {
        var scoresJson = await _osuApi.RequestAsync("users/32665201/scores/best", new()
        {
            ["mode"] = "osu",
            ["limit"] = "1"
        });

        if (scoresJson is null)
        {
            return;
        }

        var topScoreId = scoresJson[0]?["id"]?.ToString();

        if (_db.Data["topScoreId"]?.Value<string?>() is null)
        {
            SaveTopScoreId(topScoreId);
        }

        var oldTopScoreId = _db.Data["topScoreId"]?.ToString();

        if (oldTopScoreId != topScoreId)
        {
            await GetChannels().ForEachAwaitAsync(async channel =>
            {
                await channel.SendMessageAsync(TopScoreMessage + $"https://osu.ppy.sh/scores/osu/{topScoreId}");
            });
        }

        SaveTopScoreId(topScoreId);
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

    private void SaveTopScoreId(string? topScoreId)
    {
        if (topScoreId is null)
        {
            return;
        }

        var data = _db.Data;
        data["topScoreId"] = topScoreId;
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

    // This method is kinda scuffed but who cares
    private static string GetDiff(string? oldText, string? newText)
    {
        oldText ??= "";
        newText ??= "";

        var diff = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(oldText, newText);

        var diffString = "";

        foreach (var (oldLine, newLine) in diff.OldText.Lines.Zip(diff.NewText.Lines))
        {
            switch (newLine.Type)
            {
                case ChangeType.Imaginary:
                    diffString += $"> 🟥 {oldLine.Text}\n";

                    continue;
                case ChangeType.Inserted:
                    diffString += $"> 🟩 {newLine.Text}\n";

                    continue;
                case ChangeType.Modified:
                    var oldLineText = "";

                    foreach (var oldPiece in oldLine.SubPieces)
                    {
                        if (oldPiece.Type == ChangeType.Unchanged)
                        {
                            oldLineText += oldPiece.Text;
                        }
                        else if (oldPiece.Type != ChangeType.Imaginary)
                        {
                            oldLineText += "[HLSTART]" + oldPiece.Text + "[HLEND]";
                        }
                    }

                    diffString += $"> 🟥 {Decode(oldLineText)}\n";

                    var newLineText = "";

                    foreach (var newPiece in newLine.SubPieces)
                    {
                        if (newPiece.Type == ChangeType.Unchanged)
                        {
                            newLineText += newPiece.Text;
                        }
                        else if (newPiece.Type != ChangeType.Imaginary)
                        {
                            newLineText += "[HLSTART]" + newPiece.Text + "[HLEND]";
                        }
                    }

                    diffString += $"> 🟩 {Decode(newLineText)}\n";

                    break;
            }
        }

        return diffString;

        string Decode(string s)
        {
            return s
                .Replace("[HLEND][HLSTART]", "")
                .Replace("[HLSTART]", "***")
                .Replace("[HLEND]", "***");
        }
    }
}
