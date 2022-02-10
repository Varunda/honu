<template>
    <div>
        <a-table
            :entries="loadedSpawns" :paginate="false">

            <a-col>
                <a-header>
                    <b>Type</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.type == 'sunderer'">
                        Sunderer
                    </span>

                    <span v-else-if="entry.type == 'router'">
                        Router
                    </span>

                    <span v-else class="text-danger">
                        Unchecked type {{entry.type}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>First spawn</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.firstSpawn | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Time alive</b>
                </a-header>

                <a-body v-slot="entry">
                    {{(entry.lastSpawn.getTime() - entry.firstSpawn.getTime()) / 1000 | mduration}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Spawns</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.spawns}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Spawns per minute</b>
                </a-header>

                <a-body v-slot="entry">
                    {{(entry.spawns / ((entry.lastSpawn.getTime() - entry.firstSpawn.getTime()) / 1000 / 60)) | locale(2)}}
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import { ExpandedExpEvent, Experience } from "api/ExpStatApi";
    import { Session } from "api/SessionApi";

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

        computed: {
            loadedSpawns: function(): Loading<Spawn[]> {
                return Loadable.loaded(this.spawns);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            ToggleButton
        }
    });
    export default SessionViewerSpawns;
</script>