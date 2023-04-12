using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Code.Hubs.Implementations {

    public class WrappedHub : Hub<IWrappedHub> {

        private readonly ILogger<WrappedHub> _Logger;

        public WrappedHub(ILogger<WrappedHub> logger) {
            _Logger = logger;
        }

        /// <summary>
        ///     Update the status of a wrapped entry being generated
        /// </summary>
        /// <param name="ID">ID of the report being updated</param>
        /// <param name="status">new status</param>
        public async Task UpdateStatus(Guid ID, string status) => await Clients.Group($"wrapped-{ID}").UpdateStatus(status);

        /// <summary>
        ///     Send a message to the clients of a wrapped entry
        /// </summary>
        /// <param name="ID">ID of the wrapped entry</param>
        /// <param name="msg">message to be sent</param>
        public async Task SendMessage(Guid ID, string msg) => await Clients.Group($"wrapped-{ID}").SendMessage(msg);

        /// <summary>
        ///     Send a warning message to the clients of a wrapped entry
        /// </summary>
        /// <param name="ID">ID of the wrapped entry</param>
        /// <param name="msg">message to be sent</param>
        public async Task SendWarning(Guid ID, string msg) => await Clients.Group($"wrapped-{ID}").SendWarning(msg);

        /// <summary>
        ///     Send an error message to the clients of a wrapped entry
        /// </summary>
        /// <param name="ID">ID of the wrapped entry</param>
        /// <param name="msg">message to be sent</param>
        public async Task SendError(Guid ID, string msg) => await Clients.Group($"wrapped-{ID}").SendError(msg);

        /// <summary>
        ///     Update the input characters to a wrapped entry
        /// </summary>
        /// <param name="ID">ID of the wrapped entry</param>
        /// <param name="chars">characters that will be included in the wrapped entry</param>
        public async Task UpdateInputCharacters(Guid ID, List<PsCharacter> chars) => await Clients.Group($"wrapped-{ID}").UpdateInputCharacters(chars);

    }
}
