using System.Collections.Generic;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Wrapped around the Google Drive API for a permission on a file 
    /// </summary>
    public class PsbDrivePermission {

        public PsbDrivePermission(string parentID) {
            ParentFileID = parentID;
        }

        /// <summary>
        ///     ID of the <see cref="PsbDriveFile"/> this permission is for
        /// </summary>
        public readonly string ParentFileID;

        /// <summary>
        ///     Unique ID
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        ///     Email of the user this permission refers to
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        ///     Pretty value of the permission
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        ///     Role granted by the permission. Values are: owner, organized, fileOrganizer, writer, commenter, reader
        /// </summary>
        public string Role { get; set; } = "";

        /// <summary>
        ///     The type of grantee. Values are: user, group, domain, anyone
        /// </summary>
        public string Type { get; set; } = "";

        public bool? PendingOwnership { get; set; }

        /// <summary>
        ///     Details on a shared drive item
        /// </summary>
        public List<PsbDrivePermissionDetail> Details { get; set; } = new();

    }

    /// <summary>
    ///     Details about a permission
    /// </summary>
    public class PsbDrivePermissionDetail {

        /// <summary>
        ///     Permission type for this user. Current values are: "file", "member"
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        ///     Primary role for this user
        /// </summary>
        public string Role { get; set; } = "";

        /// <summary>
        ///     ID of the item from which this permission is inherited
        /// </summary>
        public string? InheritedFrom { get; set; }

        /// <summary>
        ///     Whether this permissions is inherited
        /// </summary>
        public bool Inherited { get; set; }
    
    }




}
