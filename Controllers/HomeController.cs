using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using watchtower.Code;
using watchtower.Models.Census;
using watchtower.Models.Internal;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers {

    public class HomeController : Controller {

        private readonly CharacterRepository _CharacterRepository;
        private readonly AlertRepository _AlertRepository;
        private readonly CharacterUpdateQueue _Queue;

        public HomeController(CharacterRepository charRepo,
            CharacterUpdateQueue queue, AlertRepository alertRepository) {

            _CharacterRepository = charRepo;
            _Queue = queue;
            _AlertRepository = alertRepository;
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

        public async Task<IActionResult> Alert(string alertID, string? outfitID) {
            if (outfitID == "") {
                outfitID = null;
            }

            PsAlert? alert = null;

            if (alertID.Contains('-')) {
                try {
                    alert = await _AlertRepository.GetByInstanceID(alertID);
                } catch (FormatException) {
                    alert = null;
                }

                if (alert != null && outfitID == null) {
                    return Redirect($"/alert/{alert.ID}");
                }

                if (alert != null && outfitID != null) {
                    DateTimeOffset start = new DateTimeOffset(alert.Timestamp);
                    DateTimeOffset end = new DateTimeOffset(alert.Timestamp + TimeSpan.FromSeconds(alert.Duration));

                    string gen = $"{start.ToUnixTimeSeconds()},{end.ToUnixTimeSeconds()};o{outfitID};";
                    string url = Convert.ToBase64String(Encoding.ASCII.GetBytes(gen));

                    return Redirect($"/report/{url}");
                }
            }

            return View("AlertViewer");
        }

        public IActionResult Alerts() {
            return View("AlertList");
        }

        public IActionResult FriendNetwork(string charID) {
            return View();
        }

        public IActionResult Health() {
            return View();
        }

        public IActionResult RealtimeNetwork(short worldID) {
            return View();
        }

        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public IActionResult AccountManagement() {
            return View();
        }

        [PermissionNeeded(HonuPermission.ALERT_CREATE)]
        [Authorize]
        public IActionResult AlertCreate() {
            return View();
        }

        public IActionResult RealtimeAlert(short? worldID, uint? zoneID) {
            return View();
        }

        public IActionResult OutfitSankey() {
            return View();
        }

        public IActionResult Population() {
            return View();
        }

    }
}
