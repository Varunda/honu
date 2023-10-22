using System;
using System.Collections.Generic;

namespace watchtower.Models.Wrapped {

    public class WrappedSimple {

        public Guid ID { get; set; }

        public string Display { get; set; } = "";

        public List<WrappedSimpleEntry> PlayerMostKilled { get; set; } = new();

        public List<WrappedSimpleEntry> PlayerMostDeaths { get; set; } = new();

        public List<WrappedSimpleEntry> PlayerHighestKD { get; set; } = new();

        public List<WrappedSimpleEntry> PlayerMostHealed { get; set; } = new();

        public List<WrappedSimpleEntry> PlayerMostRevived { get; set; } = new();

    }
}
