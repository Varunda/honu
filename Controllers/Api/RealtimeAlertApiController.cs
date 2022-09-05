using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.RealtimeAlert;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/realtime-alert")]
    public class RealtimeAlertApiController : ApiControllerBase {

        private readonly ILogger<RealtimeAlertApiController> _Logger;
        private readonly RealtimeAlertRepository _RealtimeAlertRepository;

        public RealtimeAlertApiController(ILogger<RealtimeAlertApiController> logger,
            RealtimeAlertRepository realtimeAlertRepository) {

            _Logger = logger;
            _RealtimeAlertRepository = realtimeAlertRepository;
        }

        /// <summary>
        ///     Get a list of all realtime alerts
        /// </summary>
        /// <response code="200">
        ///     All <see cref="RealtimeAlert"/>s currently happening with all events removed
        /// </response>
        [HttpGet]
        public ApiResponse<List<RealtimeAlert>> GetList() {
            List<RealtimeAlert> alerts = _RealtimeAlertRepository.GetAll();

            List<RealtimeAlert> mini = new(alerts.Count);
            foreach (RealtimeAlert a in alerts) {
                mini.Add(a.AsMini());
            }

            return ApiOk(alerts);
        }

        /// <summary>
        ///     Get the full <see cref="RealtimeAlert"/>, which includes all the events
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="zoneID">ID of the zone</param>
        /// <response code="200">
        ///     The response will contain the alert
        /// </response>
        /// <response code="204">
        ///     No alert found
        /// </response>
        [HttpGet("{worldID}/{zoneID}")]
        public ApiResponse<RealtimeAlert> GetAlert(short worldID, uint zoneID) {
            RealtimeAlert? alert = _RealtimeAlertRepository.Get(worldID, zoneID);

            if (alert == null) {
                return ApiNoContent<RealtimeAlert>();
            }

            return ApiOk(alert);
        }

    }
}
