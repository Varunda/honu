using watchtower.Models.Census;

namespace watchtower.Models.Api {
    public class ExpandedAlert {

        public PsAlert Alert { get; set; } = new();

        public PsMetagameEvent? MetagameEvent { get; set; } = null;

    }
}
