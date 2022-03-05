using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using watchtower.Models.Census;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers {

    public class HomeController : Controller {

        private readonly CharacterRepository _CharacterRepository;
        private readonly AlertDbStore _AlertDb;
        private readonly CharacterUpdateQueue _Queue;

        public HomeController(CharacterRepository charRepo,
            CharacterUpdateQueue queue, AlertDbStore alertDb) {

            _CharacterRepository = charRepo;
            _Queue = queue;
            _AlertDb = alertDb;
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

        public IActionResult PsbUnauthorized() {
            return View();
        }

        public IActionResult JaegerNsa() {
            return View();
        }

        public IActionResult RealtimeMap() {
            return View();
        }

        public async Task<IActionResult> Alert(string alertID) {
            if (alertID.Contains('-')) {
                string[] parts = alertID.Split('-');

                if (parts.Length == 2) {
                    bool validWorld = short.TryParse(parts[0], out short worldID);
                    bool validInstance = int.TryParse(parts[1], out int instanceID);

                    if (validWorld == true && validInstance == true) {
                        PsAlert? alert = await _AlertDb.GetByInstanceID(instanceID, worldID);

                        if (alert != null) {
                            return Redirect($"/alert/{alert.ID}");
                        }
                    }
                }
            }

            return View("AlertViewer");
        }

        public IActionResult Alerts() {
            return View("AlertList");
        }

    }
}
