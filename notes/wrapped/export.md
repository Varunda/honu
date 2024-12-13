
-- commands to export the data into a CSV

select now(); copy (select * from wt_kills WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_kills_12.csv' WITH header csv; select now();

select now(); copy (select * from wt_exp WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_exp_12.csv' WITH header csv; select now();

select now(); copy (select * from vehicle_destroy WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_vehicle_destroy_12.csv' WITH header csv; select now();

select now(); copy (select * from item_added WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_item_added_12.csv' WITH header csv; select now();

select now(); copy (select * from achievement_earned WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_achievement_earned_12.csv' WITH header csv; select now();

select now(); copy (select * from wt_ledger_player WHERE timestamp >= '2024-01-01' AND timestamp < '2024-12-01') TO '/mnt/vdb1/db/wrapped_2024_ledger_player_12.csv' WITH header csv; select now();
