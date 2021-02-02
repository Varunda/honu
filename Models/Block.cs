using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class Block {

        public string Name { get; set; } = "";

        public List<BlockEntry> Entries = new List<BlockEntry>();

        public int Total { get; set; } = 0;

    }
}
