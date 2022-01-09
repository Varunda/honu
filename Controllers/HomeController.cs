using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using watchtower.Models.Census;
using watchtower.Services;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers {

    public class HomeController : Controller {

        private readonly ICharacterRepository _CharacterRepository;
        private readonly BackgroundCharacterWeaponStatQueue _Queue;

        public HomeController(ICharacterRepository charRepo,
            BackgroundCharacterWeaponStatQueue queue) {

            _CharacterRepository = charRepo;
            _Queue = queue;
        }

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

        public IActionResult MassSession() {
            return View();
        }

        public IActionResult Character() {
            return View();
        }

        public IActionResult ItemStatViewer() {
            return View();
        }

        public IActionResult CharacterViewer(string charID) {
            _Queue.Queue(charID);
            return View();
        }

        public IActionResult SessionViewer(long sessionID) {
            return View();
        }

        public IActionResult OutfitViewer(string outfitID) {
            return View();
        }
        
        public IActionResult OutfitFinder() {
            return View();
        }

        public async Task<IActionResult> Player(string name) {
            List<PsCharacter> chars = await _CharacterRepository.GetByName(name);
            if (chars.Count == 1) {
                return Redirect($"/c/{chars[0].ID}");
            }
            return Redirect("/character");
        }

        public IActionResult Report() {
            return View();
        }

        public IActionResult PsbNamed() {
            return View();
        }

        public IActionResult Items() {
            return View();
        }

    }
}
