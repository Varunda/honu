--
SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

CREATE TABLE public.achievement_earned_2024 (
    id bigint,
    character_id character varying,
    achievement_id integer,
    "timestamp" timestamp with time zone,
    zone_id integer,
    world_id smallint
);
ALTER TABLE public.achievement_earned_2024 OWNER TO postgres;

CREATE TABLE public.item_added_2024 (
    id bigint,
    character_id character varying,
    item_id integer,
    context character varying,
    item_count integer,
    "timestamp" timestamp with time zone,
    zone_id integer,
    world_id smallint
);
ALTER TABLE public.item_added_2024 OWNER TO postgres;

CREATE TABLE public.vehicle_destroy_death_2024 (
    id bigint,
    attacker_character_id character varying,
    attacker_vehicle_id character varying,
    attacker_weapon_id integer,
    attacker_loadout_id smallint,
    attacker_team_id smallint,
    attacker_faction_id smallint,
    killed_character_id character varying,
    killed_faction_id smallint,
    killed_team_id smallint,
    killed_vehicle_id character varying,
    facility_id character varying,
    world_id smallint,
    zone_id integer,
    "timestamp" timestamp with time zone
);
ALTER TABLE public.vehicle_destroy_death_2024 OWNER TO postgres;

CREATE TABLE public.vehicle_destroy_kill_2024 (
    id bigint,
    attacker_character_id character varying,
    attacker_vehicle_id character varying,
    attacker_weapon_id integer,
    attacker_loadout_id smallint,
    attacker_team_id smallint,
    attacker_faction_id smallint,
    killed_character_id character varying,
    killed_faction_id smallint,
    killed_team_id smallint,
    killed_vehicle_id character varying,
    facility_id character varying,
    world_id smallint,
    zone_id integer,
    "timestamp" timestamp with time zone
);
ALTER TABLE public.vehicle_destroy_kill_2024 OWNER TO postgres;

CREATE TABLE public.wt_deaths_2024 (
    id bigint,
    world_id smallint,
    zone_id integer,
    attacker_character_id character varying,
    attacker_loadout_id smallint,
    attacker_faction_id smallint,
    attacker_team_id smallint,
    attacker_fire_mode_id integer,
    attacker_vehicle_id integer,
    killed_character_id character varying,
    killed_loadout_id smallint,
    killed_faction_id smallint,
    killed_team_id smallint,
    revived_event_id bigint,
    weapon_id integer,
    is_headshot boolean,
    "timestamp" timestamp with time zone
);
ALTER TABLE public.wt_deaths_2024 OWNER TO postgres;

CREATE TABLE public.wt_exp_2024 (
    id bigint,
    world_id smallint,
    zone_id integer,
    source_character_id character varying,
    experience_id integer,
    source_loadout_id smallint,
    source_faction_id smallint,
    other_id character varying,
    amount integer,
    "timestamp" timestamp with time zone,
    source_team_id smallint
);
ALTER TABLE public.wt_exp_2024 OWNER TO postgres;

CREATE TABLE public.wt_kills_2024 (
    id bigint,
    world_id smallint,
    zone_id integer,
    attacker_character_id character varying,
    attacker_loadout_id smallint,
    attacker_faction_id smallint,
    attacker_team_id smallint,
    attacker_fire_mode_id integer,
    attacker_vehicle_id integer,
    killed_character_id character varying,
    killed_loadout_id smallint,
    killed_faction_id smallint,
    killed_team_id smallint,
    revived_event_id bigint,
    weapon_id integer,
    is_headshot boolean,
    "timestamp" timestamp with time zone
);
ALTER TABLE public.wt_kills_2024 OWNER TO postgres;

CREATE TABLE public.wt_ledger_player_2024 (
    id bigint,
    control_id bigint,
    character_id character varying,
    facility_id integer,
    outfit_id character varying,
    "timestamp" timestamp with time zone
);
ALTER TABLE public.wt_ledger_player_2024 OWNER TO postgres;
