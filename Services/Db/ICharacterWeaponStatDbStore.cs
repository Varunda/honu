using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Db {

    public interface ICharacterWeaponStatDbStore {

        /// <summary>
        ///     Get the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A list of all the <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.CharacterID"/>
        ///     of <paramref name="charID"/>
        /// </returns>
        Task<List<WeaponStatEntry>> GetByCharacterID(string charID);

        /// <summary>
        ///     Get all the <see cref="WeaponStatEntry"/> for a weapon
        /// </summary>
        /// <param name="itemID">ID of the weapon</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A list of all <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.WeaponID"/> of <paramref name="itemID"/>
        /// </returns>
        Task<List<WeaponStatEntry>> GetByItemID(string itemID, int? minKills = null);

        /// <summary>
        ///     Get the top performers with a weapon. This is meant to be used internally and isn't commented :)
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="column"></param>
        /// <param name="worlds"></param>
        /// <param name="factions"></param>
        /// <param name="minKills"></param>
        /// <returns></returns>
        Task<List<WeaponStatEntry>> GetTopEntries(string itemID, string column, List<short> worlds, List<short> factions, int minKills = 1159);

        /// <summary>
        ///     Update or insert an entry
        /// </summary>
        /// <param name="entry">Entry to upsert</param>
        Task Upsert(WeaponStatEntry entry);

    }

    public static class ICharacterWeaponStatDbStoreExtensionMethods {

        public static Task<List<WeaponStatEntry>> GetTopKD(this ICharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "kd", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopKPM(this ICharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "kpm", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopAccuracy(this ICharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "acc", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopHeadshotRatio(this ICharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "hsr", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopKills(this ICharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions) {
            return repo.GetTopEntries(itemID, "kills", worlds, factions, 0);
        }
    }

}
