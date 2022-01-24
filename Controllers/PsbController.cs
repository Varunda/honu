using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using watchtower.Code;

namespace watchtower.Controllers {

    //[Authorize]
    //[PsbAdmin]
    public class PsbController : Controller {

        public IActionResult Named() {
            return View();
        }

    }
}
