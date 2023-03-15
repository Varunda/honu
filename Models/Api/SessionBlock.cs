using System.Collections.Generic;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Models.Api {

    public class SessionBlock {

        public string CharacterID { get; set; } = "";

        public List<Session> Sessions { get; set; } = new();

        public List<PsOutfit> Outfits { get; set; } = new();

    }
}
