using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using watchtower.Code.CharacterViewer;

namespace watchtower.Services.CharacterViewer {

    public class ExtraStatHoster {

        private readonly ILogger<ExtraStatHoster> _Logger;
        private readonly IServiceProvider _Services;
        private readonly ICharacterStatGeneratorStore _GeneratorStore;

        public ExtraStatHoster(ILogger<ExtraStatHoster> logger,
            IServiceProvider services, ICharacterStatGeneratorStore genStore) {

            _Logger = logger;
            _Services = services;
            _GeneratorStore = genStore;
        }

        public void Clear() {
            _GeneratorStore.ClearGenerators();
        }

        public void Discover() {
            string[] files;

            _Logger.LogDebug($"Searching for 'extra-stats' in '.': {Directory.GetCurrentDirectory()}");

            try {
                files = Directory.GetFiles("./extra-stats");
            } catch (DirectoryNotFoundException) {
                _Logger.LogInformation($"Folder './extra-stats' does not exist, attempting to make");
                try {
                    DirectoryInfo info = Directory.CreateDirectory("./extra-stats");
                    _Logger.LogInformation($"{info.FullName}");
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"Failed to make extra stat folder './extra-stats'");
                }

                return;
            }

            _Logger.LogDebug($"Using the following files for extra stats: {string.Join(", ", files)}");

            foreach (string file in files) {
                string? extension = Path.GetExtension(file);
                if (extension == null || extension == string.Empty) {
                    continue;
                }

                if (extension.ToLower() != ".dll") {
                    _Logger.LogDebug($"{file} is a not DLL, skipping");
                    continue;
                }

                Assembly assembly;

                try {
                    assembly = Assembly.LoadFrom(file);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"Failed to load assembly from '{file}'");
                    continue;
                }

                Type[] types = assembly.GetExportedTypes();

                foreach (Type type in types) {
                    _Logger.LogTrace($"{file} => {type.FullName}");

                    if (type.GetInterface(nameof(ICharacterStatGenerator)) != null) {
                        ICharacterStatGenerator? gen = (ICharacterStatGenerator) ActivatorUtilities.CreateInstance(_Services, type);
                        if (gen == null) {
                            _Logger.LogWarning($"Failed to create instance of {type.FullName}");
                            continue;
                        }
                        _GeneratorStore.Add(gen);
                    }
                }
            }
        }

    }
}
