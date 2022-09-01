using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using watchtower.Code;
using watchtower.Models.Internal;

namespace watchtower.Controllers {

    public class PsbController : Controller {

        [PermissionNeeded(HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public IActionResult Named() {
            return View();
        }

    }
}
