using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Models.PSB;
using static watchtower.Models.PSB.PsbGroupContact;

namespace watchtower.Services.Repositories.Readers {

    public class PsbOvOContactReader : ISheetsReader<PsbOvOContact> {

        public override PsbOvOContact ReadEntry(List<string?> values) {
            PsbOvOContact contact = new();

            contact.Groups = values.GetRequiredString(0).ToLower().Split("/").Select(iter => iter.ToLower()).ToList();
            contact.Name = values.GetRequiredString(1);
            contact.Email = values.GetRequiredString(2);
            contact.DiscordID = values.GetRequiredUInt64(4);

            string repType = values.GetRequiredString(5).ToLower().Trim();

            if (repType == "outfit rep") {
                contact.RepType = RepresentativeType.OVO;
            } else if (repType == "community rep") {
                contact.RepType = RepresentativeType.COMMUNITY;
            } else if (repType == "observer user") {
                contact.RepType = RepresentativeType.OBSERVER;
            } else if (repType == "scrim team") {
                contact.RepType = RepresentativeType.COMMUNITY;
            } else {
                throw new ArgumentException($"failed to validate {repType} as a valid {nameof(RepresentativeType)}");
            }

            contact.AccountLimit = values.GetRequiredInt32(6);

            return contact;
        }

    }
}
