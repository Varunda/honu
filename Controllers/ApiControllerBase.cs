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

        protected ApiResponse<T> ApiNotFound<T>(string err) => new ApiResponse<T>(404, err);

        protected ApiResponse<T> ApiInternalError<T>(Exception ex) => new ApiResponse<T>(500, ex.Message);

    }
}
