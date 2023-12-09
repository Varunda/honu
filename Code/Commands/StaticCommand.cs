using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Repositories.Static;

namespace watchtower.Code.Commands {

    [Command]
    public class StaticCommand {

        private readonly ILogger<StaticCommand> _Logger;
        private readonly IServiceProvider _Services;

        private readonly AchievementCollection _AchievementCollection;
        
        public StaticCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<StaticCommand>>();

            _AchievementCollection = services.GetRequiredService<AchievementCollection>();

            _Services = services;
        }

        public void Print(string collectionName) {
            try {
                Assembly honu = typeof(BaseStaticCollection<>).Assembly;

                string typeName = $"watchtower.Services.Census.{collectionName}Collection";

                Type? staticCollection = honu.GetType(typeName);

                if (staticCollection == null) {
                    _Logger.LogError($"Failed to get collection '{typeName}'");
                    return;
                }

                MethodInfo[] methods = staticCollection.GetMethods();
                MethodInfo? getAllMethod = methods.FirstOrDefault(iter => iter.Name == "GetAll");

                if (getAllMethod == null) {
                    _Logger.LogError($"Type {staticCollection.Name} does not have a method named GetAll()");
                    return;
                }

                object? collection = _Services.GetService(staticCollection);
                if (collection == null) {
                    _Logger.LogError($"Failed to get service {staticCollection.FullName}");
                    return;
                }

                object? getAllResult = getAllMethod.Invoke(collection, null);
                if (getAllResult == null) {
                    _Logger.LogError($"Got a null object from .GetAll()");
                    return;
                }

                Type resultType = getAllResult.GetType();
                _Logger.LogDebug($"{resultType.FullName}");

                Task<object> resultTask = Task.FromResult(getAllResult);

                _Logger.LogInformation("down here");
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to get collection '{collectionName}'");
            }
        }

        public async Task Achievement() {
            List<Achievement> achs = await _AchievementCollection.GetAll();

            _Logger.LogDebug($"{achs.Count}");
        }

        public async Task UpdateAll() {
            Stopwatch timer = Stopwatch.StartNew();

            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IRefreshableRepository).IsAssignableFrom(p) && p.IsAbstract == false && p.ContainsGenericParameters == false);

            foreach (Type t in types) {
                _Logger.LogDebug($"refreshing {t.FullName}");
                IRefreshableRepository repo = (IRefreshableRepository) _Services.GetRequiredService(t);
                await repo.Refresh(CancellationToken.None);
            }

            _Logger.LogDebug($"finished static data update in {timer.ElapsedMilliseconds}ms");
        }

    }
}
