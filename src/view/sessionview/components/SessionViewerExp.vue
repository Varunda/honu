<template>
    <div>

        <div v-if="spawns.length > 0">
            <h3>Spawns</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerSpawns"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerSpawns"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                </div>

                <div class="flex-grow-1 flex-basis-0">
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="playerVehicleKills.length > 0">
            <h3>Vehicle kills</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerVehicleKills"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerVehicleKills"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="heals.length > 0" class="mw-100">
            <h3>Heals</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerHeals"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerHeals"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitHeals"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitHeals"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="revives.length > 0">
            <h3>Revives</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerRevives"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerRevives"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitRevives"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitRevives"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="resupplies.length > 0">
            <h3>Resupplies</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerResupplies"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerResupplies"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitResupplies"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitResupplies"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="repairs.length > 0">
            <h3>MAX Repairs</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerRepairs"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitRepairs"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

        <div v-if="shieldRepairs.length > 0">
            <h3>Shield Repairs</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerShieldRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerShieldRepairs"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitShieldRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitShieldRepairs"></chart-entry-list>
                </div>
            </div>

            <hr class="border" />
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import ChartEntryPie from "./ChartEntryPie.vue";
    import ChartEntryList from "./ChartEntryList.vue";

    import { Session } from "api/SessionApi";
    import { ExpandedExpEvent, Experience, ExperienceBlock, ExpEvent } from "api/ExpStatApi";
    import { PsCharacter } from "api/CharacterApi";

    interface Entry {
        display: string;
        count: number;
        link: string | null;
    }

    export const SessionViewerExp = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Object as PropType<ExperienceBlock>, required: true },
            FullExp: { type: Boolean, required: true }
        },

        data: function() {
            return {
                playerHeals: [] as Entry[],
                outfitHeals: [] as Entry[],

                playerRevives: [] as Entry[],
                outfitRevives: [] as Entry[],

                playerResupplies: [] as Entry[],
                outfitResupplies: [] as Entry[],

                playerRepairs: [] as Entry[],
                outfitRepairs: [] as Entry[],

                playerShieldRepairs: [] as Entry[],
                outfitShieldRepairs: [] as Entry[],

                playerSpawns: [] as Entry[],
                playerVehicleKills: [] as Entry[]
            }
        },

        mounted: function(): void {
            this.generateAll();
        },

        methods: {
            generateAll: function(): void {
                this.generateHealEntries();
                this.generateReviveEntries();
                this.generateResupplyEntries();
                this.generateRepairEntries();
                this.generateShieldRepairEntries();
                this.generateSpawnEntries();
            },

            generateSpawnEntries: function(): void {
                const sundy: Entry = { display: "Sundy", count: this.exp.events.filter(iter => iter.experienceID == Experience.SUNDERER_SPAWN_BONUS).length, link: null };
                const router: Entry = { display: "Router", count: this.exp.events.filter(iter => iter.experienceID == Experience.GENERIC_NPC_SPAWN).length, link: null };
                const squadSpawn: Entry = { display: "Squad spawn", count: this.exp.events.filter(iter => iter.experienceID == Experience.SQUAD_SPAWN).length, link: null };
                const vehicleSpawn: Entry = { display: "Squad vehicle spawn", count: this.exp.events.filter(iter => iter.experienceID == Experience.SQUAD_VEHICLE_SPAWN_BONUS).length, link: null };

                this.playerSpawns = [sundy, router, squadSpawn, vehicleSpawn];
            },

            generateVehicleKillEntries: function(): void {
                const exp: ExpEvent[] = this.exp.events;

                function make(name: string, expID: number): Entry {
                    return {
                        display: name,
                        count: exp.filter(iter => iter.experienceID == expID).length,
                        link: null
                    }
                }

                const flash = make("Flash", Experience.VKILL_FLASH);
                const galaxy = make("Galaxy", Experience.VKILL_GALAXY);
                const lib = make("Liberator", Experience.VKILL_LIBERATOR);
                const lightning = make("Lightning", Experience.VKILL_LIGHTNING);
                const magrider = make("Magrider", Experience.VKILL_MAGRIDER);
                const mosquito = make("Mosquito", Experience.VKILL_MOSQUITO);
                const prowler = make("Prowler", Experience.VKILL_PROWLER);
                const reaver = make("Reaver", Experience.VKILL_REAVER);
                const scythe = make("Scythe", Experience.VKILL_SCYTHE);
                const vanguard = make("Vanguard", Experience.VKILL_VANGUARD);
                const harasser = make("Harasser", Experience.VKILL_HARASSER);
                const valkyrie = make("Valkyrie", Experience.VKILL_VALKYRIE);
                const ant = make("Ant", Experience.VKILL_ANT);
                const colossus = make("Colossus", Experience.VKILL_COLOSSUS);
                const javelin = make("Javelin", Experience.VKILL_JAVELIN);
                const chimera = make("Chimera", Experience.VKILL_CHIMERA);
                const dervish = make("Dervish", Experience.VKILL_DERVISH);

                this.playerVehicleKills = [
                    flash, galaxy, lib, lightning, magrider, mosquito,
                    prowler, reaver, scythe, vanguard, harasser, valkyrie,
                    ant, colossus, javelin, chimera, dervish
                ].filter(iter => iter.count > 0).sort((a, b) => b.count - a.count);
            },

            generateHealEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.heals);
                this.playerHeals = set[0];
                this.outfitHeals = set[1];
            },

            generateReviveEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.revives);
                this.playerRevives = set[0];
                this.outfitRevives = set[1];
            },

            generateResupplyEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.resupplies);
                this.playerResupplies = set[0];
                this.outfitResupplies = set[1];
            },

            generateRepairEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.repairs);
                this.playerRepairs = set[0];
                this.outfitRepairs = set[1];
            },

            generateShieldRepairEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.shieldRepairs);
                this.playerShieldRepairs = set[0];
                this.outfitShieldRepairs = set[1];
            },

            generatePlayerAndOutfitExp(events: ExpEvent[]): [Entry[], Entry[]] {
                const charMap: Map<string, Entry> = new Map();
                const outfitMap: Map<string, Entry> = new Map();

                for (const ev of events) {
                    if (ev.otherID.length != 19) {
                        continue;
                    }

                    const c: PsCharacter | null = this.exp.characters.find(iter => iter.id == ev.otherID) || null;

                    if (charMap.has(ev.otherID) == false) {
                        charMap.set(ev.otherID, {
                            display: c == null ? `<missing ${ev.otherID}>` : `${(c.outfitID ? `[${c.outfitTag}] ` : "")}${c.name}`,
                            count: 0,
                            link: `/c/${ev.otherID}`
                        });
                    }
                    ++charMap.get(ev.otherID)!.count;

                    if (c != null) {
                        if (outfitMap.has(c.outfitID ?? "0") == false) {
                            let entry: Entry = {
                                display: "<no outfit>",
                                count: 0,
                                link: (c.outfitID == null) ? null : `/o/${c.outfitID}`
                            };

                            if (c.outfitID != null) {
                                entry.display = `[${c.outfitTag}] ${c.outfitName}`;
                            }
                            outfitMap.set(c.outfitID ?? "0", entry);
                        }

                        ++outfitMap.get(c.outfitID ?? "0")!.count;
                    }
                }

                const player = Array.from(charMap.values()).sort((a, b) => b.count - a.count);
                const outfit = Array.from(outfitMap.values()).sort((a, b) => b.count - a.count);

                return [player, outfit];
            }

        },

        computed: {
            heals: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isHeal(iter.experienceID));
            },

            revives: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isRevive(iter.experienceID));
            },

            resupplies: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isResupply(iter.experienceID));
            },

            repairs: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isMaxRepair(iter.experienceID));
            },

            shieldRepairs: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isShieldRepair(iter.experienceID));
            },

            spawns: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isSpawn(iter.experienceID));
            }

        },

        components: {
            ChartTimestamp,
            ChartEntryPie,
            ChartEntryList
        }
    });
    export default SessionViewerExp;
</script>