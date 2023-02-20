using System;
using watchtower.Models.Census;

namespace watchtower.Models.Db {

    /// <summary>
    ///     What dictates if an <see cref="AlertEndSubscription"/> will be sent
    /// </summary>
    public enum AlertEndSubscriptionSourceType {

        /// <summary>
        ///     Unknown, default
        /// </summary>
        UNKNOWN,

        /// <summary>
        ///     The <see cref="AlertEndSubscription"/> will be sent if a specific character participates in an alert
        /// </summary>
        CHARACTER,

        /// <summary>
        ///     The <see cref="AlertEndSubscription"/> will be sent if an outfit participates in an alert
        /// </summary>
        OUTFIT,

        /// <summary>
        ///     The <see cref="AlertEndSubscription"/> will be sent when an alert ends on a specific world
        /// </summary>
        WORLD

    }

    /// <summary>
    ///     When an alert ends (and finishes generation), all alert end subscriptions will generate a message.
    ///     See remarks for more info
    /// </summary>
    /// <remarks>
    ///     An alert end subscription can be for:
    ///     - An outfit
    ///         - Message targets:
    ///             - Sent to a DM
    ///             - Sent to a channel within a guild
    ///         - Conditions specific to outfit:
    ///             - Number of characters in the outfit who participated in the alert
    ///     - A character
    ///         - Message targets:
    ///             - Sent to a DM
    ///         - Conditions specific to character:
    ///             - How many seconds the character participated in the alert for
    ///     - A world
    ///         - Message targets:
    ///             - Sent to a DM
    ///             - Sent to a channel within a guild
    ///     
    ///     There are some global conditions as well that apply to any alert:
    ///         - How many unique characters participated in the alert
    /// </remarks>
    public class AlertEndSubscription {

        /// <summary>
        ///     Unique ID of the subscription
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        ///     Discord ID of the user who created the subscription
        /// </summary>
        public ulong CreatedByID { get; set; }

        /// <summary>
        ///     When the subscription was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     What type of subscription will this be sent for?
        ///     <ul>
        ///         <li><see cref="AlertEndSubscriptionSourceType.OUTFIT"/>: sent if an outfit participates in an alert</li>
        ///         <li><see cref="AlertEndSubscriptionSourceType.CHARACTER"/>: sent if a character participates in an alert</li>
        ///         <li><see cref="AlertEndSubscriptionSourceType.WORLD"/>: sent when an alert ends in a world/server</li>
        ///         <li><see cref="AlertEndSubscriptionSourceType.UNKNOWN"/>: the <see cref="AlertEndSubscription"/> is in an invalid state</li>
        ///     </ul>
        /// </summary>
        public AlertEndSubscriptionSourceType SourceType {
            get {
                if (OutfitID != null) {
                    return AlertEndSubscriptionSourceType.OUTFIT;
                }
                if (CharacterID != null) {
                    return AlertEndSubscriptionSourceType.CHARACTER;
                }
                if (WorldID != null) {
                    return AlertEndSubscriptionSourceType.WORLD;
                }
                return AlertEndSubscriptionSourceType.UNKNOWN;
            }
        }

        /// <summary>
        ///     If the alert end subscription is for an outfit, what guild ID will the message be sent to?
        ///     <see cref="ChannelID"/> cannot be null if this is not null
        /// </summary>
        public ulong? GuildID { get; set; }

        /// <summary>
        ///     If the alert end subscription is for an outfit, what channel ID will the message be sent to?
        ///     <see cref="GuildID"/> cannot be null if this is not null
        /// </summary>
        public ulong? ChannelID { get; set; }

        /// <summary>
        ///     If set, this alert end subscription will be sent to a channel or DM if an outfit has members who
        ///     participated in the alert. Set <see cref="OutfitCharacterMinimum"/> for how many characters must 
        ///     have participated to send the alert
        /// </summary>
        public string? OutfitID { get; set; }

        /// <summary>
        ///     If set, this alert end subscription will be sent to a channel or DM when an alert ends on a world/server.
        ///     Set <see cref="WorldCharacterMinimum"/> for how many characters must have participated to send the notification
        /// </summary>
        public short? WorldID { get; set; }

        /// <summary>
        ///     If set, this alert end subscription will be sent to a DM when an alert ends and a player
        ///     participated in it. See <see cref="CharacterMinimumSeconds"/> for how many seconds a character must have 
        ///     participated in the alert to send the notification
        /// </summary>
        public string? CharacterID { get; set; }

