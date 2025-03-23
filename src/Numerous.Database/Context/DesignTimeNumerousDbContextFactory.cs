// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using DotNetEnv;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Numerous.Database.Context;

[UsedImplicitly]
public sealed class DesignTimeNumerousDbContextFactory : IDesignTimeDbContextFactory<NumerousDbContext>
{
    public NumerousDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load(".env");

        var connectionString =
            $"Host=localhost;"
            + $"Username={Env.GetString("POSTGRES_USER")};"
            + $"Password={Env.GetString("POSTGRES_PASSWORD")};"
            + $"Database={Env.GetString("POSTGRES_DB")}";

        var optionsBuilder = new DbContextOptionsBuilder<NumerousDbContext>()
            .UseNpgsql(connectionString, o => o.UseNodaTime());

        return new NumerousDbContext(optionsBuilder.Options);
    }
}
