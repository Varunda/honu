# Wrapped notes

Notes about making wrapped

## Table creation

```sql
SELECT now();

CREATE TABLE wt_exp_2022 AS (select * FROM wt_exp WHERE timestamp BETWEEN '2022-01-01' AND '2023-01-01');

SELECT now();

SET maintenance_work_mem = '768MB';

CREATE INDEX idx_wt_exp_2022_source_character_id ON wt_exp_2022 (source_character_id);

SELECT now();

-- took 3 days, but that was with a maintenance_work_mem of 64MB. Clustering wt_kills_2022 with 768MB took 3 hours
CLUSTER wt_exp_2022 USING idx_wt_exp_2022_source_character_id;

SELECT now();
```

Each table is a year of data with a single index on the character ID we care about

Then, each table is clustered on the character ID, so reading from the table is just a sequential read on the spinning disk

This took loading the exp data from 15 minutes to around 500ms