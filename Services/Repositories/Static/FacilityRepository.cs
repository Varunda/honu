using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class FacilityRepository {

        private readonly ILogger<FacilityRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly IFacilityDbStore _Db;

        public FacilityRepository(ILogger<FacilityRepository> logger,
            IFacilityDbStore db, IMemoryCache cache) {

            _Logger = logger;
            _Db = db;
            _Cache = cache;
        }

        /// <summary>
        ///     Get all <see cref="PsFacility"/>s
        /// </summary>
        /// <returns>
        ///     A list of all <see cref="PsFacility"/>s
        /// </returns>
        public async Task<List<PsFacility>> GetAll() {
            if (_Cache.TryGetValue("Facilities.All", out List<PsFacility> facs) == false) {
                facs = await _Db.GetAll();

                // if the DB call fails for whatever, or it has yet to be populated, don't cache it
                if (facs.Count > 0) {
                    _Cache.Set("Facilities.All", facs, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromHours(4)
                    });
                }
            }

            return facs;
        }

        /// <summary>
        ///     Get a single <see cref="PsFacility"/> by it's <see cref="PsFacility.FacilityID"/>
        /// </summary>
        /// <param name="facilityID">ID of the <see cref="PsFacility"/> to get</param>
        /// <returns>
        ///     The <see cref="PsFacility"/> with <see cref="PsFacility.FacilityID"/> of <paramref name="facilityID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<PsFacility?> GetByID(int facilityID) {
            return (await GetAll()).FirstOrDefault(iter => iter.FacilityID == facilityID);
        }

        /// <summary>
        ///     Get a list of <see cref="PsFacility"/> that have <see cref="PsFacility.FacilityID"/> in <paramref name="IDs"/>
        /// </summary>
        /// <param name="IDs">IDs to get the facilities of</param>
        /// <returns>
        ///     A list of <see cref="PsFacility"/>, where each element has a <see cref="PsFacility.FacilityID"/> 
        ///     that is within <paramref name="IDs"/>. If an ID does not have a corresponding <see cref="PsFacility"/>,
        ///     then it will not be included in the returned list
        /// </returns>
        public async Task<List<PsFacility>> GetByIDs(IEnumerable<int> IDs) {
            return (await GetAll()).Where(iter => IDs.Contains(iter.FacilityID)).ToList();
        }

        /// <summary>
        ///     Get the <see cref="PsFacility"/> by it's <see cref="PsFacility.RegionID"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PsFacility?> GetByRegionID(int id) {
            return (await GetAll()).FirstOrDefault(iter => iter.RegionID == id);
        }

        /// <summary>
        ///     Search for <see cref="PsFacility"/>s that match the name
        /// </summary>
        /// <param name="name">name to search by. Case insensitive, and all non-alphanumeric characters are removed</param>
        /// <returns>
        ///     A list of all possible <see cref="PsFacility"/>s that match the name passed
        /// </returns>
        public async Task<List<PsFacility>> SearchByName(string name) {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string baseName = rgx.Replace(name.Trim().ToLower(), "");

            List<PsFacility> facilities = await GetAll();
            List<PsFacility> possibleBases = new();

            if (baseName != name.Trim().ToLower()) {
                _Logger.LogDebug($"Text stripped from input: [name='{name}'] [baseName='{baseName}']");
            }

            foreach (PsFacility fac in facilities) {
                // if the name of a base is a perfect match, then they probably meant that one
                if (fac.Name.ToLower() == baseName || fac.Name.ToLower() == name) {
                    _Logger.LogTrace($"exact match {name}");
                    possibleBases.Add(fac);
                    break;
                }

                // remove all non-alphanumeric and space characters
                string strippedName = "";
                foreach (char c in fac.Name) {
                    if (char.IsNumber(c) || char.IsLetter(c) || c == ' ') {
                        strippedName += c;
                    }
                }
                strippedName = strippedName.ToLower();

                // 3 different forms are accepted:
                //      1. the name itself
                //      2. the name, but plural (Chac Fusion Lab / Chac Fusion Labs)
                //      3. the name, but with 'the' in front of it (The Bastion / Bastion)
                string facName = $"{strippedName} {fac.TypeName}".ToLower();
                if (facName.StartsWith(baseName) == true
                    || (facName + "s").StartsWith(baseName) == true
                    || facName.StartsWith("the " + baseName) == true
                    ) {

                    _Logger.LogDebug($"{facName} => {baseName}");

                    possibleBases.Add(fac);
                }
            }

            return possibleBases;
        }

    }
}
