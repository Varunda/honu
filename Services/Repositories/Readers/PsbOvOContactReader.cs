using System.Collections.Generic;
using System.Linq;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.Readers {

    public class PsbOvOContactReader : ISheetsReader<PsbOvOContact> {

        public override PsbOvOContact ReadEntry(List<string?> values) {
            PsbOvOContact contact = new();

            contact.Groups = values.GetRequiredString(0).ToLower().Split("/").Select(iter => iter.ToLower()).ToList();
            contact.Name = values.GetRequiredString(1);
            contact.Email = values.GetRequiredString(2);
            contact.DiscordID = values.GetRequiredUInt64(4);
            contact.RepType = values.GetRequiredString(5);
            contact.AccountLimit = values.GetRequiredInt32(6);

            return contact;
        }

    }
}
