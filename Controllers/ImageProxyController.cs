using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace watchtower.Controllers {

    public class ImageProxyController : Controller {

        private readonly ILogger<ImageProxyController> _Logger;
        private readonly HttpClient _HttpClient;

        public ImageProxyController(ILogger<ImageProxyController> logger) {
            _Logger = logger;

            _HttpClient = new HttpClient() {
                BaseAddress = new Uri("https://census.daybreakgames.com/files/ps2/images/static/")
            };
        }

        public async Task<IActionResult> Get(long imageID) {

            if (Directory.Exists("./census-image-proxy/") == false) {
                Directory.CreateDirectory("./census-image-proxy/");
            }

            string filename = $"{imageID}.png";
            string path = $"./census-image-proxy/{filename}";

            if (System.IO.File.Exists(path) == false) {
                HttpResponseMessage response = await _HttpClient.GetAsync(filename);

                if (response.StatusCode != HttpStatusCode.OK) {
                    return StatusCode((int)response.StatusCode);
                }

                await System.IO.File.WriteAllBytesAsync(path, await response.Content.ReadAsByteArrayAsync());
                _Logger.LogInformation($"saved proxy image to image ID {imageID} at {path}");
            }

            FileStream image = System.IO.File.OpenRead(path);
            return File(image, "image/png", filename, false);
        }

    }
}
