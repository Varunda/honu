
-- commands to export the data into a CSV

select now(), 'death'; copy (select * from wt_kills WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_kills_13.csv' WITH header csv; select now();
select now(), 'exp'; copy (select * from wt_exp WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_exp_13.csv' WITH header csv; select now();
select now(), 'vehicle'; copy (select * from vehicle_destroy WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_vehicle_destroy_13.csv' WITH header csv; select now();
select now(), 'item added'; copy (select * from item_added WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_item_added_13.csv' WITH header csv; select now();
select now(), 'achievement'; copy (select * from achievement_earned WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_achievement_earned_13.csv' WITH header csv; select now();
select now(), 'ledger'; copy (select * from wt_ledger_player WHERE timestamp >= '2024-12-01' AND timestamp < '2025-01-01') TO '/mnt/vdb1/db/wrapped_2024_ledger_player_13.csv' WITH header csv; select now();

-- import 

select now(), 'kills'; copy wt_kills_2024 from '/mnt/vdb1/db/wrapped_2024_kills_13.csv' with header csv; select now();
select now(), 'deaths'; copy wt_deaths_2024 from '/mnt/vdb1/db/wrapped_2024_kills_13.csv' with header csv; select now();
select now(), 'exp'; copy wt_exp_2024 from '/mnt/vdb1/db/wrapped_2024_exp_13.csv' with header csv; select now();
select now(), 'v kills'; copy vehicle_destroy_kill_2024 from '/mnt/vdb1/db/wrapped_2024_vehicle_destroy_13.csv' with header csv; select now();
select now(), 'v deaths'; copy vehicle_destroy_death_2024 from '/mnt/vdb1/db/wrapped_2024_vehicle_destroy_13.csv' with header csv; select now();
select now(), 'item added'; copy item_added_2024 from '/mnt/vdb1/db/wrapped_2024_item_added_13.csv' with header csv; select now();
select now(), 'ledger'; copy wt_ledger_player_2024 from '/mnt/vdb1/db/wrapped_2024_ledger_player_13.csv' with header csv; select now();
select now(), 'achievement'; copy achievement_earned_2024 from '/mnt/vdb1/db/wrapped_2024_achievement_earned_13.csv' with header csv; select now();

-- clusters

select now(), 'kills'; cluster wt_kills_2024; select now();
select now(), 'deaths'; cluster wt_deaths_2024; select now();
select now(), 'exp'; cluster wt_exp_2024 from; select now();
select now(), 'v kills'; cluster vehicle_destroy_kill_2024; select now();
select now(), 'v deaths'; cluster vehicle_destroy_death_2024; select now();
select now(), 'item added'; cluster item_added_2024; select now();
select now(), 'ledger'; cluster wt_ledger_player_2024; select now();
select now(), 'achievement'; cluster achievement_earned_2024; select now();
