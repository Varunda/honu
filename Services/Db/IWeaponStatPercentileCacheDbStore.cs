using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public interface IWeaponStatPercentileCacheDbStore {

        /// <summary>
        ///     Get the cached percentile stats of an item for the type given
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="typeID">ID of the cache type</param>
        /// <returns>
        ///     The <see cref="WeaponStatPercentileCache"/> with <see cref="WeaponStatPercentileCache.ItemID"/> of <paramref name="itemID"/>
        ///     and <see cref="WeaponStatPercentileCache.TypeID"/> of <paramref name="typeID"/>, if it exists
        /// </returns>
        Task<WeaponStatPercentileCache?> GetByItemID(string itemID, short typeID);

        /// <summary>
        ///     Update/Insert a <see cref="WeaponStatPercentileCache"/> into the database
        /// </summary>
        /// <param name="itemID">Item ID the <see cref="WeaponStatPercentileCache"/> is for</param>
        /// <param name="entry">Parameters used to insert the data</param>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        Task Upsert(string itemID, WeaponStatPercentileCache entry);

        /// <summary>
        ///     Generate percentile data of an item based on a column in the table. Meant for internal use
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <param name="columnName">Name of the column in the table. I know this opens this for SQL injection, but this isn't called from an API</param>
        /// <param name="minKills">How many kills a character must have to be included</param>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        Task<WeaponStatPercentileCache?> Generate(string itemID, string columnName, int minKills = 1159);

    }

    public static class IWeaponStatPercentileCacheDbStoreExtensions {

        /// <summary>
        ///     Force the generation of the percentiles for KPM for an item
        /// </summary>
        /// <param name="db">Extension instance</param>
        /// <param name="itemID">Item ID to generate the percentiles of</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A newly genereated <see cref="WeaponStatPercentileCache"/> of the item,
        ///     or <c>null</c> if the item does not exist
        /// </returns>
        public static async Task<WeaponStatPercentileCache?> GenerateKpm(this IWeaponStatPercentileCacheDbStore db, string itemID, int minKills = 1159) {
            WeaponStatPercentileCache? entry = await db.Generate(itemID, "kpm", minKills);
            if (entry == null) {
                return null;
            }

            entry.TypeID = PercentileCacheType.KPM;
            return entry;
        }

        /// <summary>
        ///     Force the generation of the percentiles for KD for an item
        /// </summary>
        /// <param name="db">Extension instance</param>
        /// <param name="itemID">Item ID to generate the percentiles of</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A newly genereated <see cref="WeaponStatPercentileCache"/> of the item,
        ///     or <c>null</c> if the item does not exist
        /// </returns>
        public static async Task<WeaponStatPercentileCache?> GenerateKd(this IWeaponStatPercentileCacheDbStore db, string itemID, int minKills = 1159) {
            WeaponStatPercentileCache? entry = await db.Generate(itemID, "kd", minKills);
            if (entry == null) {
                return null;
            }

            entry.TypeID = PercentileCacheType.KD;
            return entry;
        }

        /// <summary>
        ///     Force the generation of the percentiles for accuracy for an item
        /// </summary>
        /// <param name="db">Extension instance</param>
        /// <param name="itemID">Item ID to generate the percentiles of</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A newly genereated <see cref="WeaponStatPercentileCache"/> of the item,
        ///     or <c>null</c> if the item does not exist
        /// </returns>
        public static async Task<WeaponStatPercentileCache?> GenerateAcc(this IWeaponStatPercentileCacheDbStore db, string itemID, int minKills = 1159) {
            WeaponStatPercentileCache? entry = await db.Generate(itemID, "acc", minKills);
            if (entry == null) {
                return null;
            }

            entry.TypeID = PercentileCacheType.ACC;
            return entry;
        }

        /// <summary>
        ///     Force the generation of the percentiles for headshot ratio for an item
        /// </summary>
        /// <param name="db">Extension instance</param>
        /// <param name="itemID">Item ID to generate the percentiles of</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A newly genereated <see cref="WeaponStatPercentileCache"/> of the item,
        ///     or <c>null</c> if the item does not exist
        /// </returns>
        public static async Task<WeaponStatPercentileCache?> GenerateHsr(this IWeaponStatPercentileCacheDbStore db, string itemID, int minKills = 1159) {
            WeaponStatPercentileCache? entry = await db.Generate(itemID, "hsr", minKills);
            if (entry == null) {
                return null;
            }

            entry.TypeID = PercentileCacheType.HSR;
            return entry;
        }

    }

}
