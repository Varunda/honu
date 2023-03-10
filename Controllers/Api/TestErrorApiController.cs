using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/test-error")]
    public class TestErrorApiController : ApiControllerBase {

        private readonly ILogger<TestErrorApiController> _Logger;
        private readonly IDbHelper _DbHelper;

        public TestErrorApiController(ILogger<TestErrorApiController> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     throw an exception due to a DB timeout
        /// </summary>
        /// <response code="500">
        ///     always
        /// </response>
        [HttpGet("db-timeout")]
        public async Task<ApiResponse<int>> DbTimeout() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT pg_sleep(10);
            ");

            cmd.CommandTimeout = 1;

            await cmd.ExecuteNonQueryAsync();

            return ApiOk(0);
        }

    }
}
