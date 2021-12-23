using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch29DirectiveTables : IDbPatch {

        public int MinVersion => 29;
        public string Name => "create directive tables";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_directives (
                    character_id varchar NOT NULL,
                    directive_id int NOT NULL,
                    directive_tree_id int NOT NULL,
                    completion_date timestamptz NULL,

                    PRIMARY KEY (character_id, directive_id)
                );

                CREATE INDEX IF NOT EXISTS idx_character_directives_character_id 
                    ON character_directives (character_id);

                CREATE TABLE IF NOT EXISTS character_directive_objective (
                    character_id varchar NOT NULL,
                    directive_id int NOT NULL,
                    objective_id int NOT NULL,
                    objective_group_id int NOT NULL,
                    status int NOT NULL,
                    state_data int NOT NULL,

                    PRIMARY KEY (character_id, directive_id)
                );

                CREATE INDEX IF NOT EXISTS idx_character_directive_objective_character_id
                    ON character_directive_objective (character_id);

                CREATE TABLE IF NOT EXISTS character_directive_tree (
                    character_id varchar NOT NULL,
                    tree_id int NOT NULL,
                    current_tier int NOT NULL,
                    current_level int NOT NULL,
                    completion_date timestamptz NULL,

                    PRIMARY KEY (character_id, tree_id)
                );

                CREATE INDEX IF NOT EXISTS idx_character_directive_tree_character_id
                    ON character_directive_tree (character_id);

                CREATE TABLE IF NOT EXISTS character_directive_tier (
                    character_id varchar NOT NULL,
                    tree_id int NOT NULL,
                    tier_id int NOT NULL,
                    completion_date timestamptz NULL,

                    PRIMARY KEY (character_id, tree_id, tier_id)
                );

                CREATE INDEX IF NOT EXISTS idx_character_directive_tier_character_id
                    ON character_directive_tier (character_id);
                
                CREATE TABLE IF NOT EXISTS directive (
                    id int NOT NULL PRIMARY KEY,
                    tree_id int NOT NULL,
                    tier_id int NOT NULL,
                    objective_set_id int NOT NULL,
                    name varchar NOT NULL,
                    description varchar NOT NULL,
                    image_set_id int NOT NULL,
                    image_id int NOT NULL
                );

                CREATE TABLE IF NOT EXISTS directive_tree (
                    id int NOT NULL PRIMARY KEY,
                    category_id int NOT NULL,
                    name varchar NOT NULL,
                    image_set_id int NOT NULL,
                    image_id int NOT NULL
                );

                CREATE TABLE IF NOT EXISTS directive_tier (
                    tier_id int NOT NULL,
                    tree_id int NOT NULL,
                    reward_set_id int NULL,
                    directive_points int NOT NULL,
                    completion_count int NOT NULL,
                    name varchar NOT NULL,
                    image_set_id int NOT NULL,
                    image_id int NOT NULL,

                    PRIMARY KEY (tier_id, tree_id)
                );

                CREATE INDEX IF NOT EXISTS idx_directive_tier_tree_id 
                    ON directive_tier (tree_id);

                CREATE TABLE IF NOT EXISTS directive_tree_category (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
