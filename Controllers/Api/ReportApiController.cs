using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Report;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/report")]
    public class ReportApiController : ApiControllerBase {

        private readonly ILogger<ReportApiController> _Logger;
        private readonly ReportDbStore _ReportDb;
        private readonly ReportRepository _ReportRepository;

        public ReportApiController(ILogger<ReportApiController> logger,
            ReportDbStore reportDb, ReportRepository reportRepo) {

            _Logger = logger;
            _ReportDb = reportDb;
            _ReportRepository = reportRepo;
        }

        /// <summary>
        ///     Get the ID of a <see cref="OutfitReport"/> generated from the generator passed in <paramref name="generator"/>
        /// </summary>
        /// <remarks>
        ///     This is useful in the case where another service is creating the report, and then giving the report. While you could
        ///     display the Base64 encoded URL of the generator as the URL, it can make for some pretty long URLs. Using the ID
        ///     version makes passing the URL around much easier, and a consistent length
        /// </remarks>
        /// <param name="generator">Generator string used</param>
        /// <response code="200">
        ///     The response will contain the <see cref="OutfitReport.ID"/> of the <see cref="OutfitReport"/>
        ///     just parse from <paramref name="generator"/>
        /// </response>
        /// <response code="400">
        ///     A parsing exception happened while parsing <paramref name="generator"/>. The response will contain
        ///     more information about what part of the parse failed
        /// </response>
        [HttpPost]
        public async Task<ApiResponse<Guid>> CreateReport([FromBody] string generator) {
            OutfitReport report = new OutfitReport();
            try {
                report = await _ReportRepository.ParseGenerator(generator);
            } catch (Exception ex) {
                return ApiBadRequest<Guid>($"Failed to parse generator string '{generator}':\n{ex.Message}");
            }

            report.ID = Guid.NewGuid();

            await _ReportDb.Insert(report);

            return ApiOk(report.ID);
        }

    }

}
