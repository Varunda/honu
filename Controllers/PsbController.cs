﻿using Microsoft.AspNetCore.Authorization;
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

        [PermissionNeeded(HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public IActionResult Tourney() {
            return View();
        }

        [PermissionNeeded(HonuPermission.PSB_PRACTICE_GET)]
        [Authorize]
        public IActionResult Practice() {
            return View();
        }

        [PermissionNeeded(HonuPermission.PSB_PRACTICE_GET, HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public IActionResult Index() {
            return View();
        }

        [PermissionNeeded(HonuPermission.PSB_PRACTICE_GET, HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public IActionResult Ovo() {
            return View();
        }

        [PermissionNeeded(HonuPermission.PSB_OVO_GET)]
        [Authorize]
        public IActionResult OvoUsage() {
            return View();
        }

    }
}
