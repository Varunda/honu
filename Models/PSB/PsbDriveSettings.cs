using System.Collections.Generic;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Settings related to managing the PSB drive thru Honu
    /// </summary>
    public class PsbDriveSettings {

        /// <summary>
        ///     Location to the credential json file that is used to perform API methods. Can be absolute or relative
        /// </summary>
        public string CredentialFile { get; set; } = "";

        /// <summary>
        ///     ID of the folder that contains all the practice account sheets
        /// </summary>
        public string PracticeFolderId { get; set; } = "";

        /// <summary>
        ///     Various contact sheets used within PSB
        /// </summary>
        public PsbDriveSettingsContactSheets ContactSheets { get; set; } = new();

    }

    public class PsbDriveSettingsContactSheets {

        /// <summary>
        ///     Google Drive file ID of the practice contact sheet
        /// </summary>
        public string Practice { get; set; } = "";

        /// <summary>
        ///     Google Drive file ID of the OvO contact sheet
        /// </summary>
        public string OVO { get; set; } = "";

    }

}
