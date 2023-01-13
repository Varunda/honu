using System.Collections.Generic;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.Readers {

    public class PsbPracticeContactReader : ISheetsReader<PsbContact> {

        public override PsbContact ReadEntry(List<string?> values) {
            PsbContact contact = new();

            contact.Email = values.GetRequiredString(2);
            contact.DiscordID = values.GetNullableUInt64(4) ?? 0;
            contact.Name = values.GetNullableString(1) ?? "<no name given>";

            return contact;
        }

    }
}
