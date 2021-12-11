using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Implementations {

    public class DefaultDbCreator : IDbCreator {

        private readonly ILogger<DefaultDbCreator> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly bool _RunDb = true;

        public DefaultDbCreator(ILogger<DefaultDbCreator> logger,
                IDbHelper dbHelper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task Execute() {
            if (_RunDb == false) {
                return;
            }

            // Ensure the extension is loaded, as some of the patches may use the things the extension provides
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                CREATE EXTENSION IF NOT EXISTS pg_trgm;
            ");

            try {
                _Logger.LogTrace($"Getting current DB version");
                int version = await GetVersion();
                _Logger.LogInformation($"Current DB version: {version}");

                List<IDbPatch> patches = GetPatches();
                foreach (IDbPatch patch in patches) {
                    _Logger.LogTrace($"Checking patch {patch.Name}/{patch.MinVersion}");

                    if (version < patch.MinVersion) {
                        _Logger.LogDebug($"Patch {patch.Name} min version {patch.MinVersion} lower than current version {version}");
                        await patch.Execute(_DbHelper);

                        await UpdateVersion(patch.MinVersion);
                    }
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to execute DbCreator");
            }
        }

        /// <summary>
        ///     Get all the patches loaded in the currently assembly
        /// </summary>
        private List<IDbPatch> GetPatches() {
            List<IDbPatch> patches = new List<IDbPatch>();

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types) {
                if (typeof(IDbPatch).IsAssignableFrom(type)
                    && type.GetCustomAttribute<PatchAttribute>() != null) {

                    object? patch = Activator.CreateInstance(type);
                    if (patch != null) {
                        patches.Add((IDbPatch)patch);
                    } else {
                        _Logger.LogWarning($"Failed to create type {type.Name}");
                    }
                }
            }

            return patches.OrderBy(iter => iter.MinVersion).ToList();
        }

        /// <summary>
        ///     Update the DB version
        /// </summary>
        private async Task UpdateVersion(int version) {
            _Logger.LogTrace($"Updating version to {version}");
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO metadata (name, value)
                    VALUES ('watchtower_id', @ID)
                ON CONFLICT (name) DO
                    UPDATE SET value = @ID;
            ");
            cmd.Parameters.AddWithValue("@ID", version);

            await cmd.ExecuteNonQueryAsync();

            _Logger.LogTrace($"Updated version to {version}");
        }

        /// <summary>
        ///     Get the current DB version, or -1 if no tables have been created, or an error occurs
        /// </summary>
        private async Task<int> GetVersion() {
            if (await DoesMetadataTableExist() == false) {
                _Logger.LogInformation($"No metadata table");
                return -1;
            }

            _Logger.LogTrace($"DB version metadata key: 'watchtower_id'");

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT value
                    FROM metadata
                    WHERE name = 'watchtower_id'
            ");

            object? value = await cmd.ExecuteScalarAsync();
            if (value == null) {
                return -1;
            }

            if (int.TryParse(value.ToString(), out int version) == true) {
                return version;
            }

            _Logger.LogWarning($"Failed to part {value} to a valid Int32");

            return -1;
        }

        /// <summary>
        ///     Check if the metadata table exists
        /// </summary>
        private async Task<bool> DoesMetadataTableExist() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT EXISTS (
                    SELECT FROM pg_tables
                    WHERE  schemaname = 'public'
                    AND    tablename  = 'metadata'
               );
            ");

            object? value = await cmd.ExecuteScalarAsync();
            if (value == null) {
                return false;
            }

            return (bool)value;
        }

    }
}
