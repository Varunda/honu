
-- START
SET maintenance_work_mem = '768MB';

-- EXP
SELECT now();
CREATE TABLE IF NOT EXISTS wt_exp_2022 AS (
	SELECT * FROM wt_exp WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_wt_exp_2022_source_character_id ON wt_exp_2022 (source_character_id);

SELECT now();
CLUSTER wt_exp_2022 USING idx_wt_exp_2022_source_character_id;

-- KILLS
SELECT now();
CREATE TABLE IF NOT EXISTS wt_kills_2022 AS (
	SELECT * FROM wt_kills WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_wt_kills_2022_attacker_character_id ON wt_kills_2022 (attacker_character_id);

SELECT now();
CLUSTER wt_kills_2022 USING idx_wt_kills_2022_attacker_character_id;

-- DEATHS
SELECT now();
CREATE TABLE IF NOT EXISTS wt_deaths_2022 AS (select * from wt_kills_2022);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_wt_deaths_2022_killed_character_id ON wt_deaths_2022 (killed_character_id);

SELECT now();
CLUSTER wt_deaths_2022 USING idx_wt_deaths_2022_killed_character_id;

-- VEHICLE_DESTROY KILL
SELECT now();
CREATE TABLE IF NOT EXISTS vehicle_destroy_kill_2022 AS (
	select * from vehicle_destroy WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_kill_2022_attacker_character_id ON vehicle_destroy_kill_2022 (attacker_character_id);

SELECT now();
CLUSTER vehicle_destroy_kill_2022 USING idx_vehicle_destroy_kill_2022_attacker_character_id;

-- VEHICLE_DESTROY DEATH
SELECT now();
CREATE TABLE IF NOT EXISTS vehicle_destroy_death_2022 AS (select * from vehicle_destroy_kill_2022);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_vehicle_destroy_death_2022_killed_character_id ON vehicle_destroy_death_2022 (killed_character_id);

SELECT now();
CLUSTER vehicle_destroy_death_2022 USING idx_vehicle_destroy_death_2022_killed_character_id;

-- ACHIEVEMENT_EARNED
SELECT now();
CREATE TABLE IF NOT EXISTS achievement_earned_2022 AS (
	select * FROM achievement_earned WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_achievement_earned_2022_character_id ON achievement_earned_2022 (character_id);

SELECT now();
CLUSTER achievement_earned_2022 USING idx_achievement_earned_2022_character_id;

-- ITEM_ADDED
SELECT now();
CREATE TABLE IF NOT EXISTS item_added_2022 AS (
	SELECT * FROM item_added WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_item_added_2022_character_id ON item_added_2022 (character_id);

SELECT now();
CLUSTER item_added_2022 USING idx_item_added_2022_character_id;

-- PLAYER_FACILITY_CONTROL
SELECT now();
CREATE TABLE IF NOT EXISTS wt_ledger_player_2022 AS (
	SELECT * FROM wt_ledger_player WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01'
);

SELECT now();
CREATE INDEX IF NOT EXISTS idx_wt_ledger_player_2022_character_id ON wt_ledger_player (character_id);

SELECT now();
CLUSTER wt_ledger_2022 USING idx_wt_ledger_player_2022_character_id;
