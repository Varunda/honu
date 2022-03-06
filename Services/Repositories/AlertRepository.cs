using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AlertRepository {

        private readonly ILogger<AlertRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly AlertDbStore _AlertDb;

        public AlertRepository(ILogger<AlertRepository> logger,
            AlertDbStore alertDb, IMemoryCache cache) {

            _Logger = logger;
            _AlertDb = alertDb;
            _Cache = cache;
        }

        public Task<PsAlert?> GetByID(long ID) {
            return _AlertDb.GetByID(ID);
        }

        public Task<PsAlert?> GetByInstanceID(int instanceID, short worldID) {
            return _AlertDb.GetByInstanceID(instanceID, worldID);
        }

        /// <summary>
        ///     Get an alert by its instance string, which is WorldID-InstanceID
        /// </summary>
        /// <param name="worldInstID">Instance string to get the alert of</param>
        /// <returns>
        ///     The <see cref="PsAlert"/> that matches the instance string
        /// </returns>
        /// <exception cref="FormatException">If <paramref name="worldInstID"/> was malformed</exception>
        public async Task<PsAlert?> GetByInstanceID(string worldInstID) {
            if (worldInstID.Contains('-') == false) {
                throw new FormatException($"missing '-' in {worldInstID}. A valid worldInst string is $WORLD_ID-$INSTANCE_ID");
            }

            string[] parts = worldInstID.Split('-');

            if (parts.Length != 2) {
                throw new FormatException($"failed to split {worldInstID} into two parts. A valid worldInst string is $WORLD_ID-$INSTANCE_ID");
            }
            
            bool validWorld = short.TryParse(parts[0], out short worldID);
            bool validInstance = int.TryParse(parts[1], out int instanceID);

            if (validWorld == false) {
                throw new FormatException($"failed to convert {parts[0]} into a valid Int16");
            }

            if (validInstance == false) {
                throw new FormatException($"failed to convert {parts[1]} into a valid Int64");
            }

            PsAlert? alert = await _AlertDb.GetByInstanceID(instanceID, worldID);
            return alert;
        }

        /// <summary>
        ///     Update an alert by it's ID
        /// </summary>
        /// <param name="alertID">ID of the alert to update</param>
        /// <param name="alert">Parameters used to update the alert</param>
        public Task UpdateByID(long alertID, PsAlert alert) => _AlertDb.UpdateByID(alertID, alert);

        /// <summary>
        ///     Insert a new alert, returning the ID of the newly inserted alert
        /// </summary>
        /// <param name="alert">Parameters used to insert the alert</param>
        /// <returns></returns>
        public Task<long> Insert(PsAlert alert) => _AlertDb.Insert(alert);

    }
}
