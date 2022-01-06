<template>
    <div>
        <h2 class="wt-header d-flex">
            <span>
                Winter Leaderboard
            </span>

            <span class="flex-grow-1">

            </span>

            <button type="button" class="btn btn-small border" :class="[ settings.showFunNames == true ? 'btn-primary' : 'btn-secondary' ]" @click="settings.showFunNames = !settings.showFunNames">
                Use fun names
            </button>

            <span class="btn-group">
                <button type="button" class="btn btn-small border" :class="[ show.kills ? 'btn-primary' : 'btn-secondary' ]" @click="show.kills = !show.kills">
                    Kills
                </button>

                <button type="button" class="btn btn-small border" :class="[ show.support ? 'btn-primary' : 'btn-secondary' ]" @click="show.support = !show.support">
                    Support
                </button>

                <button type="button" class="btn btn-small border" :class="[ show.spawns ? 'btn-primary' : 'btn-secondary' ]" @click="show.spawns = !show.spawns">
                    Spawns
                </button>

                <button type="button" class="btn btn-small border" :class="[ show.vehicleKills ? 'btn-primary' : 'btn-secondary' ]" @click="show.vehicleKills = !show.vehicleKills">
                    Vehicle kills
                </button>

                <button type="button" class="btn btn-small border" :class="[ show.misc ? 'btn-primary' : 'btn-secondary' ]" @click="show.misc = !show.misc">
                    Misc
                </button>
            </span>
        </h2>

        <winter-section v-if="show.kills" :category="catKills" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
        <winter-section v-if="show.support" :category="catSupport" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
        <winter-section v-if="show.spawns" :category="catSpawns" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
        <winter-section v-if="show.vehicleKills" :category="catVehicleKills" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
        <winter-section v-if="show.misc" :category="catMisc" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata } from "../Report";

    import WinterCard from "./winter/WinterCard.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";

    const WinterSection = Vue.extend({
        props: {
            category: { type: Object as PropType<WinterCategory>, required: true },
            ShowFunNames: { type: Boolean, required: true },
            size: { type: Number, required: true }
        },

        template: `
            <div>
                <h4>{{category.name}}</h4>

                <div class="d-flex flex-row">
                    <winter-card v-for="metric in category.metrics" :key="metric.name"
                        :card="metric" :show-fun-name="ShowFunNames" :size="size">
                    </winter-card>
                </div>
            </div>
        `,

        components: {
            WinterCard
        }
    });

    class WinterMetric {
        public name: string = "";
        public funName: string = "";
        public description: string = "";
        public entries: WinterEntry[] = [];
    }

    class WinterCategory {

        public constructor(name: string) {
            this.name = name;
        }

        public name: string = "";
        public metrics: WinterMetric[] = [];
    }

    class WinterEntry {
        public characterID: string = "";
        public name: string = "";
        public value: number = 0;
        public display: string | null = null;
    }

    export const ReportWinter = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                settings: {
                    size: 5 as number,
                    showFunNames: false as boolean
                },

                show: {
                    kills: true as boolean,
                    support: true as boolean,
                    spawns: true as boolean,
                    vehicleKills: false as boolean,
                    misc: true as boolean
                },

                categories: [
                    new WinterCategory("Kills") as WinterCategory,
                    new WinterCategory("Support") as WinterCategory,
                    new WinterCategory("Spawns") as WinterCategory,
                    new WinterCategory("Vehicle kills") as WinterCategory,
                    new WinterCategory("Misc") as WinterCategory,
                ] as WinterCategory[],

                essential: [] as WinterMetric[],
                fun: [] as WinterMetric[]
            }
        },

        mounted: function(): void {
            this.makeAll();
        },

        methods: {

            makeAll: function(): void {
                this.makeKills();
                this.makeKpm();
                this.makeAssists();

                this.makeHeals();
                this.makeRevives();
                this.makeResupplies();
                this.makeMaxRepairs();
                this.makeShieldRepairs();

                this.makeSpawns();
                this.makeSundySpawns();
                this.makeRouterSpawns();
                this.makeRoutersPlaced();
                this.makeSpawnsPerRouter();
            },

            makeKills: function(): void {
                const metric: WinterMetric = new WinterMetric();
                metric.name = "Kills";
                metric.funName = "Kills";
                metric.description = "Most kills (per minute)";

                const map: Map<string, WinterEntry> = new Map();

                for (const kill of this.report.kills) {
                    if (map.has(kill.attackerCharacterID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = kill.attackerCharacterID;

                        entry.name = this.getCharacterName(kill.attackerCharacterID);
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(kill.attackerCharacterID)!;
                    ++entry.value;
                    map.set(kill.attackerCharacterID, entry);
                }

                const entries: WinterEntry[] = Array.from(map.values());
                for (const entry of entries) {
                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                    if (metadata != undefined) {
                        entry.display = `${entry.value} (${(entry.value / Math.max(1, metadata.timeAs) * 60).toFixed(2)})`;
                    }
                }

                metric.entries = entries.sort((a, b) => b.value - a.value);

                this.catKills.metrics.push(metric);
            },

            makeAssists: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Assists";
                metric.funName = "Assists";
                metric.description = "Highest assists (per minute)";

                this.catKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.ASSIST, Experience.HIGH_PRIORITY_ASSIST, Experience.PRIORITY_ASSIST, Experience.SPAWN_ASSIST],
                    (metadata) => metadata.timeAs)
                );
            },

            makeHeals: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Heals";
                metric.funName = "Green Wizard";
                metric.description = "Most heals (per minute)";
                this.catSupport.metrics.push(this.generateExperience(metric, [Experience.HEAL, Experience.SQUAD_HEAL], (metadata) => metadata.classes.medic.timeAs));
            },

            makeRevives: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Revives";
                metric.funName = "Necromancer";
                metric.description = "Most revives (per minute)";

                this.catSupport.metrics.push(this.generateExperience(metric, [Experience.REVIVE, Experience.SQUAD_REVIVE], (metadata) => metadata.classes.medic.timeAs));
            },

            makeResupplies: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Resupplies";
                metric.funName = "Ammo printer";
                metric.description = "Most resupplies (per minute)";

                this.catSupport.metrics.push(this.generateExperience(metric, [Experience.RESUPPLY, Experience.SQUAD_RESUPPLY], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeMaxRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX repairs";
                metric.funName = "Welder";
                metric.description = "Most MAX repairs (per minute)";

                this.catSupport.metrics.push(this.generateExperience(metric, [Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeShieldRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Shield repairs";
                metric.funName = "Shield battery";
                metric.description = "Most shield repairs (per minute)";

                this.catSupport.metrics.push(this.generateExperience(metric, [Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR], (metadata) => metadata.classes.medic.timeAs));
            },

            makeSpawns: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Spawns";
                metric.funName = "Spawns";
                metric.description = "Most spawns (per minute)";

                this.catSpawns.metrics.push(this.generateExperience(
                    metric,
                    [Experience.GALAXY_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN, Experience.SQUAD_SPAWN, Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS],
                    (metadata) => metadata.timeAs)
                );
            },

            makeRouterSpawns: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Router spawns";
                metric.funName = "Network provider";
                metric.description = "Most router spawns (per minute)";

                this.catSpawns.metrics.push(this.generateExperience(
                    metric,
                    [ Experience.GENERIC_NPC_SPAWN ],
                    (metadata) => metadata.timeAs)
                );
            },

            makeSundySpawns: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Sundy spawns";
                metric.funName = "Cat herder";
                metric.description = "Most sundy spawns (per minute)";

                this.catSpawns.metrics.push(this.generateExperience(
                    metric,
                    [Experience.SUNDERER_SPAWN_BONUS],
                    (metadata) => metadata.timeAs)
                );
            },

            makeRoutersPlaced: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Routers placed";
                metric.funName = "Routers placed";
                metric.description = "Most routers placed (per hour)";

                const map: Map<string, Set<string>> = new Map();

                for (const ev of this.report.experience) {
                    if (ev.experienceID != Experience.GENERIC_NPC_SPAWN) {
                        continue;
                    }

                    if (map.has(ev.sourceID) == false) {
                        map.set(ev.sourceID, new Set());
                    }

                    const set: Set<string> = map.get(ev.sourceID)!;
                    set.add(ev.otherID);
                }

                const metrics: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.value = iter[1].size;
                        entry.name = this.getCharacterName(iter[0]);

                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata != undefined) {
                            entry.display = `${entry.value} (${(entry.value / metadata.timeAs * 60 * 60).toFixed(2)})`;
                        }
                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = metrics;

                this.catSpawns.metrics.push(metric);
            },

            makeSpawnsPerRouter: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Spawns per router";
                metric.funName = "Spawns per router";
                metric.description = "Spawns per router (# placed)";

                const map: Map<string, Map<string, number>> = new Map();

                for (const ev of this.report.experience) {
                    if (ev.experienceID != Experience.GENERIC_NPC_SPAWN) {
                        continue;
                    }

                    if (map.has(ev.sourceID) == false) {
                        map.set(ev.sourceID, new Map());
                    }

                    const charMap: Map<string, number> = map.get(ev.sourceID)!;

                    if (charMap.has(ev.otherID) == false) {
                        charMap.set(ev.otherID, 0);
                    }

                    charMap.set(ev.otherID, (charMap.get(ev.otherID) || 0) + 1);
                }

                const entries: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.name = this.getCharacterName(entry.characterID);

                        let routers: number = iter[1].size;
                        let count: number = 0;

                        Array.from(iter[1].entries()).map(aa => {
                            count += aa[1];
                        });

                        entry.value = count / Math.max(1, routers);
                        entry.display = `${entry.value.toFixed(2)} (${routers})`;

                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = entries;
                this.catSpawns.metrics.push(metric);
            },

            makeKpm: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "KPM";
                metric.funName = "Speed gunner";
                metric.description = "Highest KPM";

                const map: Map<string, number> = new Map();

                for (const kill of this.report.kills) {
                    const id: string = kill.attackerCharacterID;
                    map.set(id, (map.get(id) || 0) + 1);
                }

                map.forEach((kills: number, charID: string) => {
                    if (kills < 25) {
                        return;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(charID);
                    if (metadata == undefined) {
                        console.warn(`Missing metadata for ${charID}`);
                        return;
                    }

                    const entry: WinterEntry = new WinterEntry();
                    entry.characterID = charID;
                    entry.name = this.getCharacterName(charID);
                    entry.value = kills / Math.max(1, metadata.timeAs) * 60;
                    entry.display = entry.value.toFixed(2);

                    metric.entries.push(entry);
                });

                metric.entries.sort((a, b) => b.value - a.value);

                this.catKills.metrics.push(metric);
            },

            generateExperience: function(metric: WinterMetric, expIDs: number[], perMinuteSelector: ((metadata: PlayerMetadata) => number) | null = null): WinterMetric {
                const map: Map<string, WinterEntry> = new Map();

                for (const exp of this.report.experience) {
                    if (expIDs.indexOf(exp.experienceID) == -1) {
                        continue;
                    }

                    if (map.has(exp.sourceID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = exp.sourceID;

                        entry.name = this.getCharacterName(exp.sourceID);
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(exp.sourceID)!;
                    ++entry.value;
                    map.set(exp.sourceID, entry);
                }

                const entries: WinterEntry[] = Array.from(map.values());
                if (perMinuteSelector != null) {
                    for (const entry of entries) {
                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata != undefined) {
                            const minutes: number = perMinuteSelector(metadata);
                            entry.display = `${entry.value} (${(entry.value / Math.max(1, minutes) * 60).toFixed(2)})`;
                        }
                    }
                }

                metric.entries = entries.sort((a, b) => b.value - a.value);

                return metric;
            },

            getCharacterName(charID: string): string {
                const character: PsCharacter | null = this.report.characters.get(charID) || null;
                return character != null ? `${character.outfitID != null ? `[${character.outfitTag}] ` : ""}${character.name}` : `<missing ${charID}>`;
            }
        },

        computed: {
            catKills: function(): WinterCategory {
                return this.categories[0];
            },
            catSupport: function(): WinterCategory {
                return this.categories[1];
            },
            catSpawns: function(): WinterCategory {
                return this.categories[2];
            },
            catVehicleKills: function(): WinterCategory {
                return this.categories[3];
            },
            catMisc: function(): WinterCategory {
                return this.categories[4];
            },
        },

        components: {
            WinterCard,
            WinterSection
        }
    });

    export default ReportWinter;
</script>