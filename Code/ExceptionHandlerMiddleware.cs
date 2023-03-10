using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace watchtower.Code {

    /// <summary>
    ///     middleware to handle uncaught exceptions and format them into the problems format
    /// </summary>
    public class ExceptionHandlerMiddleware {

        private readonly RequestDelegate _Next;

        public ExceptionHandlerMiddleware(RequestDelegate next) {
            _Next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ExceptionHandlerMiddleware> logger) {
            try {
                await _Next(context);
            } catch (Exception ex) {
                ProblemDetails dets = new();
                dets.Type = context.Request.Path.Value;
                dets.Title = $"internal server error: {ex.Message}";

                if (ex is NpgsqlException && ex.InnerException is TimeoutException) {
                    dets.Title = $"internal server error: Database timeout";
                }

                dets.Status = 500;
                dets.Detail = ex.ToString();
                dets.Instance = $"error:{Guid.NewGuid()}";

                logger.LogError($"error {dets.Instance}: {dets.Detail}");

                if (context.Response.HasStarted == true) {
                    return;
                }

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = 500;

                await context.Response.WriteAsync(JToken.FromObject(dets).ToString());
            }
        }

    }
}
