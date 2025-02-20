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

                <toggle-button v-model="settings.showFunNames" class="mr-2">
                    Show fun names
                </toggle-button>

                <toggle-button v-model="settings.showHeader" class="mr-2">
                    Show section headers
                </toggle-button>

                <span class="btn-group">
                    <toggle-button v-model="show.kills">
                        Kills
                    </toggle-button>

                    <toggle-button v-model="show.medic">
                        Medic
                    </toggle-button>

                    <toggle-button v-model="show.engi">
                        Engineer
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
            <winter-section v-if="show.kills" :category="catKills" :settings="settings"></winter-section>
            <winter-section v-if="show.medic" :category="catMedic" :settings="settings"></winter-section>
            <winter-section v-if="show.engi" :category="catEngi" :settings="settings"></winter-section>
            <winter-section v-if="show.spawns" :category="catSpawns" :settings="settings"></winter-section>
            <winter-section v-if="show.weaponTypes" :category="catWeaponTypes" :settings="settings"></winter-section>
            <winter-section v-if="show.vehicleKills" :category="catVehicleKills" :settings="settings"></winter-section>
            <winter-section v-if="show.vehicleSupport" :category="catVehicleSupport" :settings="settings"></winter-section>
            <winter-section v-if="show.misc" :category="catMisc" :settings="settings"></winter-section>
        </template>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata, ReportParameters } from "../Report";
    import { InfantryDamageEntry } from "../InfantryDamage";

    import ToggleButton from "components/ToggleButton";
    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";

    import WinterCard from "./winter/WinterCard.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { Experience, ExperienceType, ExpEvent } from "api/ExpStatApi";
    import { KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";

    import TimeUtils from "util/Time";
    import LoadoutUtils from "util/Loadout";
    import LocaleUtil from "util/Locale";
    import { BootstrapColor } from "util/Color";

    const WinterSection = Vue.extend({
        props: {
            category: { type: Object as PropType<WinterCategory>, required: true },
            settings: { type: Object, required: true },
        },

        template: `
            <div>
                <h4 v-if="settings.showHeader == true" class="wt-header" :style="'background-color: var(--' + (category.color || 'secondary') + ');'">
                    {{category.name}}
                </h4>

                <div class="d-flex flex-row flex-wrap">
                    <template v-for="metric in category.metrics">
                        <winter-card v-if="metric.entries.length > 0" :card="metric" :settings="settings"></winter-card>
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
        public helpText: string | null = null;
    }

    class WinterCategory {
        public constructor(name: string, color?: BootstrapColor) {
            this.name = name;
            if (color) {
                this.color = color;
            }
        }

        public name: string = "";
        public color: BootstrapColor = "secondary";
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
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                settings: {
                    size: 5 as number,
                    showFunNames: false as boolean,
                    showHeader: true as boolean
                },

                show: {
                    kills: true as boolean,
                    medic: true as boolean,
                    engi: true as boolean,
                    spawns: true as boolean,
                    vehicleKills: true as boolean,
                    vehicleSupport: true as boolean,
                    weaponTypes: true as boolean,
                    misc: true as boolean
                },

                categories: [
                    new WinterCategory("Kills", "primary") as WinterCategory,
                    new WinterCategory("Medic", "success") as WinterCategory,
                    new WinterCategory("Engineer", "indigo") as WinterCategory,
                    new WinterCategory("Spawns", "info") as WinterCategory,
                    new WinterCategory("Weapon types", "pink") as WinterCategory,
                    new WinterCategory("Vehicle kills", "blue") as WinterCategory,
                    new WinterCategory("Vehicle support", "cyan") as WinterCategory,
                    new WinterCategory("Misc", "purple") as WinterCategory,
                ] as WinterCategory[],

                essential: [] as WinterMetric[],
                fun: [] as WinterMetric[],

                compatFormat: Intl.NumberFormat(undefined, {
                    notation: "compact",
                    minimumFractionDigits: 1,
                    maximumFractionDigits: 1
                }) as Intl.NumberFormat
            }
        },

        mounted: function(): void {
            this.makeAll();
        },

        methods: {
            ///
            /// Woe unto the code below, for it is bad, but it gets the job done
            ///

            makeAll: function(): void {
                this.makeKills();
                this.makeKpm();
                this.makeKD();
                this.makeHSR();
                this.makeAssists();
                this.makeMostMaxKills();
                this.makeKillstreak();
                this.makeKillParticipation();
                this.makeSPM();
                this.makeScore();

                this.makeHeals();
                this.makeHealthHealed();
                this.makeRevives();
                this.makeShieldRepairs();
                this.makeShieldsHealed();

                this.makeResupplies();
                this.makeTotalRepairHealth();
                this.makeMaxRepairs();
                this.makeMaxRepairHealth();
                this.makeVehicleRepair();
                this.makeVehicleRepairHealth();
                this.makeVehicleResupply();
                this.makeHardlightAssists();
                this.makeManaKills();

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
                this.makeValkKills();
                this.makeLiberatorKills();
                this.makeGalaxyKills();

                this.makeMostUniqueWeapons();
                this.makeC4Kills();
                this.makeKnifeKills();
                this.makePistolKills();
                this.makeLauncherKills();

                this.makeAverageLifetime();
                this.makeRoadkills();
                this.makeInfantryDamage();
                this.makeInfantryDamagePerMinute();
                this.makeTeamKills();

                // get the world ID
                let worldID: number = 0;
                if (this.report.characters.size > 0) {
                    this.report.characters.forEach((char: PsCharacter, charID: string) => {
                        worldID = char.worldID;
                        return;
                    });
                }

                // don't encourage killing spawns!
                console.log(`ReportWinter> worldID: ${worldID}`);
                if (worldID == 19) {
                    this.makeBeaconKills();
                }

                //this.makeRecon(); // BUGGED
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
                metric.description = "Most assists (per minute)";

                this.catKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.ASSIST, Experience.HIGH_PRIORITY_ASSIST, Experience.PRIORITY_ASSIST, Experience.SPAWN_ASSIST],
                    (metadata) => metadata.timeAs)
                );
            },

            makeKillParticipation: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Kill participation";
                metric.funName = "Kill participation";
                metric.description = "% of kills + assists of total kills";

                const killCount: number = this.report.kills.length;

                const killsAndAssists: Map<string, number> = new Map();
                for (const ev of this.report.kills) {
                    killsAndAssists.set(ev.attackerCharacterID, (killsAndAssists.get(ev.attackerCharacterID) || 0) + 1);
                }

                for (const ev of this.report.experience) {
                    if (Experience.isAssist(ev.experienceID) == false) {
                        continue;
                    }

                    killsAndAssists.set(ev.sourceID, (killsAndAssists.get(ev.sourceID) || 0) + 1);
                }

                const metrics: WinterEntry[] = Array.from(killsAndAssists.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.value = iter[1];
                        entry.name = this.getCharacterName(iter[0]);

                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata != undefined) {
                            entry.display = `${LocaleUtil.locale(entry.value / killCount * 100, 2)}% (${entry.value})`;
                        }
                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = metrics;

                this.catKills.metrics.push(metric);
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

            makeScore: function(): void {
                const metric: WinterMetric = new WinterMetric();
                metric.name = "Score";
                metric.funName = "Score";
                metric.description = "Highest score (per minute)";
                metric.helpText = "Not tracked before 2022-08-02";

                const map: Map<string, WinterEntry> = new Map();

                for (const ev of this.report.experience) {
                    if (map.has(ev.sourceID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = ev.sourceID;

                        entry.name = this.getCharacterName(ev.sourceID);
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(ev.sourceID)!;
                    entry.value += ev.amount;
                    ++entry.value;
                    map.set(ev.sourceID, entry);
                }

                const entries: WinterEntry[] = Array.from(map.values());
                for (const entry of entries) {
                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                    if (metadata != undefined) {
                        entry.display = `${LocaleUtil.locale(entry.value, 0)} (${(entry.value / Math.max(1, metadata.timeAs) * 60).toFixed(2)})`;
                    }
                }

                metric.entries = entries.sort((a, b) => b.value - a.value);
                this.compactFormatLocale(metric, (metadata) => metadata.timeAs);

                this.catKills.metrics.push(metric);
            },

            makeSPM: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "SPM";
                metric.funName = "Speed runner";
                metric.description = "Highest SPM";
                metric.helpText = "Not tracked before 2022-08-02";

                const map: Map<string, number> = new Map();

                for (const exp of this.report.experience) {
                    const id: string = exp.sourceID;
                    map.set(id, (map.get(id) || 0) + exp.amount);
                }

                map.forEach((score: number, charID: string) => {
                    if (score < 2_000) {
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
                    entry.value = score / Math.max(1, metadata.timeAs) * 60;
                    entry.display = `${LocaleUtil.format(entry.value, this.compatFormat)} (${LocaleUtil.format(score, this.compatFormat)})`;

                    metric.entries.push(entry);
                });

                metric.entries.sort((a, b) => b.value - a.value);

                this.catKills.metrics.push(metric);
            },

            makeInfantryDamage: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Infantry damage dealt";
                metric.funName = "Infantry damage dealt";
                metric.description = "Damage dealt to infantry (estimated!)";
                metric.helpText = "Estimated from assist XP, not available before 2022-08-02";

                const map: Map<string, number> = new Map();
                this.report.playerInfantryDamage.forEach((value: InfantryDamageEntry, key: string) => {
                    map.set(key, value.totalDamage);
                });

                metric = this.generateFromMap(metric, map, (metadata) => metadata.timeAs, 0, 2);
                this.compactFormatLocale(metric, (metadata) => metadata.timeAs);

                this.catMisc.metrics.push(metric);
            },

            makeInfantryDamagePerMinute: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Infantry damage per min";
                metric.funName = "Infantry damage per min";
                metric.description = "Damage dealt to infantry (estimated!)";
                metric.helpText = "Estimated from assist XP, not available before 2022-08-02";

                metric.entries = Array.from(this.report.playerInfantryDamage.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];

                        const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                        if (metadata == undefined) {
                            console.warn(`Missing player metadata for ${iter[0]}/${this.getCharacterName(iter[0])})`);
                        }

                        entry.value = iter[1].totalDamage / (metadata?.timeAs || 120) * 60;
                        entry.name = this.getCharacterName(iter[0]);
                        entry.display = `${LocaleUtil.format(entry.value, this.compatFormat)} (${LocaleUtil.format(iter[1].totalDamage, this.compatFormat)})`;

                        return entry;
                    }).sort((a, b) => b.value - a.value);

                this.catMisc.metrics.push(metric);
            },

            makeKillstreak: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Longest killstreak";
                metric.funName = "Streaker";
                metric.description = "Longest killstreak (30s timeout)";
                metric.helpText = "Killstreaks are reset on death, or if there is more than 30 seconds between kills";

                const map: Map<string, number[]> = new Map();
                const streaks: Map<string, number> = new Map();
                const lastKill: Map<string, KillEvent> = new Map();

                const events: KillEvent[] = [...this.report.kills];
                events.push(...this.report.deaths);
                events.sort((a, b) => {
                    return a.timestamp.getTime() - b.timestamp.getTime();
                });

                for (const ev of events) {
                    // If this is a kill
                    if (this.report.trackedCharacters.indexOf(ev.attackerCharacterID) > -1) {
                        if (map.has(ev.attackerCharacterID) == false) {
                            map.set(ev.attackerCharacterID, []);
                        }

                        // Timeout for kills, if kills are more than 30 seconds apart, they are not a streak
                        const lastKillEv: KillEvent | null = lastKill.get(ev.attackerCharacterID) || null;
                        if (lastKillEv != null) {
                            const lastKillTs: number = lastKillEv.timestamp.getTime();
                            const curKillTs: number = ev.timestamp.getTime();
                            if (curKillTs - lastKillTs >= 1000 * 30) { // 1000 ms * 30 seconds
                                if (streaks.has(ev.attackerCharacterID) == true) {
                                    const s: number[] = map.get(ev.attackerCharacterID) || [];
                                    s.push(streaks.get(ev.attackerCharacterID)!);
                                    map.set(ev.attackerCharacterID, s);
                                    streaks.set(ev.attackerCharacterID, 0);
                                }
                            }
                        }

                        streaks.set(ev.attackerCharacterID, (streaks.get(ev.attackerCharacterID) || 0) + 1);
                        lastKill.set(ev.attackerCharacterID, ev);
                    } else if (this.report.trackedCharacters.indexOf(ev.killedCharacterID) > -1) { // this is death
                        if (map.has(ev.killedCharacterID) == false) {
                            map.set(ev.killedCharacterID, []);
                        }

                        if (streaks.has(ev.killedCharacterID) == true) {
                            const s: number[] = map.get(ev.killedCharacterID) || [];
                            s.push(streaks.get(ev.killedCharacterID)!);
                            map.set(ev.killedCharacterID, s);
                            streaks.set(ev.killedCharacterID, 0);
                        }
                    }
                }

                const metrics: WinterEntry[] = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = iter[0];
                        entry.value = Math.max(...iter[1]);
                        entry.name = this.getCharacterName(iter[0]);

                        return entry;
                    }).sort((a, b) => b.value - a.value);

                metric.entries = metrics;

                this.catKills.metrics.push(metric);
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

                metric = this.generateFromMap(metric, map, (metadata) => metadata.timeAs / 60, 0, 2);

                this.catMisc.metrics.push(metric);
            },

            makeHeals: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Heals";
                metric.funName = "Green Wizard";
                metric.description = "Most heals (per minute)";
                this.catMedic.metrics.push(this.generateExperience(metric, [Experience.HEAL, Experience.SQUAD_HEAL], (metadata) => metadata.classes.medic.timeAs));
            },

            makeHealthHealed: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Health healed";
                metric.funName = "Health healed";
                metric.description = "Health healed (estimated, per minute)";
                metric.helpText = "Estimated from 1 xp per heal tick is equal to 10 health healed";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.experience) {
                    if (Experience.isHeal(ev.experienceID) == false) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(ev.sourceID);
                    if (ev.experienceID == Experience.SQUAD_HEAL) {
                        ev.amount /= 1.5;
                    }

                    // One 1xp = 10 health healed (GUESS ALERT THIS IS A GUESS SIREN WEEWHOO)
                    // also ignore double XP if the player has it
                    const amount: number = ev.amount * 10 / (metadata?.scoreMultiplier ?? 1);

                    map.set(ev.sourceID, (map.get(ev.sourceID) || 0) + amount);
                }

                this.generateFromMap(metric, map, (metadata) => metadata.classes.medic.timeAs);
                this.compactFormatLocale(metric, (metadata) => metadata.classes.medic.timeAs);

                this.catMedic.metrics.push(metric);

                //this.catMedic.metrics.push(this.generateFromMap(metric, map, (metadata) => metadata.classes.medic.timeAs));
            },

            makeShieldsHealed: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Shields restored";
                metric.funName = "Shields restored";
                metric.description = "Shields restored (estimated, per minute)";
                metric.helpText = "Estimated from 1 xp per tick is equal to 10 shield restored";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.experience) {
                    if (Experience.isShieldRepair(ev.experienceID) == false) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(ev.sourceID);
                    if (ev.experienceID == Experience.SQUAD_SHIELD_REPAIR) {
                        ev.amount /= 1.5;
                    }

                    // One 1xp = 10 health healed (GUESS ALERT THIS IS A GUESS SIREN WEEWHOO)
                    // also ignore double XP if the player has it
                    const amount: number = ev.amount * 10 / (metadata?.scoreMultiplier ?? 1);

                    map.set(ev.sourceID, (map.get(ev.sourceID) || 0) + amount);
                }

                this.generateFromMap(metric, map, (metadata) => metadata.classes.medic.timeAs);
                this.compactFormatLocale(metric, (metadata) => metadata.classes.medic.timeAs);

                this.catMedic.metrics.push(metric);
            },

            makeRevives: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Revives";
                metric.funName = "Necromancer";
                metric.description = "Most revives (per minute)";

                this.catMedic.metrics.push(this.generateExperience(metric, [Experience.REVIVE, Experience.SQUAD_REVIVE], (metadata) => metadata.classes.medic.timeAs));
            },

            makeResupplies: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Resupplies";
                metric.funName = "Ammo printer";
                metric.description = "Most resupplies (per minute)";

                this.catEngi.metrics.push(this.generateExperience(metric, [Experience.RESUPPLY, Experience.SQUAD_RESUPPLY], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeMaxRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX repairs";
                metric.funName = "Welder";
                metric.description = "Most MAX repairs (per minute)";

                this.catEngi.metrics.push(this.generateExperience(metric, [Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeMaxRepairHealth: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX health repairs";
                metric.funName = "MAX health repairs";
                metric.description = "MAX health repairs (estimated, per minute)";
                metric.helpText = "Estimated from 1 xp per repair tick is equal to 25 health repaired";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.experience) {
                    if (Experience.isMaxRepair(ev.experienceID) == false) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(ev.sourceID);
                    if (ev.experienceID == Experience.SQUAD_MAX_REPAIR) {
                        ev.amount /= 1.5;
                    }

                    // One 1xp = 25 health healed (GUESS ALERT THIS IS A GUESS SIREN WEEWHOO)
                    // also ignore double XP if the player has it
                    const amount: number = ev.amount * 25 / (metadata?.scoreMultiplier ?? 1);

                    map.set(ev.sourceID, (map.get(ev.sourceID) || 0) + amount);
                }

                this.generateFromMap(metric, map, (metadata) => metadata.classes.engineer.timeAs, 0, 2);
                this.compactFormatLocale(metric, (metadata) => metadata.classes.engineer.timeAs);

                this.catEngi.metrics.push(metric);
            },

            makeVehicleRepairHealth: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Vehicle health repairs";
                metric.funName = "Vehicle health repairs";
                metric.description = "Vehicle health repairs (estimated, per minute)";
                metric.helpText = "Lower bound, estimated from 1 xp per repair tick is equal to 25 health repaired";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.experience) {
                    if (Experience.isVehicleRepair(ev.experienceID) == false) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(ev.sourceID);
                    let amt: number = ev.amount;
                    if (Experience.isSquadVehicleRepair(ev.experienceID)) {
                        amt /= 1.5;
                    }

                    // One 1xp = 25 health healed (GUESS ALERT THIS IS A GUESS SIREN WEEWHOO)
                    // also ignore double XP if the player has it
                    const amount: number = amt * 25 / (metadata?.scoreMultiplier ?? 1);

                    map.set(ev.sourceID, (map.get(ev.sourceID) || 0) + amount);
                }

                this.generateFromMap(metric, map, (metadata) => metadata.classes.engineer.timeAs, 0, 2);
                this.compactFormatLocale(metric, (metadata) => metadata.classes.engineer.timeAs);

                this.catEngi.metrics.push(metric);
            },

            makeTotalRepairHealth: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Total repairs";
                metric.funName = "Total repairs";
                metric.description = "Total repairs (estimated, per minute)";
                metric.helpText = "Lower bound, estimated from 1 xp per repair tick is equal to 25 health repaired";

                const map: Map<string, number> = new Map();

                for (const ev of this.report.experience) {
                    if (Experience.isVehicleRepair(ev.experienceID) == false
                        && Experience.isMaxRepair(ev.experienceID) == false
                        && ev.experienceID != 87 // secondary objective repair
                        && ev.experienceID != 276 // terminal repair
                        && ev.experienceID != 1375 // hardlight repair
                        && ev.experienceID != 1378 // squad hardlight repair
                        && ev.experienceID != 1545 // do0k lock repair
                    ) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(ev.sourceID);
                    if (Experience.isSquadVehicleRepair(ev.experienceID)) {
                        ev.amount /= 1.5;
                    }

                    // One 1xp = 25 health healed (GUESS ALERT THIS IS A GUESS SIREN WEEWHOO)
                    // also ignore double XP if the player has it
                    const amount: number = ev.amount * 25 / (metadata?.scoreMultiplier ?? 1);

                    map.set(ev.sourceID, (map.get(ev.sourceID) || 0) + amount);
                }

                this.generateFromMap(metric, map, (metadata) => metadata.classes.engineer.timeAs, 0, 2);
                this.compactFormatLocale(metric, (metadata) => metadata.classes.engineer.timeAs);

                this.catEngi.metrics.push(metric);
            },

            makeHardlightAssists: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Hardlight Assists";
                metric.funName = "Brick layer";
                metric.description = "Most draw fire assists (per minute)";
                metric.helpText = "Data not tracked before 2022-05-02";

                this.catEngi.metrics.push(this.generateExperience(metric, [1393], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeManaKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MANA AI kills";
                metric.funName = "Bastion RP";
                metric.description = "Most kills on a MANA AI turret";

                this.catEngi.metrics.push(this.generateWeaponKills(
                    metric,
                    [555, 6005423, 556, 554], // nc, ns, tr, vs
                    (metadata) => metadata.timeAs)
                );
            },

            makeShieldRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Shield repairs";
                metric.funName = "Shield battery";
                metric.description = "Most shield repairs (per minute)";

                this.catMedic.metrics.push(this.generateExperience(metric, [Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR], (metadata) => metadata.classes.medic.timeAs));
            },

            makeSpawns: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Total spawns";
                metric.funName = "Mother";
                metric.description = "Most spawns (per minute)";

                this.catSpawns.metrics.push(this.generateExperience(
                    metric,
                    [
                        Experience.GALAXY_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN, Experience.SQUAD_SPAWN,
                        Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS, Experience.ANT_SPAWN
                    ],
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
                metric.name = "Bus spawns";
                metric.funName = "Cat herder";
                metric.description = "Most spawns from a bus (per minute)";

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
                metric.name = "Buses placed";
                metric.funName = "Bus stop builder";
                metric.description = "Most buses deployed (per hour)";

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
                metric.helpText = "Minimum 25 kills";

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
                metric.helpText = "Minimum 25 kills";

                for (const player of this.report.trackedCharacters) {
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
                metric.helpText = "Revives remove a death (like in game). Minimum 25 kills";

                for (const player of this.report.trackedCharacters) {
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
                metric.description = "Most vehicle kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [
                        Experience.VKILL_FLASH, Experience.VKILL_HARASSER, Experience.VKILL_LIGHTNING, Experience.VKILL_ANT,
                        Experience.VKILL_MOSQUITO, Experience.VKILL_REAVER, Experience.VKILL_SCYTHE, Experience.VKILL_DERVISH,
                        Experience.VKILL_VALKYRIE, Experience.VKILL_LIBERATOR, Experience.VKILL_GALAXY,
                        Experience.VKILL_PROWLER, Experience.VKILL_VANGUARD, Experience.VKILL_PROWLER, Experience.VKILL_CHIMERA,
                        Experience.VKILL_COLOSSUS, Experience.VKILL_JAVELIN
                    ],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeFlashKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Flash kills";
                metric.funName = "Flash kills";
                metric.description = "Most flash kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_FLASH, Experience.VKILL_JAVELIN],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeLightningKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Lightning kills";
                metric.funName = "Lightning kills";
                metric.description = "Most lightning kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_LIGHTNING],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeMBTKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MBT kills";
                metric.funName = "MBT kills";
                metric.description = "Most MBT kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_MAGRIDER, Experience.VKILL_VANGUARD, Experience.VKILL_PROWLER, Experience.VKILL_CHIMERA],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeHarasserKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Harasser kills";
                metric.funName = "Harasser kills";
                metric.description = "Most harasser kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_HARASSER],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeESFKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "ESF kills";
                metric.funName = "ESF kills";
                metric.description = "Most ESF kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_MOSQUITO, Experience.VKILL_SCYTHE, Experience.VKILL_REAVER, Experience.VKILL_DERVISH],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeValkKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Valkyrie kills";
                metric.funName = "Valkyire kills";
                metric.description = "Most valkyire kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_VALKYRIE],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeLiberatorKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Liberator kills";
                metric.funName = "Liberator kills";
                metric.description = "Most liberator kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_LIBERATOR],
                    (metadata) => metadata.timeAs / 60)
                );
            },

            makeGalaxyKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Galaxy kills";
                metric.funName = "Galaxy  kills";
                metric.description = "Most galaxy kills (per hour)";

                this.catVehicleKills.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VKILL_GALAXY],
                    (metadata) => metadata.timeAs / 60)
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

                for (const player of this.report.trackedCharacters) {
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
                metric.description = "Most vehicle repairs (per minute)";

                this.catEngi.metrics.push(this.generateExperience(
                    metric,
                    [...Experience.VehicleRepairs, ...Experience.SquadVehicleRepairs],
                    (metadata) => metadata.timeAs)
                );
            },

            makeVehicleResupply: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Vehicle resupplies";
                metric.funName = "Vehicle resupplies";
                metric.description = "Most vehicle resupplies (per minute)";

                this.catEngi.metrics.push(this.generateExperience(
                    metric,
                    [Experience.VEHICLE_RESUPPLY, Experience.SQUAD_VEHICLE_RESUPPLY],
                    (metadata) => metadata.timeAs)
                );
            },

            makeRecon: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Recon";
                metric.funName = "Spotter";
                metric.description = "Most recon (per minute)";

                this.catMisc.metrics.push(this.generateExperience(
                    metric,
                    [Experience.MOTION_DETECT, Experience.SQUAD_MOTION_DETECT],
                    (metadata) => metadata.timeAs)
                );
            },

            makeBeaconKills: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Beacon kills";
                metric.funName = "Beacon kills";
                metric.description = "Most beacons killed (per hour)";

                this.catMisc.metrics.push(this.generateExperience(
                    metric,
                    [270], // 270 => Squad Spawn beacon kill
                    (metadata) => metadata.timeAs)
                );
            },

            makeTeamKills: function(): void {
                // this doesn't actually work cause honu doesn't send the teamkill events
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Teamkills";
                metric.funName = "Teamkills";
                metric.description = "Most teamkills";

                const map: Map<string, number> = new Map();

                for (const kill of this.report.kills) {
                    if (kill.attackerTeamID == kill.killedTeamID) {
                        map.set(kill.attackerCharacterID, (map.get(kill.attackerCharacterID) ?? 0) + 1);
                    }
                }

                this.catMisc.metrics.push(this.generateFromMap(metric, map));
            },

            /**
             * Generate a winter metric from a map that contains a character ID an whatever value they got. An option to have a per time selector is given
             * @param metric
             * @param map
             * @param perMinuteSelector
             */
            generateFromMap: function(metric: WinterMetric, map: Map<string, number>,
                perMinuteSelector: ((metadata: PlayerMetadata) => number) | null = null,
                fixedLength?: number, fixedLengthPerMin?: number): WinterMetric {

                metric.entries = Array.from(map.entries())
                    .map(iter => {
                        const entry: WinterEntry = new WinterEntry();

                        entry.characterID = iter[0];
                        entry.value = iter[1];
                        entry.name = this.getCharacterName(entry.characterID);

                        if (perMinuteSelector != null) {
                            const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(entry.characterID);
                            if (metadata != undefined) {
                                const minutes: number = perMinuteSelector(metadata);
                                entry.display = `${LocaleUtil.locale(entry.value, fixedLength)} (${LocaleUtil.locale(entry.value / Math.max(1, minutes) * 60, fixedLengthPerMin)})`;
                            }
                        }

                        return entry;
                    }).sort((a, b) => b.value - a.value);

                return metric;
            },

            compactFormatLocale: function(metric: WinterMetric, perMinuteSelector: ((metadata: PlayerMetadata) => number)): void {
                metric.entries.forEach((iter: WinterEntry) => {
                    const metadata: PlayerMetadata | undefined = this.report.playerMetadata.get(iter.characterID);
                    if (metadata != undefined) {
                        const minutes: number = perMinuteSelector(metadata);
                        iter.display = `${LocaleUtil.format(iter.value, this.compatFormat)} (${LocaleUtil.format(iter.value / Math.max(1, minutes) * 60, this.compatFormat)})`;
                    }
                });
            },

            /**
             * Generate a winter metric from the amount of XP ticks of a type were earned
             * @param metric
             * @param expIDs
             * @param perMinuteSelector
             */
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

            /**
             * Generate a winter metric for the top killers with specific weapon IDs
             * @param metric
             * @param weaponIDs
             * @param perMinuteSelector
             */
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

            /**
             * Generate a winter metric for the top killers with a specific weapon type
             * @param metric
             * @param itemCategoryID
             * @param perMinuteSelector
             */
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
            catMedic: function(): WinterCategory {
                return this.categories[1];
            },
            catEngi: function(): WinterCategory {
                return this.categories[2];
            },
            catSpawns: function(): WinterCategory {
                return this.categories[3];
            },
            catWeaponTypes: function(): WinterCategory {
                return this.categories[4];
            },
            catVehicleKills: function(): WinterCategory {
                return this.categories[5];
            },
            catVehicleSupport: function(): WinterCategory {
                return this.categories[6];
            },
            catMisc: function(): WinterCategory {
                return this.categories[7];
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
