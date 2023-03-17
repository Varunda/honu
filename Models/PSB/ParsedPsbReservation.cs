using DSharpPlus.Entities;
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

        public string MessageLink { get; set; } = "";

        public PsbParsedReservationMetadata Metadata { get; set; } = new();

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
                builder.Description += $"Reservation parsed successfully, but this does not mean the information is correct! Double check it!";
            }

            if (Reservation.Outfits.Count > 0) {
                builder.AddField("Groups in reservation", string.Join(", ", Reservation.Outfits));
            } else {
                builder.AddField("Groups in reservation", "**missing**");
            }
            builder.AddField("Accounts requested", $"{Reservation.Accounts}");
            builder.AddField("Start time", $"`{Reservation.Start:u}` ({Reservation.Start.GetDiscordFullTimestamp()} - {Reservation.Start.GetDiscordRelativeTimestamp()})");
            builder.AddField("End time", $"`{Reservation.End:u}` ({Reservation.End.GetDiscordFullTimestamp()} - {Reservation.End.GetDiscordRelativeTimestamp()})");
            if (Reservation.Bases.Count > 0) {
                builder.AddField("Bases", string.Join("\n", Reservation.Bases.Select(iter => iter.GetDiscordPretty())));
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

            if (Metadata.AccountSheetApprovedById != null) {
                builder.AddField("Accounts approved by", $"<@{Metadata.AccountSheetApprovedById}>");
            }

            if (Metadata.BookingApprovedById != null) {
                builder.AddField("Base booking approved by", $"<@{Metadata.BookingApprovedById}>");
            }

            return builder;
        }

    }
}
