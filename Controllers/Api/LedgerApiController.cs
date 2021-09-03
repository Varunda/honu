using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IFacilityControlDbStore _ControlDb;

        public LedgerApiController(ILogger<LedgerApiController> logger,
            IFacilityCollection facilityCollection, IFacilityControlDbStore controlDb) {

            _Logger = logger;
            _FacilityCollection = facilityCollection ?? throw new ArgumentNullException(nameof(facilityCollection));
            _ControlDb = controlDb ?? throw new ArgumentNullException(nameof(controlDb));
        }

        [HttpGet]
        public async Task<ActionResult<List<FacilityControlEntry>>> GetAll() {
            FacilityControlOptions parameters = new FacilityControlOptions();
            List<FacilityControlDbEntry> entries = await _ControlDb.Get(parameters);

            List<PsFacility> facilities = await _FacilityCollection.GetAll();

            List<FacilityControlEntry> ret = new List<FacilityControlEntry>();

            foreach (FacilityControlDbEntry entry in entries) {
                PsFacility? facility = facilities.FirstOrDefault(iter => iter.FacilityID == entry.FacilityID);

                FacilityControlEntry elem = new();
                elem.FacilityID = entry.FacilityID;
                elem.Captured = entry.Captured;
                elem.CaptureAverage = entry.CaptureAverage;
                elem.Defended = entry.Defended;
                elem.DefenseAverage = entry.DefenseAverage;
                elem.TotalAverage = entry.TotalAverage;

                elem.FacilityName = facility?.Name ?? $"<missing facility {elem.FacilityID}";
                elem.TypeID = facility?.TypeID ?? 0;
                elem.TypeName = facility?.TypeName ?? "";
                elem.ZoneID = facility?.ZoneID ?? 0;

                ret.Add(elem);
            }

            return Ok(ret);
        }

    }
}
