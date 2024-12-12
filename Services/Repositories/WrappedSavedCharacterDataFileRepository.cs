using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using watchtower.Models.Wrapped;

namespace watchtower.Services.Repositories {

    public class WrappedSavedCharacterDataFileRepository {

        private readonly ILogger<WrappedSavedCharacterDataFileRepository> _Logger;

        public WrappedSavedCharacterDataFileRepository(ILogger<WrappedSavedCharacterDataFileRepository> logger) {
            _Logger = logger;
        }

        /// <summary>
        ///     Get the saved character data if it already exists
        /// </summary>
        /// <param name="year">Year the wrapped entry is being generated for</param>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     The <see cref="WrappedSavedCharacterData"/> saved to the disk if it exists,
        ///     or null if it doesn't exist
        /// </returns>
        public async Task<WrappedSavedCharacterData?> Get(DateTime year, string charID) {
            string filename = $"HonuWrapped.{year.Year}.{charID}";
            string filepath = $"./wrapped/{filename}.json";

            if (Directory.Exists("./wrapped/") == false) {
                Directory.CreateDirectory("./wrapped/");
            }

            if (File.Exists(filepath) == false) {
                _Logger.LogDebug($"missing {filepath}");
                return null;
            }

            Stopwatch timer = Stopwatch.StartNew();
            string json;
            try {
                json = await File.ReadAllTextAsync(filepath);
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to read file '{filepath}'");
                return null;
            }
            long readMs = timer.ElapsedMilliseconds; timer.Restart();

            JToken j = JToken.Parse(json);

            WrappedSavedCharacterData? data = j.ToObject<WrappedSavedCharacterData>();
            long parseMs = timer.ElapsedMilliseconds;

            if (data == null) {
                _Logger.LogDebug($"failed to find saved JSON for {charID}");
            } else {
                _Logger.LogDebug($"found saved JSON for {charID}, [read={readMs}ms] [parsed={parseMs}ms]");
            }

            return data;
        }

        /// <summary>
        ///     Save wrapped character to disk
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="year">Year of the wrapped</param>
        /// <param name="data">Data to save</param>
        public async Task Save(string charID, DateTime year, WrappedSavedCharacterData data) {
            string filename = $"HonuWrapped.{year.Year}.{charID}";
            string filepath = $"./wrapped/{filename}.json";

            if (Directory.Exists("./wrapped/") == false) {
                Directory.CreateDirectory("./wrapped/");
            }

            if (File.Exists(filepath) == true) {
                _Logger.LogWarning($"Saved JSON for {charID} already exist! Overwritting");
            }

            _Logger.LogInformation($"saving character data to JSON [charID={charID}] [path={filepath}]");

            JToken json = JToken.FromObject(data);

            await File.WriteAllTextAsync(filepath, $"{json}", encoding: System.Text.Encoding.UTF8);
        }

    }
}
