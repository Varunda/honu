using Google.Apis.Drive.v3.Data;

namespace watchtower.Models.PSB {

    public class PsbDriveFile {

        /// <summary>
        ///     ID of the GDrive file
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        ///     Name of the GDrive file
        /// </summary>
        public string Name { get; set; } = "";

        public string Kind { get; set; } = "";

        public string DriveId { get; set; } = "";

        public string MimeType { get; set; } = "";

        /// <summary>
        ///     convert a GDrive file into a PSB wrapped one
        /// </summary>
        public static PsbDriveFile Convert(File file) {
            return new PsbDriveFile() {
                ID = file.Id,
                Name = file.Name,
                Kind = file.Kind,
                DriveId = file.DriveId,
                MimeType = file.MimeType
            };
        }

    }
}
