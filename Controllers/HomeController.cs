using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace watchtower.Controllers {

    public class HomeController : Controller {

        public IActionResult Index() {
            return View();
        }

        public IActionResult SelectWorld() {
            return View();
        }

        public IActionResult Ledger() {
            return View();
        }

        public IActionResult OutfitPop() {
            return View();
        }

        public IActionResult Character() {
            return View();
        }

        public IActionResult CharacterViewer(string charID) {
            return View();
        }

        public IActionResult SessionViewer(long sessionID) {
            return View();
        }

    }
}
