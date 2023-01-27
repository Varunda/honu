using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.DiscordInteractions {

    /// <summary>
    ///     PSB sheet management slash command parent
    /// </summary>
    [SlashCommandGroup("access", "Manage access to PSB sheets")]
    public class AccessSlashCommands : ApplicationCommandModule {

        [SlashCommandGroup("practice", "Access commands for practice sheets")]
        public class Practice : PermissionSlashCommand {

            public ILogger<Practice> _Logger { private get; set; } = default!;
            public HonuAccountDbStore _HonuAccountDb { private get; set; } = default!;
            public PsbContactSheetRepository _PsbContactRepository { private get; set; } = default!;

            /// <summary>
            ///     List who has access to a practice sheet
            /// </summary>
            /// <param name="ctx">Context (provided)</param>
            /// <param name="tag">Tag to use</param>
            [SlashCommand("list", "list who has permission to a PSB sheet")]
            public async Task Command(InteractionContext ctx, [Option("tag", "Tag of the outfit")] string tag) {
                try {
                    if (await _CheckPermission(ctx, HonuPermission.PSB_PRACTICE_GET) == false) {
                        return;
                    }

                    await ctx.CreateDeferredText("Loading sheets...");

                    PsbDriveFile? sheet = await GetPracticeSheet(ctx, tag);
                    if (sheet == null) {
                        return;
                    }

                    await ctx.EditResponseText($"Loading permissions for {sheet.Name}...");

                    List<PsbDrivePermission>? perms = await _PsbDrive.GetPermissions(sheet.ID);
                    if (perms == null) {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to load permission for {sheet.Name}/{sheet.ID}"));
                        return;
                    }

                    List<PsbPracticeContact> practiceContacts = await _PsbContactRepository.GetPracticeContacts();

                    int psbCount = 0; // how many PSB staff users have permission to this block
                    int repCount = 0; // how many reps have permission to this block
                    int otherCount = 0; // how many users have permission to this block for an unknown reason

                    DiscordWebhookBuilder builder = new();
                    DiscordEmbedBuilder embed = new();
                    embed.Title = $"Users with permission to {sheet.Name}";
                    embed.Url = $"https://docs.google.com/spreadsheets/d/{sheet.ID}";
                    foreach (PsbDrivePermission perm in perms) {

                        // Find the discord ID based on email
                        ulong? discordID = null;

                        // I don't like 'writer' as a role, cause it's displayed as editor in the UI
                        embed.Description += $"[{(perm.Role == "writer" ? "editor" : perm.Role)}] ";

                        // Find the discord ID either by Honu account
                        HonuAccount? permAccount = await _HonuAccountDb.GetByEmail(perm.Email, CancellationToken.None);
                        if (permAccount != null) {
                            discordID = permAccount.DiscordID;
                            ++psbCount;
                        }

                        // Or practice rep contacts
                        if (discordID == null && practiceContacts != null) {
                            PsbContact? contact = practiceContacts.FirstOrDefault(iter => iter.Email.Trim().ToLower() == perm.Email.Trim().ToLower());

                            if (contact != null) {
                                discordID = contact.DiscordID;
                                ++repCount;
                            }
                        }

                        if (perm.Email.ToLower() == "planetsidebattles@gmail.com") {
                            ++psbCount;
                            embed.Description += $"PSB";
                        } else if (discordID == null) {
                            embed.Description += $"!!! <missing> {perm.DisplayName} ({perm.Email})";
                            ++otherCount;
                        } else {
                            embed.Description += $"<@{discordID.Value}>";
                        }

                        embed.Description += $"\n";
                    }

                    embed.Description = $"Users with access:\nPSB staff: {psbCount}\nReps: {repCount}\nUnknown: {otherCount}\n\n" + embed.Description;

                    builder.AddEmbed(embed);

                    await ctx.EditResponseAsync(builder);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            /// <summary>
            ///     Add a user to a practice block
            /// </summary>
            /// <param name="ctx">context (provided)</param>
            /// <param name="user">User object</param>
            /// <param name="tag">Outfit tag</param>
            /// <param name="email">Email of the user</param>
            //[SlashCommand("add", "Add a Discord user to a practice rep block")]
            public async Task AddUser(InteractionContext ctx,
                [Option("User", "Discord user to grant access to the block")] DiscordUser user,
                [Option("Outfit", "Outfit tag to add to the block")] string tag,
                [Option("Email", "Email address of the user if needed")] string email) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.PSB_PRACTICE_MANAGE) == false) {
                        return;
                    }

                    // find the sheet, lets us get the permissions
                    await ctx.CreateDeferredText("Processing command...");
                    PsbDriveFile? sheet = await GetPracticeSheet(ctx, tag);
                    if (sheet == null) {
                        return;
                    }

                    // we have the GDrive file ID, ensure they aren't already on this sheet
                    await ctx.EditResponseText("Found practice sheet, checking existing permissions...");
                    PsbDrivePermission? perm = await GetPermissionFromEmail(ctx, sheet.ID, email);
                    if (perm != null) {
                        List<PsbPracticeContact> sheetContacts = await _PsbContactRepository.GetPracticeContacts();
                        PsbPracticeContact? existingContact = sheetContacts?.FirstOrDefault(iter => iter.Email.ToLower() == email.ToLower());

                        await ctx.EditResponseText($"Error: email {email} is already on {tag}'s practice sheet under user {(existingContact != null ? $"<@{existingContact.DiscordID}>" : perm.DisplayName)}");
                        return;
                    }

                    await _PsbDrive.AddUserToDriveFile(sheet.ID, email);

                    await ctx.EditResponseText($"Success! Added <@{user.Id}>/{user.Username}#{user.Discriminator}/{user.Id} to {tag}'s practice sheet");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            /// <summary>
            ///     Slash command to remove a user from a practice sheet
            /// </summary>
            /// <param name="ctx">context (provided)</param>
            /// <param name="user">User being removed</param>
            /// <param name="tag">Tag of the outfit that has the practice sheet</param>
            //[SlashCommand("remove", "Remove a user from a practice sheet")]
            public async Task RemoveCommand(InteractionContext ctx,
                [Option("User", "Discord user to be removed from the sheet")] DiscordUser user,
                [Option("Tag", "Outfit tag the user will be removed from")] string tag) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.PSB_PRACTICE_MANAGE) == false) {
                        return;
                    }

                    await ctx.CreateDeferredText("Processing command...");

                    // find the PsbContact of the user, which has the user's email
                    await ctx.EditResponseText($"Found practice sheet, checking permissions...");
                    PsbContact? contact = (await _PsbContactRepository.GetPracticeContacts())?.FirstOrDefault(iter => iter.DiscordID == user.Id);
                    if (contact == null) {
                        await ctx.EditResponseText($"Failed: could not find {nameof(PsbContact)} for <@{user.Id}> is not in the practice rep sheet");
                        return;
                    }

                    // find the sheet, which lets us get the permissions
                    await ctx.EditResponseText($"Removing <@{user.Id}> from {tag}'s practice sheet...");
                    PsbDriveFile? sheet = await GetPracticeSheet(ctx, tag);
                    if (sheet == null) {
                        return;
                    }

                    // We have the user's email and the GDrive file ID, ensure they have permission to this sheet
                    PsbDrivePermission? perm = await GetPermissionFromEmail(ctx, sheet.ID, contact.Email);
                    if (perm == null) {
                        await ctx.EditResponseText($"Error: Data is inconsistent! <@{user.Id}> is on the practice rep sheet, but does not have a GDrive permission object");
                        return;
                    }

                    await _PsbDrive.RemoveUserFromDriveFile(sheet.ID, perm.ID);

                    await ctx.EditResponseText($"Success! Removed <@{user.Id}>/{user.Username}#{user.Discriminator}/{user.Id} from {tag}'s practice block");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            //[SlashCommand("remove-all", "Remove someone from all practice sheets")]
            public async Task RemoveFromAll(InteractionContext ctx,
                [Option("email", "Email to be removed from all sheets")] string email) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.HONU_ACCOUNT_ADMIN) == false) {
                        return;
                    }

                    // get the practice sheets
                    await ctx.CreateDeferredText($"Removing {email} from all practice sheets...");
                    List<PsbDriveFile>? files = await _PsbDrive.GetPracticeSheets();
                    if (files == null) {
                        await ctx.EditResponseText($"Error: failed to load practice sheets: {_PsbDrive.GetInitializeFailureReason()}");
                        return;
                    }

                    await ctx.EditResponseText($"Removing {email} from {files.Count} practice sheets...");

                    foreach (PsbDriveFile file in files) {
                        _Logger.LogDebug($"Removing {email} from {file.Name}/{file.ID}");

                        List<PsbDrivePermission>? perms = await _PsbDrive.GetPermissions(file.ID);
                        if (perms == null) {
                            _Logger.LogWarning($"null perms for {file.Name}/{file.ID}: {_PsbDrive.GetInitializeFailureReason()}");
                            continue;
                        }

                        PsbDrivePermission? perm = perms.FirstOrDefault(iter => iter.Email.ToLower() == email.ToLower());
                        if (perm == null) {
                            _Logger.LogDebug($"No permission for {email} (Have {perms.Count} entries)");
                            continue;
                        }

                        try {
                            await _PsbDrive.RemoveUserFromDriveFile(file.ID, perm.ID);
                        } catch (Exception gEx) {
                            _Logger.LogError(gEx, $"Error removing {email} from {file.Name}/{file.ID}");
                        }
                    }

                    await ctx.EditResponseText($"Success!");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            [SlashCommand("transfer-owner", "Transfer ownership of a practice block sheet from the service account to another")]
            public async Task TransferOwnership(InteractionContext ctx,
                [Option("tag", "Tag of the outfit")] string tag,
                [Option("email", "Email to transfer ownership to")] string email) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.HONU_DISCORD_ADMIN) == false) {
                        return;
                    }

                    await ctx.CreateDeferredText($"Processing command...");

                    PsbDriveFile? sheet = await GetPracticeSheet(ctx, tag);
                    if (sheet == null) {
                        return;
                    }

                    PsbDrivePermission? perm = await GetPermissionFromEmail(ctx, sheet.ID, email);
                    if (perm == null) {
                        await ctx.EditResponseText($"Failed to find permission for {email} on file {sheet.Name}/{sheet.ID}");
                        return;
                    }

                    await _PsbDrive.TransferOwnership(sheet.ID, perm.ID);

                    await ctx.EditResponseText($"Success!");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }

            }

            /// <summary>
            ///     Accept ownership of a practice block sheet
            /// </summary>
            /// <param name="ctx">Context (provided)</param>
            /// <param name="tag">Outfit tag</param>
            //[SlashCommand("accept-ownership", "Accept ownership of a practice block")]
            public async Task AcceptOwnership(InteractionContext ctx,
                [Option("tag", "Tag")] string tag) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.HONU_DISCORD_ADMIN) == false) {
                        return;
                    }

                    await ctx.CreateDeferredText("Transfering ownership");
                    PsbDriveFile? practiceSheet = await GetPracticeSheet(ctx, tag);
                    if (practiceSheet == null) {
                        await ctx.EditResponseText($"No practice sheet");
                        return;
                    }

                    _Logger.LogDebug($"Email: {_PsbDrive.GetEmail()}");

                    await ctx.EditResponseText("Loaded sheet, checking permissions...");
                    PsbDrivePermission? perm = await GetPermissionFromEmail(ctx, practiceSheet.ID, _PsbDrive.GetEmail());
                    if (perm == null) {
                        await ctx.EditResponseText($"no perm");
                        return;
                    }

                    await _PsbDrive.AcceptOwnership(practiceSheet.ID, perm.ID);

                    await ctx.EditResponseText("Success!");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            /// <summary>
            ///     Accept ownership of all practice account sheets that have a pending transferOwnership flag
            /// </summary>
            /// <param name="ctx">Context (provided)</param>
            //[SlashCommand("accept-ownership-all", "Accept ownership of all practice sheets")]
            public async Task AcceptOwnershipAll(InteractionContext ctx) {
                try {
                    if (await _CheckPermission(ctx, HonuPermission.HONU_DISCORD_ADMIN) == false) {
                        return;
                    }

                    await ctx.CreateDeferredText("Processing command...");

                    List<PsbDriveFile>? files = await _PsbDrive.GetPracticeSheets();
                    if (files == null) {
                        await ctx.EditResponseText($"Error: no files");
                        return;
                    }

                    string email = _PsbDrive.GetEmail().ToLower();

                    foreach (PsbDriveFile file in files) {
                        List<PsbDrivePermission>? permissions = await _PsbDrive.GetPermissions(file.ID);
                        if (permissions == null) {
                            continue;
                        }

                        PsbDrivePermission? perm = permissions.FirstOrDefault(iter => iter.Email.ToLower() == email);
                        if (perm == null) {
                            _Logger.LogDebug($"No permission for {email} on {file.Name}/{file.ID}");
                            continue;
                        }

                        _Logger.LogTrace($"{JToken.FromObject(perm)}");

                        await _PsbDrive.AcceptOwnership(file.ID, perm.ID);
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

            /// <summary>
            ///     Get the practice sheet for a specific tag, editing a deferred response with appropriate errors
            /// </summary>
            /// <param name="ctx">Interaction context</param>
            /// <param name="tag">Tag of the outfit to get the practice sheet of</param>
            /// <returns>
            ///     The <see cref="PsbDriveFile"/> that is the practice sheet for the outfit <paramref name="tag"/>,
            ///     or <c>null</c> if it does not exist. If null, an appropriate error has already been sent
            ///     in the interaction context
            /// </returns>
            private async Task<PsbDriveFile?> GetPracticeSheet(InteractionContext ctx, string tag) {
                if (await _CheckPermission(ctx, HonuPermission.PSB_PRACTICE_MANAGE) == false) {
                    return null;
                }

                List<PsbDriveFile>? practiceSheets = await _PsbDrive.GetPracticeSheets();
                if (practiceSheets == null) {
                    await ctx.EditResponseText($"Error: failed to load practice sheets: {_PsbDrive.GetInitializeFailureReason()}");
                    return null;
                }

                string sheetName = $"Outfit Practice [{tag}]".ToLower();
                PsbDriveFile? sheet = practiceSheets.FirstOrDefault(iter => iter.Name.ToLower() == sheetName);
                if (sheet == null) {
                    await ctx.EditResponseText($"Error: failed to find {tag}'s practice sheet");
                    return null;
                }

                return sheet;
            }

            /// <summary>
            ///     Get the permission object for a specific GDrive file
            /// </summary>
            /// <param name="ctx">interaction context</param>
            /// <param name="driveFileID">ID of the file that has the permissions</param>
            /// <param name="email">Email to return from the permissions of that file</param>
            /// <returns>
            ///     The <see cref="PsbDrivePermission"/> that is the GDrive permission object for the
            ///     user with the email of <paramref name="email"/>, for the GDrive file <paramref name="driveFileID"/>.
            ///     Or <c>null</c> if the permission does not exist
            /// </returns>
            private async Task<PsbDrivePermission?> GetPermissionFromEmail(InteractionContext ctx, string driveFileID, string email) {
                if (await _CheckPermission(ctx, HonuPermission.PSB_PRACTICE_MANAGE) == false) {
                    return null;
                }

                List<PsbDrivePermission>? permissions = await _PsbDrive.GetPermissions(driveFileID);
                if (permissions == null) {
                    return null;
                }

                PsbDrivePermission? perm = permissions.FirstOrDefault(iter => iter.Email.ToLower() == email.ToLower());

                return perm;
            }

        }

        [SlashCommandGroup("cache", "Clear cached responses")]
        public class ClearCache : PermissionSlashCommand {

            public ILogger<ClearCache> _Logger { private get; set; } = default!;
            public PsbContactSheetRepository _PsbContactRepository { private get; set; } = default!;

            /// <summary>
            ///     Clear a cached rep sheet
            /// </summary>
            /// <param name="ctx">Context (provided)</param>
            /// <param name="which">Which rep sheet will be removed from cache</param>
            [SlashCommand("clear", "Clear a cached response")]
            public async Task ClearCacheCommand(InteractionContext ctx,
                [Choice("Practice", "practice")]
                [Option("sheet", "Which sheet to clear the cached value from")] string which) {

                try {
                    if (await _CheckPermission(ctx, HonuPermission.PSB_NAMED_MANAGE, HonuPermission.PSB_PRACTICE_MANAGE) == false) {
                        return;
                    } 

                    if (which == "practice") {
                        _PsbContactRepository.ClearCache();
                        await ctx.CreateImmediateText($"Cleared {which} cache");
                    } else {
                        await ctx.CreateImmediateText($"Unknown value '{which}'. No cache cleared");
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to execute Discord slash command");

                    try {
                        await ctx.EditResponseText($"Internal error: {ex.Message}");
                    } catch(Exception ex2) {
                        _Logger.LogError(ex2, "Failed to update Discord slash command with exception text");
                    }
                }
            }

        }

    }
}
