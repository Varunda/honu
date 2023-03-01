using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using static watchtower.Code.DiscordInteractions.DiscordInteractionEnums;

namespace watchtower.Code.DiscordInteractions {

    public class ServerStatusSlashCommands : PermissionSlashCommand {

        public ILogger<ServerStatusSlashCommands> _Logger { set; private get; } = default!;
        public ServerStatusInteractions _Interactions { set; private get; } = default!;

        [SlashCommand("server", "Get a server's status")]
        public async Task ServerStatus(InteractionContext ctx,
            [Option("server", "Server")] StatusWorlds world) {

            await ctx.CreateDeferred(false);

            DiscordWebhookBuilder builder = new();
            builder.AddEmbed(await _Interactions.GeneralStatus((short)world));
            builder.AddComponents(ServerStatusButtonCommands.REFRESH_WORLD((short)world));

            DiscordMessage msg = await ctx.EditResponseAsync(builder);
            _Logger.LogDebug($"message created: {msg.Id}");
        }

    }

    /// <summary>
    ///     Interactions for buttons on messages
    /// </summary>
    public class ServerStatusButtonCommands : ButtonCommandModule {

        public ILogger<ServerStatusButtonCommands> _Logger { set; private get; } = default!;
        public ServerStatusInteractions _Interactions { set; private get; } = default!;

        /// <summary>
        ///     Button to refresh the general world status in a message
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        public static DiscordButtonComponent REFRESH_WORLD(short worldID) => new(DSharpPlus.ButtonStyle.Secondary, $"@refresh-world.{worldID}", "Refresh");

        /// <summary>
        ///     Refresh a message with updated world information
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="worldID">ID of the world to refresh. Is an int, as shorts are not parsed</param>
        [ButtonCommand("refresh-world")]
        public async Task RefreshWorld(ButtonContext ctx, int worldID) {
            await ctx.Interaction.CreateDeferred(true);

            DiscordEmbed response = await _Interactions.GeneralStatus((short)worldID);

            if (ctx.Message != null) {
                _Logger.LogDebug($"putting refreshed message into id {ctx.Message.Id}");
                await ctx.Message.ModifyAsync(Optional.FromValue(response));
                await ctx.Interaction.EditResponseText($"Refreshed!");
            } else {
                await ctx.Interaction.EditResponseErrorEmbed($"message provided in context was null?");
            }
        }

    }

    /// <summary>
    ///     Backing interactions used by both slash commands and button commands
    /// </summary>
    public class ServerStatusInteractions {

        private readonly ILogger<ServerStatusInteractions> _Logger;
        private readonly ContinentLockDbStore _ContinentLockDb;

        public static readonly List<uint> InterestedZoneIDs = new() {
            Zone.Indar, Zone.Hossin, Zone.Amerish, Zone.Esamir, Zone.Oshur
        };

        public ServerStatusInteractions(ILogger<ServerStatusInteractions> logger,
            ContinentLockDbStore continentLockDb) {

            _Logger = logger;
            _ContinentLockDb = continentLockDb;
        }

        /// <summary>
        ///     Create a Discord embed that contains the general status of a world
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        public async Task<DiscordEmbed> GeneralStatus(short worldID) {
            Dictionary<string, TrackedPlayer> players;
            lock (CharacterStore.Get().Players) {
                players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
            }
            players = players.Where(iter => iter.Value.WorldID == worldID && iter.Value.Online == true)
                .ToDictionary(key => key.Key, value => value.Value);

            List<ContinentLockEntry> entries = (await _ContinentLockDb.GetAll())
                .Where(iter => iter.WorldID == worldID)
                .OrderBy(iter => iter.Timestamp)
                .ToList();

            List<PsAlert> alerts = AlertStore.Get().GetAlerts().Where(iter => iter.WorldID == worldID).ToList();

            DiscordEmbedBuilder builder = new();
            builder.Title = $"{World.GetName(worldID)}";
            builder.Url = $"https://wt.honu.pw/view/{worldID}";

            if (entries.Count == 0) {
                builder.Description = $"No entries provided?";
            }

            // entries is already ordered, the intersections gets only zones we care about
            // the union will include zones that for some reason don't have a continent lock entry
            // and they will be interested into the list last
            List<uint> orderedZones = entries.Select(iter => iter.ZoneID).Intersect(InterestedZoneIDs)
                .Union(InterestedZoneIDs).ToList();

            foreach (uint zoneID in orderedZones) {
                builder.Description += $"**{Zone.GetName(zoneID)}**\n";

                ContinentLockEntry? entry = entries.FirstOrDefault(iter => iter.ZoneID == zoneID);
                if (entry != null) {
                    builder.Description += $"Last locked: {entry.Timestamp.GetDiscordTimestamp("t")} ({entry.Timestamp.GetDiscordRelativeTimestamp()})\n";
                } else {
                    builder.Description += $"Last locked: unknown (missing from db!)\n";
                }

                builder.Description += $"Players: {players.Values.Count(iter => iter.ZoneID == zoneID)}\n";

                PsAlert? zoneAlert = alerts.FirstOrDefault(iter => iter.ZoneID == zoneID);
                if (zoneAlert != null) {
                    DateTime alertEnd = zoneAlert.Timestamp + TimeSpan.FromSeconds(zoneAlert.Duration);
                    builder.Description = $"Alert: {alertEnd.GetDiscordTimestamp("t")} ({alertEnd.GetDiscordRelativeTimestamp()})\n";
                }

                ZoneState? state = ZoneStateStore.Get().GetZone(worldID, zoneID);
                if (state != null) {

                }

                builder.Description += "\n";
            }

            builder.Timestamp = DateTimeOffset.UtcNow;

            return builder;
        }


    }

}
