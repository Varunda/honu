using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Wrapped;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("api/wrapped")]
    public class WrappedApiController : ApiControllerBase {

        private readonly ILogger<WrappedApiController> _Logger;

        private readonly WrappedDbStore _WrappedDb;
        private readonly CharacterRepository _CharacterRepository;
        private readonly WrappedGenerationQueue _Queue;

        public WrappedApiController(ILogger<WrappedApiController> logger,
            WrappedDbStore wrappedDb, CharacterRepository characterRepository,
            WrappedGenerationQueue queue) {

            _Logger = logger;

            _WrappedDb = wrappedDb;
            _CharacterRepository = characterRepository;
            _Queue = queue;
        }

        /// <summary>
        ///     Get a specific <see cref="WrappedEntry"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the wrapped entry</param>
        /// <response code="200">
        ///     The <see cref="WrappedEntry"/> with <see cref="WrappedEntry.ID"/> of <paramref name="ID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="WrappedEntry"/> with <see cref="WrappedEntry.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpGet("{ID}")]
        public async Task<ApiResponse<WrappedEntry>> GetByID(Guid ID) {
            WrappedEntry? entry = await _WrappedDb.GetByID(ID);

            if (entry == null) {
                return ApiNoContent<WrappedEntry>();
            }

            return ApiOk(entry);
        }

        /// <summary>
        ///     Create a new <see cref="WrappedEntry"/>, insert it into the queue,
        ///     from a list of IDs, returning the ID of the entry
        /// </summary>
        /// <param name="IDs">IDs of the characters to include. Only arrays of length 1 to 16 are allowed</param>
        /// <response code="200">
        ///     The response will contain the <see cref="WrappedEntry.ID"/> of the entry that was just created
        /// </response>
        /// <response code="400">
        ///     Parameter validation failed for one of the following reasons:
        ///     <ul>
        ///         <li><paramref name="IDs"/> was an array length less than 1</li>
        ///         <li><paramref name="IDs"/> was an array length greater than 16</li>
        ///         <li>At least one of the IDs passed in <paramref name="IDs"/> does not exist</li>
        ///     </ul>
        /// </response>
        [HttpPost]
        public async Task<ApiResponse<Guid>> Insert([FromQuery] List<string> IDs) {
            if (IDs.Count < 1) {
                return ApiBadRequest<Guid>($"{nameof(IDs)} must include at least one value");
            }

            if (IDs.Count > 16) {
                return ApiBadRequest<Guid>($"{nameof(IDs)} cannot have more than 16 values");
            }

            // validate the characters do exist
            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC);

            List<string> notFoundIDs = new(IDs);
            foreach (PsCharacter c in chars) {
                notFoundIDs.Remove(c.ID);
            }

            if (notFoundIDs.Count > 0) {
                return ApiBadRequest<Guid>($"failed to find characters: {string.Join(", ", notFoundIDs)}");
            }

            WrappedEntry entry = new();
            entry.ID = Guid.NewGuid();
            entry.InputCharacterIDs = IDs;
            entry.Status = WrappedEntryStatus.NOT_STARTED;

            await _WrappedDb.Insert(entry);

            _Queue.Queue(entry);

            return ApiOk(entry.ID);
        }

    }
}
