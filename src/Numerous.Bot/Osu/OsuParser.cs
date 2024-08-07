// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using System.Text;
using Numerous.Common.Util;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.IO.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace Numerous.Bot.Osu;

public static class OsuParser
{
    static OsuParser()
    {
        Decoder.RegisterDependencies(new AssemblyRulesetStore());
    }

    public static ScoreInfo ParseReplay(WorkingBeatmap beatmap, byte[] data)
    {
        using var stream = new MemoryStream(data);
        var decoder = new NumerousLegacyScoreDecoder<OsuRuleset>(beatmap);

        var score = decoder.Parse(stream);

        using var stream2 = new MemoryStream(data[5..]);
        using var reader = new SerializationReader(stream2);

        score.ScoreInfo.BeatmapHash = reader.ReadString();
        reader.ReadString(); // Player name
        score.ScoreInfo.Hash = reader.ReadString();

        return score.ScoreInfo;
    }

    public static WorkingBeatmap ParseBeatmap(string osuText)
    {
        using var beatmapStream = new MemoryStream(Encoding.UTF8.GetBytes(osuText));
        using var beatmapReader = new LineBufferedReader(beatmapStream);

        var beatmap = Decoder.GetDecoder<Beatmap>(beatmapReader).Decode(beatmapReader);

        beatmap.BeatmapInfo.MD5Hash = MD5.HashData(Encoding.UTF8.GetBytes(osuText)).ToHexString();

        return new NumerousBeatmap(beatmap);
    }
}
