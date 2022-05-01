<template>

    <collapsible header-text="Winter Leaderboard">
        <template v-slot:header>
            <div style="display: inline-flex; flex-grow: 1; align-items: center;">
                <div class="flex-grow-1"></div>

                <select class="form-control flex-grow-0 mr-2" v-model.number="settings.size" style="width: 12ch;" @click.stop>
                    <option :value="5">5</option>
                    <option :value="8">8</option>
                    <option :value="10">10</option>
                    <option :value="12">12</option>
                </select>

                <button type="button" class="btn btn-small border mr-2" :class="[ settings.showFunNames == true ? 'btn-primary' : 'btn-secondary' ]" @click.stop="settings.showFunNames = !settings.showFunNames">
                    Use fun names
                </button>

                <span class="btn-group">
                    <toggle-button v-model="show.kills">
                        Kills
                    </toggle-button>

                    <toggle-button v-model="show.support">
                        Support
                    </toggle-button>

                    <toggle-button v-model="show.spawns">
                        Spawns
                    </toggle-button>

                    <toggle-button v-model="show.weaponTypes">
                        Weapon types
                    </toggle-button>

                    <toggle-button v-model="show.vehicleKills">
                        Vehicle kills
                    </toggle-button>

                    <toggle-button v-model="show.vehicleSupport">
                        Vehicle support
                    </toggle-button>

                    <toggle-button v-model="show.misc">
                        Misc
                    </toggle-button>
                </span>
            </div>
        </template>

        <template v-slot:default>
            <winter-section v-if="show.kills" :category="catKills" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.support" :category="catSupport" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.spawns" :category="catSpawns" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.weaponTypes" :category="catWeaponTypes" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.vehicleKills" :category="catVehicleKills" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.vehicleSupport" :category="catVehicleSupport" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
            <winter-section v-if="show.misc" :category="catMisc" :show-fun-names="settings.showFunNames" :size="settings.size"></winter-section>
        </template>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata } from "../Report";

    import ToggleButton from "components/ToggleButton";
    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";

    import WinterCard from "./winter/WinterCard.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { Experience, ExpEvent } from "api/ExpStatApi";
    import { KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";

    import TimeUtils from "util/Time";
    import LoadoutUtils from "util/Loadout";

    const WinterSection = Vue.extend({
        props: {
            category: { type: Object as PropType<WinterCategory>, required: true },
            ShowFunNames: { type: Boolean, required: true },
            size: { type: Number, required: true }
        },

        template: `
            <div>
                <div class="d-flex flex-row">
                    <template v-for="metric in category.metrics" :key="metric.name">
                        <winter-card v-if="metric.entries.length > 0" :card="metric" :show-fun-name="ShowFunNames" :size="size"></winter-card>
                    </template>
                </div>
            </div>
        `,

        components: {
            WinterCard, InfoHover
        }
    });

    class WinterMetric {
        public name: string = "";
        public funName: string = "";
        public description: string = "";
        public entries: WinterEntry[] = [];
        public availableAfter: string | null = null;
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
                    vehicleKills: true as boolean,
                    vehicleSupport: true as boolean,
                    weaponTypes: true as boolean,
                    misc: true as boolean
                },

                categories: [
                    new WinterCategory("Kills") as WinterCategory,
                    new WinterCategory("Support") as WinterCategory,
                    new WinterCategory("Spawns") as WinterCategory,
                    new WinterCategory("Weapon types") as WinterCategory,
                    new WinterCategory("Vehicle kills") as WinterCategory,
                    new WinterCategory("Vehicle support") as WinterCategory,
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
                this.makeKD();
                this.makeHSR();
                this.makeAssists();
                this.makeMostMaxKills();

                this.makeHeals();
                this.makeRevives();
                this.makeShieldRepairs();
                this.makeResupplies();
                this.makeMaxRepairs();
                this.makeHardlightAssists();

                this.makeSpawns();
                this.makeSundySpawns();
                this.makeSundiesPlaced();
                this.makeSpawnsPerSundy();
                this.makeRouterSpawns();
                this.makeRoutersPlaced();
                this.makeSpawnsPerRouter();

                this.makeVehicleKills();
                this.makeFlashKills();
                this.makeHarasserKills();
                this.makeLightningKills();
                this.makeMBTKills();
                this.makeESFKills();
                this.makeLiberatorKills();

                this.makeVehicleRepair();
                this.makeVehicleResupply();

                this.makeMostUniqueWeapons();
                this.makeC4Kills();
                this.makeKnifeKills();
                this.makePistolKills();
                this.makeLauncherKills();

                this.makeAverageLifetime();
                this.makeRoadkills();
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
                metric.funName = "Wingman";
                metric.description = "Highest assists (per minute)";

                this.catKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.ASSIST, Experience.HIGH_PRIORITY_ASSIST, Experience.PRIORITY_ASSIST, Experience.SPAWN_ASSIST],
                    (metadata) => metadata.timeAs)
                );
            },

            makeMostMaxKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX kills";
                metric.funName = "Bad mechanic";
                metric.description = "Most MAX kills (per hour)";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.kills) {
                    if (LoadoutUtils.isMax(ev.killedLoadoutID) == true) {
                        map.set(ev.attackerCharacterID, (map.get(ev.attackerCharacterID) || 0) + 1);
                    }
                }

                const metrics: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.value = iter[1];
                        entry.name = this.getCharacterName(iter[0]);

                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata != undefined) {
                            entry.display = `${entry.value} (${(entry.value / metadata.timeAs * 60 * 60).toFixed(2)})`;
                        }
                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = metrics;

                this.catMisc.metrics.push(metric);
            },

            makeRoadkills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Road kills";
                metric.funName = "Road kills";
                metric.description = "Most road kills (per hour)";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.kills) {
                    if (ev.attackerVehicleID != 0 && ev.weaponID == 0) {
                        map.set(ev.attackerCharacterID, (map.get(ev.attackerCharacterID) || 0) + 1);
                    }
                }

                const metrics: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.value = iter[1];
                        entry.name = this.getCharacterName(iter[0]);

                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata != undefined) {
                            entry.display = `${entry.value} (${(entry.value / metadata.timeAs * 60 * 60).toFixed(2)})`;
                        }
                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = metrics;

                this.catMisc.metrics.push(metric);
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

            makeHardlightAssists: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Hardlight Assists";
                metric.funName = "Brick layer";
                metric.description = "Most draw fire assists (per minute)";
                metric.availableAfter = "2022-05-02";

                this.catSupport.metrics.push(this.generateExperience(metric, [1393], (metadata) => metadata.classes.engineer.timeAs));
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
                metric.funName = "Mother";
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
                    [Experience.GENERIC_NPC_SPAWN],
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
                metric.funName = "Network installer";
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

            makeSundiesPlaced: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Sunderers placed";
                metric.funName = "Bus stop builder";
                metric.description = "Most sunderers deployed (per hour)";

                const map: Map<string, Set<string>> = new Map();

                for (const ev of this.report.experience) {
                    if (ev.experienceID != Experience.SUNDERER_SPAWN_BONUS) {
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
                metric.funName = "Zerg in a can";
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

            makeSpawnsPerSundy: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Spawns per bus";
                metric.funName = "Bus stops made";
                metric.description = "Spawns per sundy (# placed)";

                const map: Map<string, Map<string, number>> = new Map();

                for (const ev of this.report.experience) {
                    if (ev.experienceID != Experience.SUNDERER_SPAWN_BONUS) {
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

            makeHSR: function(): void {
                const metric: WinterMetric = new WinterMetric();
                metric.name = "Headshots";
                metric.funName = "Head popper";
                metric.description = "Highest HSR";

                for (const player of this.report.players) {
                    const kills: KillEvent[] = this.report.kills.filter(iter => iter.attackerCharacterID == player);
                    if (kills.length < 25) {
                        continue;
                    }

                    const headshots: KillEvent[] = kills.filter(iter => iter.isHeadshot == true);

                    const entry: WinterEntry = new WinterEntry();
                    entry.characterID = player;
                    entry.name = this.getCharacterName(player);
                    entry.value = headshots.length / Math.max(1, kills.length);
                    entry.display = `${(entry.value * 100).toFixed(2)}% (${headshots.length}/${kills.length})`;

                    metric.entries.push(entry);
                }

                metric.entries.sort((a, b) => b.value - a.value);

                this.catKills.metrics.push(metric);
            },

            makeKD: function(): void {
                const metric: WinterMetric = new WinterMetric();
                metric.name = "KDR";
                metric.funName = "KDR";
                metric.description = "Highest KDR";

                for (const player of this.report.players) {
                    const kills: KillEvent[] = this.report.kills.filter(iter => iter.attackerCharacterID == player);
                    if (kills.length < 25) {
                        continue;
                    }

                    const deaths: KillEvent[] = this.report.deaths.filter(iter => iter.killedCharacterID == player);

                    const entry: WinterEntry = new WinterEntry();
                    entry.characterID = player;
                    entry.name = this.getCharacterName(player);
                    entry.value = kills.length / Math.max(1, deaths.length);
                    entry.display = `${(entry.value).toFixed(2)} (${kills.length}/${deaths.length})`;

                    metric.entries.push(entry);
                }

                metric.entries.sort((a, b) => b.value - a.value);

                this.catKills.metrics.push(metric);
            },

            makeVehicleKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Vehicle kills";
                metric.funName = "Vehicle kills";
                metric.description = "Most vehicle kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [
                        Experience.VKILL_FLASH, Experience.VKILL_HARASSER, Experience.VKILL_LIGHTNING, Experience.VKILL_ANT,
                        Experience.VKILL_MOSQUITO, Experience.VKILL_REAVER, Experience.VKILL_SCYTHE, Experience.VKILL_DERVISH,
                        Experience.VKILL_VALKYRIE, Experience.VKILL_LIBERATOR, Experience.VKILL_GALAXY,
                        Experience.VKILL_PROWLER, Experience.VKILL_VANGUARD, Experience.VKILL_PROWLER, Experience.VKILL_CHIMERA,
                        Experience.VKILL_COLOSSUS, Experience.VKILL_JAVELIN
                    ],
                    (metadata) => metadata.timeAs)
                );
            },

            makeFlashKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Flash kills";
                metric.funName = "Flash kills";
                metric.description = "Most flash kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_FLASH, Experience.VKILL_JAVELIN],
                    (metadata) => metadata.timeAs)
                );
            },

            makeLightningKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Lightning kills";
                metric.funName = "Lightning kills";
                metric.description = "Most lightning kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_LIGHTNING],
                    (metadata) => metadata.timeAs)
                );
            },

            makeMBTKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MBT kills";
                metric.funName = "MBT kills";
                metric.description = "Most MBT kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_MAGRIDER, Experience.VKILL_VANGUARD, Experience.VKILL_PROWLER, Experience.VKILL_CHIMERA],
                    (metadata) => metadata.timeAs)
                );
            },

            makeHarasserKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Harasser kills";
                metric.funName = "Harasser kills";
                metric.description = "Most harasser kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_HARASSER],
                    (metadata) => metadata.timeAs)
                );
            },

            makeESFKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "ESF kills";
                metric.funName = "ESF kills";
                metric.description = "Most ESF kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_MOSQUITO, Experience.VKILL_SCYTHE, Experience.VKILL_REAVER, Experience.VKILL_DERVISH],
                    (metadata) => metadata.timeAs)
                );
            },

            makeLiberatorKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Liberator kills";
                metric.funName = "Liberator kills";
                metric.description = "Most liberator kills (per minute)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_LIBERATOR],
                    (metadata) => metadata.timeAs)
                );
            },

            makeMostUniqueWeapons: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Most unique weapons";
                metric.funName = "Diverse skillset";
                metric.description = "Most amount of unique weapons";

                const map: Map<string, Set<number>> = new Map();

                for (const kill of this.report.kills) {
                    const charID: string = kill.attackerCharacterID;

                    if (map.has(charID) == false) {
                        map.set(charID, new Set());
                    }

                    const set: Set<number> = map.get(charID)!;
                    set.add(kill.weaponID);

                    map.set(charID, set);
                }

                const entries: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.name = this.getCharacterName(entry.characterID);
                        entry.value = iter[1].size;

                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = entries;

                this.catMisc.metrics.push(metric);
            },

            makeC4Kills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "C4 kills";
                metric.funName = "Explosive tendencies";
                metric.description = "Most C4 kills";

                this.catWeaponTypes.metrics.push(this.generateWeaponKills(
                    metric,
                    [432, 800623, 6009782], // c4, aurax c4, christmax c4
                    (metadata) => metadata.timeAs)
                );
            },

            makeKnifeKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Knife kills";
                metric.funName = "Slasher";
                metric.description = "Most knife kills";

                this.catWeaponTypes.metrics.push(this.generateWeaponCategoryKills(
                    metric,
                    2, // knife
                    (metadata) => metadata.timeAs
                ));
            },

            makePistolKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Pistol kills";
                metric.funName = "The cowboy";
                metric.description = "Most pistol kills";

                this.catWeaponTypes.metrics.push(this.generateWeaponCategoryKills(
                    metric,
                    3, // pistol
                    (metadata) => metadata.timeAs
                ));
            },

            makeLauncherKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Launcher kills";
                metric.funName = "NASA engineer";
                metric.description = "Most launcher kills";

                this.catWeaponTypes.metrics.push(this.generateWeaponCategoryKills(
                    metric,
                    13, // pistol
                    (metadata) => metadata.timeAs
                ));
            },

            makeAverageLifetime: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Longest life expectancy";
                metric.funName = "Elders";
                metric.description = "Longest average life expectancy";

                for (const player of this.report.players) {
                    // Get the first event of the player
                    const firstKill: KillEvent | null = this.report.kills.find(iter => iter.attackerCharacterID == player) || null;
                    const firstKillTime: number = firstKill?.timestamp.getTime() ?? 0;
                    const firstExp: ExpEvent | null = this.report.experience.find(iter => iter.sourceID == player) || null;
                    const firstExpTime: number = firstExp?.timestamp.getTime() ?? 0;

                    let firstEvent: KillEvent | ExpEvent | null = (firstKillTime < firstExpTime) ? firstKill : firstExp;

                    // Player had no events
                    if (firstEvent == null) {
                        continue;
                    }

                    const deaths: KillEvent[] = this.report.deaths.filter(iter => iter.killedCharacterID == player);
                    if (deaths.length == 0) {
                        continue;
                    }

                    const lifetimes: number[] = [];

                    let start: number = firstEvent.timestamp.getTime();
                    for (const death of deaths) {
                        lifetimes.push((death.timestamp.getTime() - start) / 1000);
                        start = death.timestamp.getTime();
                    }

                    // Don't have someone who joined then left
                    if (lifetimes.length < 2) {
                        continue;
                    }

                    const total: number = lifetimes.reduce((acc, val) => acc += val, 0);
                    const avg: number = total / lifetimes.length;

                    const entry: WinterEntry = new WinterEntry();
                    entry.characterID = player;
                    entry.name = this.getCharacterName(entry.characterID);
                    entry.value = avg;
                    entry.display = TimeUtils.duration(entry.value);

                    metric.entries.push(entry);
                }

                metric.entries.sort((a, b) => b.value - a.value);

                this.catMisc.metrics.push(metric);
            },

            makeVehicleRepair: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Vehicle repairs";
                metric.funName = "Road mechanic";
                metric.description = "Most vehicle repairs";

                this.catVehicleSupport.metrics.push(this.generateExperience(
                    metric,
                    [...Experience.VehicleRepairs, ...Experience.SquadVehicleRepairs],
                    (metadata) => metadata.timeAs)
                );
            },

            makeVehicleResupply: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Vehicle resupplies";
                metric.funName = "Vehicle resupplies";
                metric.description = "Most vehicle resupplies";

                this.catVehicleSupport.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VEHICLE_RESUPPLY, Experience.SQUAD_VEHICLE_RESUPPLY],
                    (metadata) => metadata.timeAs)
                );
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

            generateWeaponKills: function(metric: WinterMetric, weaponIDs: number[], perMinuteSelector: ((metadata: PlayerMetadata) => number) | null = null): WinterMetric {
                const map: Map<string, WinterEntry> = new Map();

                for (const kill of this.report.kills) {
                    if (weaponIDs.indexOf(kill.weaponID) == -1) {
                        continue;
                    }

                    if (map.has(kill.attackerCharacterID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = kill.attackerCharacterID;
                        entry.name = this.getCharacterName(entry.characterID);
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(kill.attackerCharacterID)!;
                    ++entry.value;
                    map.set(kill.attackerCharacterID, entry);
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

            generateWeaponCategoryKills: function(metric: WinterMetric, itemCategoryID: number, perMinuteSelector: ((metadata: PlayerMetadata) => number) | null = null): WinterMetric {
                const map: Map<string, WinterEntry> = new Map();

                for (const kill of this.report.kills) {
                    const item: PsItem | null = this.report.items.get(kill.weaponID) || null;
                    if (item == null) {
                        continue;
                    }

                    if (item.categoryID != itemCategoryID) {
                        continue;
                    }

                    if (map.has(kill.attackerCharacterID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = kill.attackerCharacterID;
                        entry.name = this.getCharacterName(entry.characterID);
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(kill.attackerCharacterID)!;
                    ++entry.value;
                    map.set(kill.attackerCharacterID, entry);
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
            catWeaponTypes: function(): WinterCategory {
                return this.categories[3];
            },
            catVehicleKills: function(): WinterCategory {
                return this.categories[4];
            },
            catVehicleSupport: function(): WinterCategory {
                return this.categories[5];
            },
            catMisc: function(): WinterCategory {
                return this.categories[6];
            },
        },

        components: {
            WinterCard,
            WinterSection,
            ToggleButton,
            Collapsible
        }
    });

    export default ReportWinter;
</script>