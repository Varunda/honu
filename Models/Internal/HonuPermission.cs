using System.Collections.Generic;

namespace watchtower.Models.Internal {

    /// <summary>
    ///     Permission that can be granted to a <see cref="HonuAccount"/>
    /// </summary>
    public class HonuPermission {

        /// <summary>
        ///     List of all <see cref="HonuPermission"/>s that exist
        /// </summary>
        public readonly static List<HonuPermission> All = new();

        /// <summary>
        ///     Unique ID of the permission
        /// </summary>
        public string ID { get; }

        /// <summary>
        ///     What this permission grants
        /// </summary>
        public string Description { get; }

        public HonuPermission(string ID, string desc) {
            this.ID = ID;
            this.Description = desc;

            HonuPermission.All.Add(this);
        }

        public const string HONU_ACCOUNT_ADMIN = "Honu.Account.Admin";
        public readonly static HonuPermission HonuAccountAdmin = new(HONU_ACCOUNT_ADMIN, "Manage all accounts in Honu");

        public const string HONU_ACCOUNT_GETALL = "Honu.Account.GetAll";
        public readonly static HonuPermission HonuAccountGetAll = new(HONU_ACCOUNT_GETALL, "Get all Honu accounts");

        public const string PSB_NAMED_GET = "PSB.Named.Get";
        public readonly static HonuPermission PsbNamedGet = new(PSB_NAMED_GET, "Get PSB named accounts");

        public const string PSB_NAMED_MANAGE = "PSB.Named.Manage";
        public readonly static HonuPermission PsbNamedCreate = new(PSB_NAMED_MANAGE, "Manage PSB named accounts");

        public const string PSB_PRACTICE_GET = "PSB.Practice.Get";
        public readonly static HonuPermission PsbPracticeGet = new(PSB_PRACTICE_GET, "Get PSB practice accounts");

        public const string PSB_PRACTICE_MANAGE = "PSB.Practice.Manage";
        public readonly static HonuPermission PsbPracticeManage = new(PSB_PRACTICE_MANAGE, "Manage PSB practice accounts");

        public const string ALERT_CREATE = "Alert.Create";
        public readonly static HonuPermission AlertCreate = new(ALERT_CREATE, "Create custom alerts");

    }
}
