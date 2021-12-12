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
                this.makeHeals();
                this.makeRevives();
                this.makeResupplies();
                this.makeMaxRepairs();
                this.makeShieldRepairs();
                this.makeKpm();
            },

            makeKills: function(): void {
                const metric: WinterMetric = new WinterMetric();
                metric.name = "Kills";
                metric.funName = "Kills";
                metric.description = "Players with the most kills";

                const map: Map<string, WinterEntry> = new Map();

                for (const kill of this.report.kills) {
                    if (map.has(kill.attackerCharacterID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = kill.attackerCharacterID;

                        const character: PsCharacter | null = this.report.characters.get(entry.characterID) || null;
                        entry.name = character != null ? `${character.outfitID != null ? `[${character.outfitTag}] ` : ""}${character.name}` : `<missing ${entry.characterID}>`;
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(kill.attackerCharacterID)!;
                    ++entry.value;
                    map.set(kill.attackerCharacterID, entry);
                }

                metric.entries = Array.from(map.values())
                    .sort((a, b) => b.value - a.value);

                if (metric.entries.length > 0) {
                    this.essential.push(metric);
                }
            },

            makeHeals: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Heals";
                metric.funName = "Necromancer";
                metric.description = "Most heals";

                this.essential.push(this.generateExperience(metric, [Experience.HEAL, Experience.SQUAD_HEAL]));
            },

            makeRevives: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Revives";
                metric.funName = "Necromancer";
                metric.description = "Most revives";

                this.essential.push(this.generateExperience(metric, [Experience.REVIVE, Experience.SQUAD_REVIVE]));
            },

            makeResupplies: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Resupplies";
                metric.funName = "Ammo printer";
                metric.description = "Most resupplies";

                this.essential.push(this.generateExperience(metric, [Experience.RESUPPLY, Experience.SQUAD_RESUPPLY]));
            },

            makeMaxRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "MAX repairs";
                metric.funName = "Welder";
                metric.description = "Most MAX repairs";

                this.essential.push(this.generateExperience(metric, [Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR]));
            },

            makeShieldRepairs: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "Shield repairs";
                metric.funName = "Shield battery";
                metric.description = "Most shield repairs";

                this.essential.push(this.generateExperience(metric, [Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR]));
            },

            makeKpm: function(): void {
                let metric: WinterMetric = new WinterMetric();
                metric.name = "KPM";
                metric.funName = "Speed gunner";
                metric.description = "Highest KPM";

                const map: Map<string, number> = new Map();

                for (const kill of this.report.kills) {
                    const id: string = kill.attackerCharacterID;
                    if (map.has(kill.attackerCharacterID) == false) {
                        const character: PsCharacter | null = this.report.characters.get(id) || null;
                        map.set(id, 0);
                    }

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

                    const character: PsCharacter | null = this.report.characters.get(entry.characterID) || null;
                    entry.name = character != null ? `${character.outfitID != null ? `[${character.outfitTag}] ` : ""}${character.name}` : `<missing ${entry.characterID}>`;
                    entry.value = kills / metadata.timeAs * 60;
                    entry.display = entry.value.toFixed(2);

                    metric.entries.push(entry);
                });

                metric.entries.sort((a, b) => b.value - a.value);

                this.essential.push(metric);
            },

            generateExperience: function(metric: WinterMetric, expIDs: number[]): WinterMetric {
                const map: Map<string, WinterEntry> = new Map();

                for (const exp of this.report.experience) {
                    if (expIDs.indexOf(exp.experienceID) == -1) {
                        continue;
                    }

                    if (map.has(exp.sourceID) == false) {
                        const entry: WinterEntry = new WinterEntry();
                        entry.characterID = exp.sourceID;

                        const character: PsCharacter | null = this.report.characters.get(entry.characterID) || null;
                        entry.name = character != null ? `${character.outfitID != null ? `[${character.outfitTag}] ` : ""}${character.name}` : `<missing ${entry.characterID}>`;
                        entry.value = 0;

                        map.set(entry.characterID, entry);
                    }

                    const entry: WinterEntry = map.get(exp.sourceID)!;
                    ++entry.value;
                    map.set(exp.sourceID, entry);
                }

                metric.entries = Array.from(map.values())
                    .sort((a, b) => b.value - a.value);

                return metric;
            }

        },

        components: {
            WinterCard
        }
    });

    export default ReportWinter;
</script>