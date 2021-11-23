using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Services.Db;

namespace watchtower.Controllers.Api {

    [Route("report")]
    [ApiController]
    public class ReportApiController : ControllerBase {

        private readonly ILogger<ReportApiController> _Logger;

        private readonly IReportDbStore _ReportDb;

        public ReportApiController(ILogger<ReportApiController> logger,
            IReportDbStore reportDb
            ) {

            _Logger = logger;

            _ReportDb = reportDb ?? throw new ArgumentNullException(nameof(reportDb));
        }





    }
}
