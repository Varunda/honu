using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.PSB {

    public class PsbOvOSheetRepository {

        private readonly ILogger<PsbOvOSheetRepository> _Logger;
        private readonly IOptions<PsbDriveSettings> _Options;
        private readonly GDriveRepository _GRepository;


        public PsbOvOSheetRepository(ILogger<PsbOvOSheetRepository> logger,
            IOptions<PsbDriveSettings> options, GDriveRepository gRepository) {

            _Logger = logger;
            _Options = options;
            _GRepository = gRepository;

            if (_Options.Value.TemplateFileId.Trim().Length == 0) {
                throw new ArgumentException($"No {nameof(PsbDriveSettings.TemplateFileId)} provided. Set it using dotnet user-secrets set PsbDrive:TemplateFileId $FILE_ID");
            }
        }

        public async Task<string> CreateSheet(PsbReservation res) {
            string date = $"{res.Start.Date:yyyy-MM-dd}";

            FilesResource.CopyRequest gReq = _GRepository.GetDriveService().Files.Copy(new Google.Apis.Drive.v3.Data.File() {
                Name = $"{date} [{string.Join("/", res.Outfits)}]",
                Parents = new List<string> { _Options.Value.OvORootFolderId }
            }, _Options.Value.TemplateFileId);

            Google.Apis.Drive.v3.Data.File gRes = await gReq.ExecuteAsync();

            List<ValueRange> data = new() {
                // contact emails
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B1:D1",
                    Values = new List<IList<object>> { new List<object> {
                        string.Join(";", res.Contacts.Select(iter => iter.Email))
                    }}
                },

                // date
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B2:D2",
                    Values = new List<IList<object>> { new List<object> {
                        date
                    }}
                },

                // time
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B3:C3",
                    Values = new List<IList<object>> { new List<object> {
                        $"{res.Start:HH:mm}"
                    }}
                }
            };

            await _GRepository.GetSheetService().Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest() {
                Data = data,
                ValueInputOption = "USER_ENTERED"
            }, gRes.Id).ExecuteAsync();

            return gRes.Id;
        }

    }
}
