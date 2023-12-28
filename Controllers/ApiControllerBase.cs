using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Controllers {

    public abstract class ApiControllerBase : Controller {

        protected ApiResponse<T> ApiOk<T>(T data) => new ApiResponse<T>(200, data);

        protected ApiResponse<T> ApiNoContent<T>() => new ApiResponse<T>(204, null);

        protected ApiResponse<T> ApiBadRequest<T>(string err) => new ApiResponse<T>(400, err);

        protected ApiResponse<T> ApiAuthorize<T>() => new ApiResponse<T>(401, "You must login");

        protected ApiResponse<T> ApiForbidden<T>(params string[] permissionNeeded) => new ApiResponse<T>(403, $"Missing required permission(s): {string.Join(", ", permissionNeeded)}");

        protected ApiResponse<T> ApiNotFound<T>(string err) => new ApiResponse<T>(404, err);

        protected ApiResponse<T> ApiInternalError<T>(Exception ex) => new ApiResponse<T>(500, ex.Message);

        protected ApiResponse<T> ApiInternalError<T>(string err) => new ApiResponse<T>(500, err);

        protected ApiResponse ApiOk() => new ApiResponse(200, null);

        protected ApiResponse ApiBadRequest(string err) => new ApiResponse(400, err);

        protected ApiResponse ApiAuthorize() => new ApiResponse(401, "You must login");

        protected ApiResponse ApiForbidden(params string[] permissionNeeded) => new ApiResponse(403, $"Missing required permission(s): {string.Join(", ", permissionNeeded)}");

        protected ApiResponse ApiNotFound(string err) => new ApiResponse(404, err);

        protected ApiResponse ApiInternalError(Exception ex) => new ApiResponse(500, ex);

        protected ApiResponse ApiInternalError(string err) => new ApiResponse(500, err);

    }
}
