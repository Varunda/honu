<template>
    <div>
        <h2 class="wt-header">Winter Leaderboard</h2>

        <h4>Summary</h4>
        <div class="d-flex">
            <winter-card v-for="metric in essential" :key="metric.name"
                :card="metric" :show-fun-name="settings.showFunNames" :size="settings.size">
            </winter-card>
        </div>

        <h4>Fun leaderboards</h4>
        <div class="d-flex" style="flex-wrap: wrap;">
            <winter-card v-for="metric in fun" :key="metric.name"
                :card="metric" :show-fun-name="settings.showFunNames" :size="settings.size">
            </winter-card>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata } from "../Report";

    import WinterCard from "./winter/WinterCard.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";

    class WinterMetric {
        public name: string = "";
        public funName: string = "";
        public description: string = "";
        public entries: WinterEntry[] = [];
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

                if (metric.entries.length > 0) {
                    this.essential.push(metric);
                }
            },

            makeAssists: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Assists";
                metric.funName = "Assists";
                metric.description = "Highest assists (per minute)";

                this.essential.push(this.generateExperience(
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

                this.essential.push(this.generateExperience(metric, [Experience.HEAL, Experience.SQUAD_HEAL], (metadata) => metadata.classes.medic.timeAs));
            },

            makeRevives: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Revives";
                metric.funName = "Necromancer";
                metric.description = "Most revives (per minute)";

                this.essential.push(this.generateExperience(metric, [Experience.REVIVE, Experience.SQUAD_REVIVE], (metadata) => metadata.classes.medic.timeAs));
            },

            makeResupplies: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Resupplies";
                metric.funName = "Ammo printer";
                metric.description = "Most resupplies (per minute)";

                this.essential.push(this.generateExperience(metric, [Experience.RESUPPLY, Experience.SQUAD_RESUPPLY], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeMaxRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX repairs";
                metric.funName = "Welder";
                metric.description = "Most MAX repairs (per minute)";

                this.essential.push(this.generateExperience(metric, [Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR], (metadata) => metadata.classes.engineer.timeAs));
            },

            makeShieldRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Shield repairs";
                metric.funName = "Shield battery";
                metric.description = "Most shield repairs (per minute)";

                this.essential.push(this.generateExperience(metric, [Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR], (metadata) => metadata.classes.medic.timeAs));
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

                this.essential.push(metric);
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

        components: {
            WinterCard
        }
    });

    export default ReportWinter;
</script>