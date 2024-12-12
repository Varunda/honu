using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Wrapped;
using watchtower.Services;
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
        private readonly HonuMetadataRepository _MetadataRepository;
        private readonly HttpUtilService _HttpUtil;
        private readonly IHttpContextAccessor _HttpContext;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "Wrapped.GenerateThrottle.{0}"; // {0} => IP

        /// <summary>
        ///     List of IPs that are not rate limited
        /// </summary>
        private readonly HashSet<string> _WhitelistedIps = new(new string[] {
            // "127.0.0.1", // localhost
            "::1", // localhost again (IPv6)
        });

        public WrappedApiController(ILogger<WrappedApiController> logger,
            WrappedDbStore wrappedDb, CharacterRepository characterRepository,
            WrappedGenerationQueue queue, HonuMetadataRepository metadataRepository,
            HttpUtilService httpUtil, IHttpContextAccessor httpContext, IMemoryCache cache) {

            _Logger = logger;

            _WrappedDb = wrappedDb;
            _CharacterRepository = characterRepository;
            _Queue = queue;
            _MetadataRepository = metadataRepository;
            _HttpUtil = httpUtil;
            _HttpContext = httpContext;
            _Cache = cache;
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
        ///     Get if wrapped is currently enabled
        /// </summary>
        /// <response code="200">
        ///     The response will contain a boolean value indicating if the server is currently accepting new Wrapped entries
        /// </response>
        [HttpGet("enabled")]
        public async Task<ApiResponse<bool>> IsEnabled() {
            bool enabled = await _MetadataRepository.GetAsBoolean("wrapped.enabled") ?? true;
            return ApiOk(enabled);
        }

        /// <summary>
        ///     Create a new <see cref="WrappedEntry"/>, insert it into the queue,
        ///     from a list of IDs, returning the ID of the entry
        /// </summary>
        /// <param name="IDs">IDs of the characters to include. Only arrays of length 1 to 16 are allowed</param>
        /// <param name="year">what year to make the events for</param>
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
        public async Task<ApiResponse<Guid>> Insert([FromQuery] List<string> IDs, int? year = null) {
            bool enabled = await _MetadataRepository.GetAsBoolean("wrapped.enabled") ?? true;
            if (enabled == false) {
                return ApiBadRequest<Guid>($"Wrapped is currently not enabled. usually this means data is being prepared");
            }

            string? ip = _HttpUtil.GetHttpRemoteIp(_HttpContext.HttpContext);
            if (ip == null) {
                _Logger.LogWarning($"missing IP! [Connection.Id={_HttpContext.HttpContext?.Connection.Id}]");
                return ApiBadRequest<Guid>($"no IP given");
            }

            _Logger.LogDebug($"new wrapped generation requested [IP={ip}] [IDs (count)={IDs.Count}]");

            if (_Queue.Count() > 100) {
                return ApiInternalError<Guid>($"Wrapped currently has too many entries");
            }

            if (IDs.Count < 1) {
                return ApiBadRequest<Guid>($"{nameof(IDs)} must include at least one value");
            }

            if (IDs.Count > 16) {
                return ApiBadRequest<Guid>($"{nameof(IDs)} cannot have more than 16 values");
            }

            if (year != null) {
                if (year.Value < 2022 || year.Value > (DateTime.UtcNow.Year - 1)) {
                    return ApiBadRequest<Guid>($"{nameof(year)} must be between 2022 and {DateTime.UtcNow.Year - 1}");
                }
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

            if (_WhitelistedIps.Contains(ip)) {
                _Logger.LogInformation($"IP is in whitelist [IP={ip}]");
            } else {
                string cacheKey = string.Format(CACHE_KEY, ip);
                if (_Cache.TryGetValue(cacheKey, out int requestCount) == false) {
                    requestCount = 1;
                }

                if (requestCount >= 5) {
                    _Logger.LogInformation($"throttled Wrapped creation [IP={ip}] [requestCount={requestCount}]");
                    return new ApiResponse<Guid>(429, "you are being rate limited! please wait!");
                }

                requestCount += 1;
                _Logger.LogDebug($"updating request count [IP={ip}] [requestCount={requestCount}]");
                _Cache.Set(cacheKey, requestCount, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            WrappedEntry entry = new();
            entry.ID = Guid.NewGuid();
            entry.InputCharacterIDs = IDs;
            entry.Status = WrappedEntryStatus.NOT_STARTED;
            entry.CreatedAt = DateTime.UtcNow;
            entry.Timestamp = DateTime.UtcNow;
            if (year != null) {
                _Logger.LogDebug($"year is manually set for wrapped [ID={entry.ID}] [year={year}]");
                // why +1?
                // originally, honu assumed that if the timestamp was in 2024, then data for 2023 was to be loaded
                // now that honu accepts a year as an input, i don't want to go back and fix all the old data
                // to match this fixed offset, so instead i'm gonna make the probably bad move to keep this offset
                // and hopefully i don't forget i do this in the future :^)
                entry.Timestamp = new DateTime(year.Value + 1, 6, 6);
            }

            await _WrappedDb.Insert(entry);

            _Queue.Queue(entry);

            return ApiOk(entry.ID);
        }

    }
}
