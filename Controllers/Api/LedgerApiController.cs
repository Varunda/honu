using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/ledger")]
    public class LedgerApiController : ControllerBase {

        private readonly ILogger<LedgerApiController> _Logger;

        private readonly IFacilityCollection _FacilityCollection;
        private readonly IFacilityDbStore _FacilityDb;
        private readonly IFacilityControlDbStore _ControlDb;

        public LedgerApiController(ILogger<LedgerApiController> logger,
            IFacilityCollection facilityCollection, IFacilityControlDbStore controlDb,
            IFacilityDbStore facDb) {

            _Logger = logger;

            _FacilityCollection = facilityCollection ?? throw new ArgumentNullException(nameof(facilityCollection));
            _FacilityDb = facDb ?? throw new ArgumentNullException(nameof(facDb));
            _ControlDb = controlDb ?? throw new ArgumentNullException(nameof(controlDb));
        }

        [HttpGet]
        public async Task<ActionResult<List<FacilityControlEntry>>> GetAll(
                [FromQuery] uint? zoneID = null,
                [FromQuery] List<short>? worldID = null,
                [FromQuery] int? playerThreshold = null,
                [FromQuery] long? periodStart = null,
                [FromQuery] long? periodEnd = null,
                [FromQuery] int? unstableState = null) {

            FacilityControlOptions parameters = new();
            parameters.ZoneID = zoneID;
            parameters.WorldIDs = worldID ?? new List<short>();
            parameters.PlayerThreshold = playerThreshold ?? 12;

            if (periodStart != null) {
                parameters.PeriodStart = DateTimeOffset.FromUnixTimeSeconds(periodStart.Value).UtcDateTime;
            }
            if (periodEnd != null) {
                parameters.PeriodEnd = DateTimeOffset.FromUnixTimeSeconds(periodEnd.Value).UtcDateTime;
            }

            if (unstableState != null) {
                if (Enum.IsDefined(typeof(UnstableState), unstableState.Value) == false) {
                    return BadRequest($"{nameof(unstableState)} is an invalid value");
                }

                parameters.UnstableState = (UnstableState)unstableState.Value;
            } else {
                parameters.UnstableState = UnstableState.UNLOCKED;
            }

            List<FacilityControlDbEntry> entries = await _ControlDb.Get(parameters);

            List<PsFacility> facilities = await _FacilityDb.GetAll();

            List<FacilityControlEntry> ret = new List<FacilityControlEntry>();

            foreach (FacilityControlDbEntry entry in entries) {
                PsFacility? facility = facilities.FirstOrDefault(iter => iter.FacilityID == entry.FacilityID);

                if (facility == null) {
                    continue;
                }

                FacilityControlEntry elem = new();
                elem.FacilityID = entry.FacilityID;
                elem.Captured = entry.Captured;
                elem.CaptureAverage = entry.CaptureAverage;
                elem.Defended = entry.Defended;
                elem.DefenseAverage = entry.DefenseAverage;
                elem.TotalAverage = entry.TotalAverage;

                elem.FacilityName = facility?.Name ?? $"<missing facility {elem.FacilityID}>";
                elem.TypeID = facility?.TypeID ?? 0;
                elem.TypeName = facility?.TypeName ?? "";
                elem.ZoneID = facility?.ZoneID ?? 0;

                ret.Add(elem);
            }

            return Ok(ret);
        }

    }
}
