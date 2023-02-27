using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Discord;
using watchtower.Models.Queues;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedAlertEndService : BackgroundService {

        private readonly ILogger<HostedAlertEndService> _Logger;
        private readonly AlertEndQueue _Queue;

        private readonly AlertDbStore _AlertDb;
        private readonly AlertPlayerDataRepository _AlertPlayerRepository;
        private readonly AlertEndSubscriptionDbStore _AlertEndSubscriptionDb;
        private readonly DiscordMessageQueue _DiscordMessageQueue;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;

        public HostedAlertEndService(ILogger<HostedAlertEndService> logger, AlertEndQueue queue,
            AlertDbStore alertDb, AlertPlayerDataRepository alertPlayerRepository,
            AlertEndSubscriptionDbStore alertEndSubscriptionDb, DiscordMessageQueue discordMessageQueue,
            CharacterRepository characterRepository, OutfitRepository outfitRepository) {

            _Logger = logger;
            _Queue = queue;

            _AlertDb = alertDb;
            _AlertPlayerRepository = alertPlayerRepository;
            _AlertEndSubscriptionDb = alertEndSubscriptionDb;
            _DiscordMessageQueue = discordMessageQueue;
            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                AlertEndQueueEntry entry = await _Queue.Dequeue(stoppingToken);
                try {
                    await Process(entry, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"error while processing alert end {entry.Alert.ID}");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"stopping with {_Queue.Count} in queue");
                }
            }
        }

        private async Task Process(AlertEndQueueEntry entry, CancellationToken cancel) {
            _Logger.LogDebug($"Processing alert end for {entry.Alert.ID} on {entry.Alert.WorldID}");

            Stopwatch timer = Stopwatch.StartNew();
            List<AlertPlayerDataEntry> parts = await _AlertPlayerRepository.GetByAlert(entry.Alert, cancel);
            _Logger.LogDebug($"created {parts.Count} players for alert {entry.Alert.ID} on {entry.Alert.WorldID} in {entry.Alert.ZoneID} in {timer.ElapsedMilliseconds}ms");
            timer.Reset();

            entry.Alert.Participants = parts.Count;
            await _AlertDb.UpdateByID(entry.Alert.ID, entry.Alert);

            List<AlertEndSubscription> subs = await _AlertEndSubscriptionDb.GetAll();
            _Logger.LogDebug($"Processing {subs.Count} alert end subscriptions");

            // don't sent duplicate alert end subscriptions to the same channel or user
            HashSet<ulong> sentToChannel = new();
            HashSet<ulong> sentToUser = new();

            foreach (AlertEndSubscription sub in subs) {
                if (sub.SourceType == AlertEndSubscriptionSourceType.UNKNOWN) {
                    _Logger.LogError($"unknown source type for subscription {sub.ID}");
                    continue;
                }

                if (parts.Count < sub.WorldCharacterMinimum) {
                    _Logger.LogTrace($"for sub {sub.ID}, only {parts.Count} players played, not the minimum of {sub.WorldCharacterMinimum}");
                    continue;
                }

                HonuDiscordMessage msg = new();

                // character (to DMs only)
                if (sub.SourceType == AlertEndSubscriptionSourceType.CHARACTER) {
                    CharacterAlertEndSubscription cSub = new(sub);

                    AlertPlayerDataEntry? playerData = parts.FirstOrDefault(iter => iter.CharacterID == cSub.CharacterID);
                    if (playerData == null) {
                        _Logger.LogTrace($"for sub {cSub.ID}, character {cSub.CharacterID} did not participate, not sending");
                        continue;
                    }

                    if (playerData.SecondsOnline < cSub.CharacterMinimumSeconds) {
                        _Logger.LogTrace($"for sub {cSub.ID}, character {cSub.CharacterID} was only online for {playerData.SecondsOnline}, not the minimum of {cSub.CharacterMinimumSeconds}");
                        continue;
                    }

                    PsCharacter? c = await _CharacterRepository.GetByID(cSub.CharacterID, CensusEnvironment.PC, fast: true);

                    DiscordEmbedBuilder builder = StartMessage(entry.Alert, parts.Count);
                    builder.Description += $"\n\n{c?.GetDisplayName() ?? $"<missing {cSub.CharacterID}>"} "
                        + $"participated for {TimeSpan.FromSeconds(playerData.SecondsOnline).GetRelativeFormat()}";

                    AddFields(new List<AlertPlayerDataEntry> { playerData }, ref builder);

                    msg.Embeds.Add(builder);

                } else if (sub.SourceType == AlertEndSubscriptionSourceType.OUTFIT) {
                    OutfitAlertEndSubscription oSub = new(sub);
                    List<AlertPlayerDataEntry> outfitData = parts.Where(iter => iter.OutfitID == oSub.OutfitID).ToList();

                    if (outfitData.Count < oSub.OutfitCharacterMinimum) {
                        _Logger.LogTrace($"for sub {oSub.ID}, only {outfitData.Count} players from outfit played, not the minimum of {oSub.OutfitCharacterMinimum}");
                        continue;
                    }

                    PsOutfit? o = await _OutfitRepository.GetByID(oSub.OutfitID);

                    DiscordEmbedBuilder builder = StartMessage(entry.Alert, parts.Count);
                    builder.Description += $"\n\n[{o?.Tag}] {o?.Name ?? $"<missing {oSub.OutfitID}>"} had {outfitData.Count} players participate";

                    AddFields(outfitData, ref builder);

                    msg.Embeds.Add(builder);

                } else if (sub.SourceType == AlertEndSubscriptionSourceType.WORLD) {
                    WorldAlertEndSubscription wSub = new(sub);
                    if (entry.Alert.WorldID != wSub.WorldID) {
                        continue;
                    }

                    DiscordEmbedBuilder builder = StartMessage(entry.Alert, parts.Count);

                    AddFields(parts, ref builder);

                    msg.Embeds.Add(builder);
                } else {
                    _Logger.LogWarning($"unhandled source type: {sub.SourceType} from subscription {sub.ID}");
                    continue;
                }

                AlertEndSubscriptionTarget target = new(sub);
                if (target.TargetType == AlertEndSubscriptionTargetType.CHANNEL) {
                    if (sentToChannel.Contains(target.ChannelID)) {
                        _Logger.LogTrace($"for sub {sub.ID}, Discord channel {target.ChannelID} already got a message");
                        continue;
                    }

                    sentToChannel.Add(target.ChannelID);

                    msg.GuildID = target.GuildID;
                    msg.ChannelID = target.ChannelID;
                } else if (target.TargetType == AlertEndSubscriptionTargetType.USER) {
                    if (sentToUser.Contains(sub.CreatedByID)) {
                        _Logger.LogTrace($"for sub {sub.ID}, Discord user {sub.CreatedByID} already got sent a message");
                        continue;
                    }

                    sentToUser.Add(sub.CreatedByID);

                    msg.TargetUserID = target.UserID;
                } else {
                    _Logger.LogWarning($"unhandled target type {target.TargetType} from sub {sub.ID}");
                    continue;
                }

                _DiscordMessageQueue.Queue(msg);
            }

        }

        private DiscordEmbedBuilder StartMessage(PsAlert alert, int playerCount) {
            DiscordEmbedBuilder builder = new();
            builder.Title = $"Alert {alert.WorldID}-{alert.InstanceID} ended";
            builder.Url = $"https://wt.honu.pw/alert/{alert.ID}/";
            builder.Description = $"Alert {alert.WorldID}-{alert.InstanceID} ended with {playerCount} unique players\n\n";
            builder.Description += $"**Server**: {World.GetName(alert.WorldID)}\n";
            builder.Description += $"**Continent: **{Zone.GetName(alert.ZoneID)}\n";
            builder.Description += $"**Winner: **{((alert.VictorFactionID == null || alert.VictorFactionID.Value == 0) ? "none" : Faction.GetName(alert.VictorFactionID.Value))}\n";

            if (alert.VictorFactionID == Faction.VS) {
                builder.Color = DiscordColor.Magenta;
            } else if (alert.VictorFactionID == Faction.NC) {
                builder.Color = DiscordColor.Blue;
            } else if (alert.VictorFactionID == Faction.TR) {
                builder.Color = DiscordColor.Red;
            } else {
                builder.Color = DiscordColor.Gray;
            }

            return builder;
        }

        /// <summary>
        ///     add stat fields to the builder
        /// </summary>
        /// <param name="data">list of players to include the stats of. Each stat is a sum of this list</param>
        /// <param name="builder">reference to the builder that the fields will be added to</param>
        private void AddFields(List<AlertPlayerDataEntry> data, ref DiscordEmbedBuilder builder) {
            int kills = data.Sum(iter => iter.Kills);
            int deaths = data.Sum(iter => iter.Deaths);

            int heals = data.Sum(iter => iter.Heals);
            int revives = data.Sum(iter => iter.Revives);
            int shieldReps = data.Sum(iter => iter.ShieldRepairs);

            int resupplies = data.Sum(iter => iter.Resupplies);
            int repairs = data.Sum(iter => iter.Repairs);

            int spawns = data.Sum(iter => iter.Spawns);

            builder.AddField("Kills", $"{kills}", true);
            builder.AddField("Deaths", $"{deaths}", true);
            builder.AddField("K/D", $"{(kills / Math.Max(1m, deaths)):F2}", true);

            if (heals > 0 || revives > 0 || shieldReps > 0) {
                builder.AddField("Heals", $"{heals}", true);
                builder.AddField("Revives", $"{revives}", true);
                builder.AddField("Shield reps", $"{shieldReps}", true);
            }

            if (resupplies > 0 || repairs > 0) {
                builder.AddField("Resupplies", $"{resupplies}", true);
                builder.AddField("Repairs", $"{repairs}", true);
            }

            builder.AddField("Spawns", $"{spawns}", true);

        }

    }
}
