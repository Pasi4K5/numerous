// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.DependencyInjection;

namespace Numerous.Bot.Services;

public interface IFileService
{
    bool DirectoryExists(string path);
    string[] GetFiles(string path, string searchPattern);
}

[SingletonService<IFileService>]
public class FileService : IFileService
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        return Directory.GetFiles(path, searchPattern);
    }
}
