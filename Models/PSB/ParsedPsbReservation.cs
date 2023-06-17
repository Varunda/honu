using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Contains a <see cref="PsbReservation"/> and information about how it was parsed
    /// </summary>
    public class ParsedPsbReservation {

        /// <summary>
        ///     Input that was parsed
        /// </summary>
        public string Input { get; set; } = "";

        /// <summary>
        ///     ID of the message that this reservation was created for
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        ///     ID of the discord user that made the post
        /// </summary>
        public ulong PosterUserId { get; set; }

        /// <summary>
        ///     The parsed reservation. May or may not be valid
        /// </summary>
        public PsbReservation Reservation { get; set; } = new();

        /// <summary>
        ///     What errors occured during parsing
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        ///     If there were any contact errors (such as an outfit not having a rep on the reservation), these are here
        /// </summary>
        public List<string> ContactErrors { get; set; } = new();

        /// <summary>
        ///     Debug text about how the when was parsed
        /// </summary>
        public string TimeFeedback { get; set; } = "";

        /// <summary>
        ///     Full debug text
        /// </summary>
        public string DebugText { get; set; } = "";

        /// <summary>
        ///     Link to the message
        /// </summary>
        public string MessageLink { get; set; } = "";

        public PsbParsedReservationMetadata Metadata { get; set; } = new();

        /// <summary>
        ///     Turn the information of a reservation into a Discord embed
        /// </summary>
        /// <param name="debug">Will debug information be included or not?</param>
        /// <returns>
        ///     A <see cref="DiscordEmbedBuilder"/> that can be attached to a message to represent a reservation
        /// </returns>
        public DiscordEmbedBuilder Build(bool debug) {
            DiscordEmbedBuilder builder = new();
            builder.Title = $"Reservation";
            builder.Url = MessageLink;

            builder.Description = $"Posted by <@{PosterUserId}>\nLink: {MessageLink}\n\n";

            if (Errors.Count > 0) {
                builder.Color = DiscordColor.Red;
                builder.Description += $"**Reservation parsed with errors:**\n{string.Join("\n", Errors.Select(iter => $"- {iter}"))}";
            } else if (Errors.Count == 0 && ContactErrors.Count > 0) {
                builder.Color = DiscordColor.Yellow;
                builder.Description += $"**Reservation parsed with warnings:**\n{string.Join("\n", ContactErrors.Select(iter => $"- {iter}"))}";
            } else {
                builder.Color = DiscordColor.Green;
                builder.Description += $"Reservation parsed successfully, but this does not mean the information is correct!\n**Double check it!**";
            }

            if (Reservation.Outfits.Count > 0) {
                string v = "";

                // include what rep from each outfit is represented
                foreach (string outfit in Reservation.Outfits) {
                    List<PsbOvOContact> contacts = Reservation.Contacts.Where(iter => iter.IsRepFor(outfit)).ToList();
                    if (contacts.Count == 0) {
                        v += $"- {outfit} (no reps given!)\n";

                    } else {
                        v += $"- {outfit} ({string.Join(" & ", contacts.Select(iter => $"{iter.Name}/<@{iter.DiscordID}>"))})\n";
                    }
                }

                builder.AddField("Groups in reservation", v);
            } else {
                builder.AddField("Groups in reservation", "**missing**");
            }

            builder.AddField("Accounts requested", $"{Reservation.Accounts}{(Reservation.Accounts >= 48 ? " **Requires OvO admin approval!**" : "")}");

            builder.AddField("Start time", $"`{Reservation.Start:u}` ({Reservation.Start.GetDiscordFullTimestamp()} - {Reservation.Start.GetDiscordRelativeTimestamp()})");
            builder.AddField("End time", $"`{Reservation.End:u}` ({Reservation.End.GetDiscordFullTimestamp()} - {Reservation.End.GetDiscordRelativeTimestamp()})");

            if (Reservation.Bases.Count > 0) {
                List<string> s = new();

                foreach (PsbBaseBooking booking in Reservation.Bases) {
                    if (booking.ZoneID != null) {
                        s.Add($"{booking.GetDiscordPretty()} - **Requires OvO admin approval!**");
                    } else {
                        s.Add($"{booking.GetDiscordPretty()}");
                    }
                }

                builder.AddField("Bases", string.Join("\n", s));
            }

            if (Reservation.Details.Length > 0) {
                builder.AddField("Details", Reservation.Details);
            }

            if (debug == true) {
                builder.Description += "\n\n" + DebugText;
                if (builder.Description.Length > 1994) {
                    builder.Description = builder.Description[..1994] + "...";
                }
            }

            if (Reservation.Accounts == 0) {
                builder.AddField("Accounts", "no accounts requested");
            } else if (Metadata.AccountSheetApprovedById != null) {
                builder.AddField("Accounts", $"approved by <@{Metadata.AccountSheetApprovedById}>");
            }

            if (Reservation.Bases.Count == 0) {
                builder.AddField("Base booking", $"no bases requested");
            } else if (Metadata.BookingApprovedById != null) {
                builder.AddField("Base booking", $"approved by <@{Metadata.BookingApprovedById}>");
            }

            builder.WithFooter($"Last updated");
            builder.WithTimestamp(DateTime.UtcNow);

            return builder;
        }

    }
}
