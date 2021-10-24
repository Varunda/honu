using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class ExpandedKillEventReader : IDataReader<ExpandedKillEvent> {

        private readonly IDataReader<PsItem> _ItemReader;
        private readonly IDataReader<PsCharacter> _CharacterReader;
        private readonly IDataReader<KillEvent> _EventReader;

        public ExpandedKillEventReader(IDataReader<PsItem> itemReader,
            IDataReader<PsCharacter> charReader, IDataReader<KillEvent> evReader) {

            _ItemReader = itemReader ?? throw new ArgumentNullException(nameof(itemReader));
            _CharacterReader = charReader ?? throw new ArgumentNullException(nameof(charReader));
            _EventReader = evReader ?? throw new ArgumentNullException(nameof(evReader));
        }

        public override ExpandedKillEvent ReadEntry(NpgsqlDataReader reader) {
            ExpandedKillEvent ev = new ExpandedKillEvent();

            ev.Event = _EventReader.ReadEntry(reader);

            if (reader.GetNullableString("item_id") != null) {
                ev.Item = _ItemReader.ReadEntry(reader);
            }

            if (reader.GetNullableString("") != null) {
                //ev.Other = _CharacterReader.ReadEntry(reader);
            }

            return ev;
        }
    }
}
