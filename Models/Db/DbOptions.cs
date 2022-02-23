using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class DbOptions {

        public string ServerUrl { get; set; } = "localhost";

        public string DatabaseName { get; set; } = "ps2";

        public string Username { get; set; } = "postgres";

        public string Password { get; set; } = "";

    }
}
