using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.GuildEngagement
{
    public class GuildSettings : IEntity
    {

        public string id { get; set; }

        public ulong GuildId
        {
            get => this.id != null && ulong.TryParse(this.id, out var result) ? result : 0;
            set => this.id = value.ToString();
        }

        public Dictionary<string, ulong> DataCenterRoleMap { get; set; } = new();

        public bool FormatNames { get; set; }

        public bool WelcomeJoiners { get; set; }

    }
}