        /// <summary>
        ///     If this alert end subscription is sent due to an alert ending in a world,
        ///     how many unique characters must have participated for the notification to be sent
        /// </summary>
        public long WorldCharacterMinimum { get; set; }

        /// <summary>
        ///     If this alert end subscription is sent due to an alert ending and an outfit participated,
        ///     how many unique characters must have participated for the notification to be sent
        /// </summary>
        public long OutfitCharacterMinimum { get; set; }

        /// <summary>
        ///     If this alert end subscription is sent due to a single user having subscribed,
        ///     how many seconds must that character have been online for the notification to be sent
        /// </summary>
        public long CharacterMinimumSeconds { get; set; }

    }

    /// <summary>
    ///     Where this alert end subscription will be send
    /// </summary>
    public enum AlertEndSubscriptionTargetType {

        /// <summary>
        ///     Unknown, the default
        /// </summary>
        UNKNOWN,

        /// <summary>
        ///     This alert end subscription will be sent to a user
        /// </summary>
        USER,

        /// <summary>
        ///     This alert end subscription will be sent to a channel within a guild
        /// </summary>
        CHANNEL

    }

    /// <summary>
    ///     Information about where/who the Discord message will be sent to
    /// </summary>
    public class AlertEndSubscriptionTarget {

        public AlertEndSubscriptionTarget(AlertEndSubscription sub) {
            if (sub.SourceType == AlertEndSubscriptionSourceType.UNKNOWN) {
                throw new ArgumentException($"cannot create {nameof(AlertEndSubscriptionTarget)}: "
                    + $"{nameof(AlertEndSubscription)} is of SourceType UNKNOWN");
            } else if (sub.SourceType == AlertEndSubscriptionSourceType.WORLD || sub.SourceType == AlertEndSubscriptionSourceType.OUTFIT) {
                // world subs can be sent to a channel or DM
                // outfit subs can be sent to a channel or DM

                if (sub.GuildID == null && sub.ChannelID != null) {
                    throw new ArgumentException($"cannot create {nameof(AlertEndSubscriptionTarget)}: "
                        + $"cannot specify a {nameof(AlertEndSubscription.ChannelID)} ({sub.ChannelID}) without a {nameof(AlertEndSubscription.GuildID)} (null)");
                }

                if (sub.GuildID != null && sub.ChannelID == null) {
                    throw new ArgumentException($"cannot create {nameof(AlertEndSubscriptionTarget)}: "
                        + $"cannot specify a {nameof(AlertEndSubscription.GuildID)} ({sub.GuildID}) without a {nameof(AlertEndSubscription.ChannelID)} (null)");
                }

                if (sub.GuildID != null && sub.ChannelID != null) {
                    ChannelID = sub.ChannelID.Value;
                    GuildID = sub.GuildID.Value;
                    TargetType = AlertEndSubscriptionTargetType.CHANNEL;
                } else {
                    UserID = sub.CreatedByID;
                    TargetType = AlertEndSubscriptionTargetType.USER;
                }
            } else if (sub.SourceType == AlertEndSubscriptionSourceType.CHARACTER) { // character subs can only be sent in DM
                UserID = sub.CreatedByID;
                TargetType = AlertEndSubscriptionTargetType.USER;
            } else {
                throw new ArgumentException($"cannot create {nameof(AlertEndSubscriptionTarget)}: "
                    + $"unchecked value of {nameof(AlertEndSubscription.SourceType)} {sub.SourceType}");
            }
        }

        /// <summary>
        ///     where this message will be sent
        /// </summary>
        public readonly AlertEndSubscriptionTargetType TargetType = AlertEndSubscriptionTargetType.UNKNOWN;

        /// <summary>
        ///     ID of the Discord user the subscription will be sent for
        /// </summary>
        public ulong UserID { get; }

        /// <summary>
        ///     ID of the Discord channel the subscription will be sent to
        /// </summary>
        public ulong ChannelID { get; }

        /// <summary>
        ///     ID of the Discord guild the channel of <see cref="ChannelID"/> is in
        /// </summary>
        public ulong GuildID { get; }

    }

    /// <summary>
    ///     A <see cref="AlertEndSubscription"/>, but with only the information about outfits
    /// </summary>
    public class OutfitAlertEndSubscription {

