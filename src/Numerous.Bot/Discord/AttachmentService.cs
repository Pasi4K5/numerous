// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Bot.Services;
using Numerous.Common.Services;

namespace Numerous.Bot.Discord;

public sealed class AttachmentService(IConfigService cfgService, IFileService files)
{
    private readonly HttpClient _httpClient = new();

    private Config Config => cfgService.Get();

    public async Task SaveAttachmentAsync(string url, string targetPath)
    {
        var response = await _httpClient.GetAsync(url);
        await using var fs = new FileStream(targetPath, FileMode.CreateNew);
        await response.Content.CopyToAsync(fs);
    }

    public string GetTargetPath(ulong msgId, IAttachment attachment, int index)
    {
        var imgDirPath = Config.AttachmentDirectory;
        var fileName = new Uri(attachment.Url).Segments.Last();

        return Path.Join(imgDirPath, $"{msgId}_{index}_{fileName}");
    }

    public IEnumerable<FileAttachmentInfo> GetFileAttachments(ulong msgId)
    {
        var imgDirPath = Config.AttachmentDirectory;

        if (!files.DirectoryExists(imgDirPath))
        {
            return Array.Empty<FileAttachmentInfo>();
        }

        return files.GetFiles(imgDirPath, $"{msgId}_*")
            .Select(filePath => new FileAttachmentInfo(filePath, string.Join('_', Path.GetFileName(filePath).Split('_')[2..])));
    }
}
