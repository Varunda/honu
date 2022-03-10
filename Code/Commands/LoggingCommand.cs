using System;
using watchtower.Commands;

namespace watchtower.Code.Commands {

    [Command]
    public class LoggingCommand {

        public LoggingCommand(IServiceProvider services) {

        }

        public void KilledTeamIDFixer() {
            Logging.KillerTeamIDFixer = !Logging.KillerTeamIDFixer;
        }

    }
}