        /// <summary>
        ///     Create a new <see cref="OutfitAlertEndSubscription"/> from a <see cref="AlertEndSubscription"/>
        /// </summary>
        /// <param name="sub">Subscription that contains the parameters</param>
        /// <exception cref="ArgumentException">If <paramref name="sub"/> contained invalid data that prevents this from being created</exception>
        public OutfitAlertEndSubscription(AlertEndSubscription sub) {
            if (sub.OutfitID == null) {
                throw new ArgumentException($"cannot create {nameof(OutfitAlertEndSubscription)} {sub.ID}: {nameof(AlertEndSubscription.OutfitID)} is null");
            }

            Subscription = sub;
        }

        /// <summary>
        ///     The <see cref="AlertEndSubscription"/> this <see cref="OutfitAlertEndSubscription"/> was created from
        /// </summary>
        public AlertEndSubscription Subscription { get; }

        /// <summary>
        ///     Unique ID of the <see cref="AlertEndSubscription"/>
        /// </summary>
        public long ID => Subscription.ID;

        /// <summary>
        ///     ID of the outfit this subscription is for
        /// </summary>
        public string OutfitID => Subscription.OutfitID!;

        /// <summary>
        ///     How many unique characters must have participated in the alert to send the subscription
        /// </summary>
        public long WorldCharacterMinimum => Subscription.WorldCharacterMinimum;

        /// <summary>
        ///     How many characters in the outfit with ID <see cref="OutfitID"/>
        ///     must be online to send this subscription
        /// </summary>
        public long OutfitCharacterMinimum => Subscription.OutfitCharacterMinimum;

    }

    /// <summary>
    ///     A <see cref="AlertEndSubscription"/>, but only with fields relevant to subscriptions for characters
    /// </summary>
    public class CharacterAlertEndSubscription {

        /// <summary>
        ///     Create a new <see cref="CharacterAlertEndSubscription"/> from a <see cref="AlertEndSubscription"/>
        /// </summary>
        /// <param name="sub">Subscription that contains the parameters</param>
        /// <exception cref="ArgumentException">If <paramref name="sub"/> contained invalid data that prevents this from being created</exception>
        public CharacterAlertEndSubscription(AlertEndSubscription sub) {
            if (sub.CharacterID == null) {
                throw new ArgumentException($"cannot create {nameof(CharacterAlertEndSubscription)} {sub.ID}: {nameof(AlertEndSubscription.CharacterID)} is null");
            }

            Subscription = sub;
        }

        public AlertEndSubscription Subscription { get; }

        /// <summary>
        ///     unique ID of the <see cref="AlertEndSubscription"/>
        /// </summary>
        public long ID => Subscription.ID;

        /// <summary>
        ///     ID of the <see cref="PsCharacter"/> this subscription is for
        /// </summary>
        public string CharacterID => Subscription.CharacterID!;

        /// <summary>
        ///     How many unique characters must have participated to send this subscription
        /// </summary>
        public long WorldCharacterMinimum => Subscription.WorldCharacterMinimum;

        /// <summary>
        ///     How many seconds the character of <see cref="CharacterID"/> must have participated
        ///     in the alert to send the subscription
        /// </summary>
        public long CharacterMinimumSeconds => Subscription.CharacterMinimumSeconds;

    }

    /// <summary>
    ///     A <see cref="AlertEndSubscription"/>, but only with fields relevant to subscriptions for worlds
    /// </summary>
    public class WorldAlertEndSubscription {

        /// <summary>
        ///     Create a new <see cref="WorldAlertEndSubscription"/> from a <see cref="AlertEndSubscription"/>
        /// </summary>
        /// <param name="sub">Subscription that contains the parameters</param>
        /// <exception cref="ArgumentException">If <paramref name="sub"/> contained invalid data that prevents this from being created</exception>
        public WorldAlertEndSubscription(AlertEndSubscription sub) {
            if (sub.WorldID == null) {
                throw new ArgumentException($"cannot create {nameof(WorldAlertEndSubscription)} {sub.ID}: {nameof(AlertEndSubscription.WorldID)} is null");
            }

            Subscription = sub;
        }

        public AlertEndSubscription Subscription { get; }

        /// <summary>
        ///     unique ID of the <see cref="AlertEndSubscription"/>
        /// </summary>
        public long ID => Subscription.ID;

        /// <summary>
        ///     ID of the world to notify when an alert ends
        /// </summary>
        public short WorldID => Subscription.WorldID!.Value;

        /// <summary>
        ///     How many unique characters must have participated to send this subscription
        /// </summary>
        public long WorldCharacterMinimum => Subscription.WorldCharacterMinimum;

    }

}
