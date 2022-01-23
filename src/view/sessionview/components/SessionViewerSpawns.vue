<template>
    <div>

        <table class="table table-sm">
            <thead>
                <tr>
                    <th></th>
                </tr>
            </thead>
        </table>

        ye
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { ExpandedExpEvent, Experience } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent } from "api/VehicleDestroyEventApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";

    import ZoneUtils from "util/Zone";
    import TimeUtils from "util/Time";
    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";

    type Spawn = {
        id: string;
        type: "router" | "sunderer";
        spawns: number;
        firstSpawn: Date;
        lastSpawn: Date;
    }

    export const SessionViewerSpawns = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Array as PropType<ExpandedExpEvent[]>, required: true },
        },

        data: function() {
            return {
                spawns: [] as Spawn[]
            }
        },

        mounted: function(): void {
            this.makeSpawns();
        },

        methods: {
            makeSpawns: function(): void {
                const map: Map<string, Spawn> = new Map();

                for (const ev of this.exp) {
                    const expID: number = ev.event.experienceID;
                    if (expID != Experience.SUNDERER_SPAWN_BONUS && expID != Experience.GENERIC_NPC_SPAWN) {
                        continue;
                    }

                    let spawn: Spawn;

                    if (expID == Experience.SUNDERER_SPAWN_BONUS) {
                        if (map.has(ev.event.otherID) == false) {
                            spawn = {
                                id: ev.event.otherID,
                                type: "sunderer",
                                spawns: 0,
                                firstSpawn: ev.event.timestamp,
                                lastSpawn: ev.event.timestamp
                            };
                            map.set(spawn.id, spawn);
                            console.log(`SessionViewerSpawns> new sunderer`);
                        }
                    } else if (expID == Experience.GENERIC_NPC_SPAWN) {
                        if (map.has(ev.event.otherID) == false) {
                            spawn = {
                                id: ev.event.otherID,
                                type: "router",
                                spawns: 0,
                                firstSpawn: ev.event.timestamp,
                                lastSpawn: ev.event.timestamp
                            };
                            map.set(spawn.id, spawn);
                            console.log(`SessionViewerSpawns> new router`);
                        }
                    }

                    spawn = map.get(ev.event.otherID)!;
                    ++spawn.spawns;
                    if (spawn.lastSpawn < ev.event.timestamp) {
                        spawn.lastSpawn = ev.event.timestamp;
                    }
                }

                this.spawns = Array.from(map.values());
            }
        },

        components: {

        }
    });
    export default SessionViewerSpawns;
</script>