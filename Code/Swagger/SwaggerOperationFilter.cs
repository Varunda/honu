using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace watchtower.Code.Swagger {

    /// <summary>
    ///     adds the following information to all endpoints:
    ///     <ul>
    ///         <li>which actions count towards the rate limit</li>
    ///         <li>a 403 response on actions that requires authorization</li>
    ///         <li>a 500 response that says all endpoints can fail</li>
    ///     </ul>
    /// </summary>
    public class SwaggerOperationFilter : IOperationFilter {

        public void Apply(OpenApiOperation operation, OperationFilterContext context) {
            Apply403Response(operation, context);
            Apply500Response(operation, context);
            ApplyRateLimiting(operation, context);
        }

        /// <summary>
        ///     add to the description of each endpoint if the endpoint counts towards rate limiting or not
        /// </summary>
        private void ApplyRateLimiting(OpenApiOperation operation, OperationFilterContext ctx) {
            DisableRateLimitingAttribute? attr = ctx.MethodInfo.GetCustomAttribute<DisableRateLimitingAttribute>();

            if (!string.IsNullOrEmpty(operation.Description)) {
                operation.Description += $"<br/><br/>";
            }

            if (attr != null) {
                operation.Description += "requests to this endpoint do not count towards rate limiting";
            } else {
                operation.Description += "requests to this endpoint count towards rate limiting";
            }
        }

        /// <summary>
        ///     add a 403 response to all endpoints that require permission from an authorized account
        /// </summary>
        private void Apply403Response(OpenApiOperation operation, OperationFilterContext ctx) {
            PermissionNeededAttribute? perms = ctx.MethodInfo.GetCustomAttribute<PermissionNeededAttribute>();

            if (perms == null) {
                return;
            }

            if ((perms.Arguments?.Length ?? 0) < 1) {
                return;
            }

            string permNeeded = "";
            if (perms.Arguments![0] is string[] p) {
                permNeeded = string.Join(", ", p);
            }

            string str = $"using this endpoint requires a honu account and one of the following permissions: <code>{permNeeded}</code>";

            if (operation.Responses.TryGetValue("403", out OpenApiResponse? response)) {
                response.Description += "<br/><br/>" + str;
            } else {
                operation.Responses.Add("403", new OpenApiResponse { Description = str });
            }
        }

        /// <summary>
        ///     add a 500 response to all endpoints
        /// </summary>
        private void Apply500Response(OpenApiOperation operation, OperationFilterContext ctx) {
            string responseStr = "an exception was thrown during the processing of this request. the exception was logged";

            // If any additional info about this has been added, use that and the default string
            if (operation.Responses.TryGetValue("500", out OpenApiResponse? response)) {
                response.Description += "<br/><br/>" + responseStr;
            } else {
                operation.Responses.Add("500", new OpenApiResponse { Description = responseStr });
            }
        }

    }
}
