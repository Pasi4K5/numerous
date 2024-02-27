// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Configuration;
using Numerous.DependencyInjection;

namespace Numerous.Discord;

[SingletonService]
public sealed class AttachmentManager(ConfigManager cm)
{
    private Config Config => cm.Get();

    public string GetTargetPath(ulong msgId, IList<IAttachment> attachments, IAttachment attachment)
    {
        var imgDirPath = Config.ImageDirectory;
        var uri = new Uri(attachment.Url);
        var fileName = uri.Segments.Last();

        return Path.Join(imgDirPath, $"{msgId}_{attachments.IndexOf(attachment)}_{fileName}");
    }

    public IEnumerable<FileAttachment> GetFileAttachments(ulong msgId, bool considerFileSizeLimit = true)
    {
        var imgDirPath = Config.ImageDirectory;

        if (!Directory.Exists(imgDirPath))
        {
            return Array.Empty<FileAttachment>();
        }

        return Directory.GetFiles(imgDirPath, $"{msgId}_*")
            .Where(filePath => !considerFileSizeLimit)
            .Select(filePath => new FileAttachment(filePath, string.Join('_', Path.GetFileName(filePath).Split('_')[2..])));
    }
}
