using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using watchtower.Services.Metrics;

namespace watchtower.Controllers {

    /// <summary>
    ///     proxies census images and caches them locally
    /// </summary>
    public class ImageProxyController : Controller {

        private readonly ILogger<ImageProxyController> _Logger;

        private readonly ImageProxyMetric _Metrics;

        private readonly HttpClient _HttpClient;

        public ImageProxyController(ILogger<ImageProxyController> logger, 
            ImageProxyMetric metrics) {

            _Logger = logger;

            _HttpClient = new HttpClient() {
                BaseAddress = new Uri("https://census.daybreakgames.com/files/ps2/images/static/")
            };

            _Metrics = metrics;
        }

        [ResponseCache(Duration = 60 * 60)]
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
                _Logger.LogInformation($"saved proxy image to image ID [imageID={imageID}] path=[{path}]");
                _Metrics.RecordMiss();
            } else {
                _Metrics.RecordHit();
            }

            FileStream image = System.IO.File.OpenRead(path);
            return File(image, "image/png", filename, false);
        }

    }
}
