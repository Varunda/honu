<template>
    <div>
        howdy

        <div v-if="snapshots.state == 'loaded'">
            <div v-for="datum in data">
                {{datum.timestamp | moment}}
                {{datum.killsDiff}}
                {{datum.deathsDiff}}
                {{datum.shotsDiff}}
                {{datum.shotsHitDiff}}
                {{datum.headshotsDiff}}
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { WeaponStatSnapshot, WeaponStatSnapshotApi } from "api/WeaponStatSnapshotApi";

    import "MomentFilter";

    type DiffedSnapshot = {
        id: number;
        timestamp: Date;

        kills: number;
        killsDiff: number | null;

        deaths: number;
        deathsDiff: number | null;

        shots: number;
        shotsDiff: number | null;

        shotsHit: number;
        shotsHitDiff: number | null;

        headshots: number;
        headshotsDiff: number | null;
    };

    export const ItemSnapshot = Vue.extend({
        props: {
            ItemId: { type: String, required: true }
        },

        data: function() {
            return {
                snapshots: Loadable.idle() as Loading<WeaponStatSnapshot[]>,
                data: [] as DiffedSnapshot[]
            }
        },

        created: function(): void {
            this.bind();
        },

        methods: {
            bind: async function(): Promise<void> {
                this.snapshots = Loadable.loading();
                this.snapshots = await WeaponStatSnapshotApi.getByItemID(Number.parseInt(this.ItemId));

                if (this.snapshots.state == "loaded") {
                    const sorted: WeaponStatSnapshot[] = this.snapshots.data.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
                    this.data = sorted.map((iter: WeaponStatSnapshot, index: number) => {
                        let next: WeaponStatSnapshot | null = null;
                        if (index != sorted.length - 1) {
                            next = sorted[index + 1];
                        }

                        return {
                            id: iter.itemID,
                            timestamp: iter.timestamp,

                            kills: iter.kills,
                            killsDiff: (next != null) ? (iter.kills - next.kills) : null,

                            deaths: iter.deaths,
                            deathsDiff: (next != null) ? (iter.deaths - next.deaths) : null,

                            shots: iter.shots,
                            shotsDiff: (next != null) ? (iter.shots - next.shots) : null,

                            shotsHit: iter.shotsHit,
                            shotsHitDiff: (next != null) ? (iter.shotsHit - next.shotsHit) : null,

                            headshots: iter.headshots,
                            headshotsDiff: (next != null) ? (iter.headshots - next.headshots) : null
                        };
                    });
                }

            }
        },

        components: {

        }
    });
    export default ItemSnapshot;
</script>