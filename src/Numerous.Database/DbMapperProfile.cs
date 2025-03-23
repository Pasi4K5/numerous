﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using NodaTime;
using NodaTime.Extensions;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;
using TimeZoneConverter;

namespace Numerous.Database;

public sealed class DbMapperProfile : Profile
{
    public DbMapperProfile()
    {
        CreateMap<DateTimeOffset, Instant>().ConvertUsing<DateTimeOffsetConverter>();
        CreateMap<Instant, DateTimeOffset>().ConvertUsing<InstantConverter>();

        CreateMap<DbAutoPingMapping, AutoPingMappingDto>();
        CreateMap<AutoPingMappingDto, DbAutoPingMapping>();

        CreateMap<DbBeatmapCompetition, BeatmapCompetitionDto>();
        CreateMap<BeatmapCompetitionDto, DbBeatmapCompetition>();

        CreateMap<DbBeatmapCompetitionScore, BeatmapCompetitionScoreDto>();
        CreateMap<BeatmapCompetitionScoreDto, DbBeatmapCompetitionScore>();

        CreateMap<DbBeatmapsetStats, BeatmapsetStatsDto>();
        CreateMap<BeatmapsetStatsDto, DbBeatmapsetStats>();

        CreateMap<DbBeatmapStats, BeatmapStatsDto>();
        CreateMap<BeatmapStatsDto, DbBeatmapStats>();

        CreateMap<DbBeatmapCompetitionScore, BeatmapCompetitionScoreDto>();
        CreateMap<BeatmapCompetitionScoreDto, DbBeatmapCompetitionScore>();

        CreateMap<DbChannel, ChannelDto>();
        CreateMap<ChannelDto, DbChannel>();

        CreateMap<DbDiscordMessage, DiscordMessageDto>()
            .ForMember(
                dest => dest.DeletedAt,
                opt => opt.MapFrom(src => src.DeletedAt.HasValue ? src.DeletedAt.Value.ToDateTimeOffset() : (DateTimeOffset?)null)
            );
        CreateMap<DiscordMessageDto, DbDiscordMessage>()
            .ForMember(
                dest => dest.DeletedAt,
                opt => opt.MapFrom(src => src.DeletedAt.HasValue ? src.DeletedAt.Value.ToInstant() : (Instant?)null)
            );

        CreateMap<DbDiscordMessageVersion, DiscordMessageVersionDto>()
            .ForMember(
                dest => dest.CleanContent,
                opt => opt.MapFrom(src => src.CleanContent ?? src.RawContent)
            );
        CreateMap<DiscordMessageVersionDto, DbDiscordMessageVersion>()
            .ForMember(
                dest => dest.CleanContent,
                opt => opt.MapFrom(src => src.CleanContent == src.RawContent ? null : src.CleanContent)
            );

        CreateMap<DbDiscordUser, DiscordUserDto>()
            .ForMember(
                dest => dest.TimeZone,
                opt => opt.MapFrom(src => src.TimeZoneId != null ? TZConvert.GetTimeZoneInfo(src.TimeZoneId) : null)
            );
        CreateMap<DiscordUserDto, DbDiscordUser>()
            .ForMember(
                dest => dest.TimeZoneId,
                opt => opt.MapFrom(src => src.TimeZone != null ? src.TimeZone.Id : null)
            );

        CreateMap<DbForumChannel, ForumChannelDto>();
        CreateMap<ForumChannelDto, DbForumChannel>();

        CreateMap<DbGroupRoleMapping, GroupRoleMappingDto>();
        CreateMap<GroupRoleMappingDto, DbGroupRoleMapping>();

        CreateMap<DbGuild, GuildDto>();
        CreateMap<GuildDto, DbGuild>();

        CreateMap<DbGuildStatsEntry, GuildStatsEntryDto>();
        CreateMap<GuildStatsEntryDto, DbGuildStatsEntry>();

        CreateMap<DbJoinMessage, JoinMessageDto>();
        CreateMap<JoinMessageDto, DbJoinMessage>();

        CreateMap<DbLocalBeatmap, LocalBeatmapDto>();
        CreateMap<LocalBeatmapDto, DbLocalBeatmap>();

        CreateMap<DbMessageChannel, MessageChannelDto>();
        CreateMap<MessageChannelDto, DbMessageChannel>();

        CreateMap<DbOnlineBeatmap, OnlineBeatmapDto>();
        CreateMap<OnlineBeatmapDto, DbOnlineBeatmap>();

        CreateMap<DbOnlineBeatmapset, OnlineBeatmapsetDto>();
        CreateMap<OnlineBeatmapsetDto, DbOnlineBeatmapset>();

        CreateMap<DbOsuUser, OsuUserDto>();
        CreateMap<OsuUserDto, DbOsuUser>();

        CreateMap<DbOsuUserStats, OsuUserStatsDto>();
        CreateMap<OsuUserStatsDto, DbOsuUserStats>();

        CreateMap<DbReminder, ReminderDto>();
        CreateMap<ReminderDto, DbReminder>();

        CreateMap<DbReplay, ReplayDto>();
        CreateMap<ReplayDto, DbReplay>();
    }

    public sealed class DateTimeOffsetConverter : ITypeConverter<DateTimeOffset, Instant>
    {
        public Instant Convert(DateTimeOffset source, Instant destination, ResolutionContext context)
        {
            return source.ToInstant();
        }
    }

    public sealed class InstantConverter : ITypeConverter<Instant, DateTimeOffset>
    {
        public DateTimeOffset Convert(Instant source, DateTimeOffset destination, ResolutionContext context)
        {
            return source.ToDateTimeOffset();
        }
    }
}
