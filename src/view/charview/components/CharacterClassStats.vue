<template>
    <table class="table table-sm w-auto d-inline-block">
        <tr class="table-secondary th-border-top-0">
            <td><b>{{title}}</b></td>
            <td><b>Time as</b></td>
            <td><b>Score</b></td>
            <td><b>SPM</b></td>
            <td v-if="IncludeMetadata == true">
                <b>Last updated</b>
                <info-hover text="When this data was last updated in the PS2 API">
                </info-hover>
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.Infil_128" style="max-height: 2rem;"></census-image>
                <b>Infil</b>
            </td>
            <td>
                {{timeAs.infil | mduration}}
                ({{timeAs.infil / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.infil | locale}}
            </td>
            <td>
                {{score.infil / (timeAs.infil || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{infilUpdated | moment}}
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.LightAssault_64" style="max-height: 2rem;"></census-image>
                <b>Light Assault</b>
            </td>
            <td>
                {{timeAs.lightAssault | mduration}}
                ({{timeAs.lightAssault / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.lightAssault | locale}}
            </td>
            <td>
                {{score.lightAssault / (timeAs.lightAssault || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{lightAssaultUpdated | moment}}
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.Medic_64" style="max-height: 2rem;"></census-image>
                <b>Medic</b>
            </td>
            <td>
                {{timeAs.medic | mduration}}
                ({{timeAs.medic / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.medic | locale}}
            </td>
            <td>
                {{score.medic / (timeAs.medic || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{medicUpdated | moment}}
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.Engi_64" style="max-height: 2rem;"></census-image>
                <b>Engineer</b>
            </td>
            <td>
                {{timeAs.engineer | mduration}}
                ({{timeAs.engineer / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.engineer | locale}}
            </td>
            <td>
                {{score.engineer / (timeAs.engineer || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{engineerUpdated | moment}}
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.Heavy_64" style="max-height: 2rem;"></census-image>
                <b>Heavy Assault</b>
            </td>
            <td>
                {{timeAs.heavy | mduration}}
                ({{timeAs.heavy / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.heavy | locale}}
            </td>
            <td>
                {{score.heavy / (timeAs.heavy || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{heavyUpdated | moment}}
            </td>
        </tr>

        <tr>
            <td>
                <census-image :image-id="imageIDs.Max_64" style="max-height: 2rem;"></census-image>
                <b>MAX</b>
            </td>
            <td>
                {{timeAs.max | mduration}}
                ({{timeAs.max / totalTime * 100 | locale}}%)
            </td>
            <td>
                {{score.max | locale}}
            </td>
            <td>
                {{score.max / (timeAs.max || 1) * 60 | fixed | locale}}
            </td>
            <td v-if="IncludeMetadata == true">
                {{maxUpdated | moment}}
            </td>
        </tr>
    </table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import InfoHover from "components/InfoHover.vue";
    import CensusImage from "components/CensusImage";

    import { CharacterStat } from "api/CharacterStatApi";

    import ImageUtil from "util/Image";

    class ClassStatSet {
        public infil: number = 0;
        public lightAssault: number = 0;
        public medic: number = 0;
        public engineer: number = 0;
        public heavy: number = 0;
        public max: number = 0;
    }

    export const CharacterClassStats = Vue.extend({
        props: {
            data: { type: Array as PropType<CharacterStat[]>, required: true },
            type: { type: String, required: true },
            title: { type: String, required: true },
            IncludeMetadata: { type: Boolean, required: true },
        },

        mounted: function(): void {
            this.setData();
        },

        data: function() {
            return {
                kills: new ClassStatSet() as ClassStatSet,
                score: new ClassStatSet() as ClassStatSet,
                timeAs: new ClassStatSet() as ClassStatSet,

                imageIDs: { ...ImageUtil.Images },

                infilUpdated: new Date() as Date,
                lightAssaultUpdated: new Date() as Date,
                medicUpdated: new Date() as Date,
                engineerUpdated: new Date() as Date,
                heavyUpdated: new Date() as Date,
                maxUpdated: new Date() as Date,
            }
        },

        methods: {
            setData: function(): void {
                if (this.type != "daily" && this.type != "weekly" && this.type != "monthly" && this.type != "forever") {
                    throw `Invalid type '${this.type}' passed, expected daily | weekly | monthly | forever`;
                }

                function setStat(set: ClassStatSet, stat: CharacterStat, type: string): void {
                    const now: Date = new Date();

                    let value: number;
                    if (type == "daily") {
                        value = stat.valueDaily;
                    } else if (type == "weekly") {
                        value = stat.valueWeekly;
                    } else if (type == "monthly") {
                        //console.log(`${stat.timestamp.getFullYear()} ${stat.timestamp.getMonth()} ${now.getFullYear()} ${now.getMonth()}`);
                        // Value is too old to be useful here
                        if (stat.timestamp.getFullYear() != now.getFullYear() || stat.timestamp.getMonth() != now.getMonth()) {
                            return;
                        }
                        value = stat.valueMonthly;
                    } else if (type == "forever") {
                        value = stat.valueForever;
                    } else {
                        throw `Unchecked type passed: '${type}'`;
                    }

                    if (stat.profileID == 0) { // all

                    } else if (stat.profileID == 1) { // infil
                        set.infil = value;
                    } else if (stat.profileID == 3) { // light assault
                        set.lightAssault = value;
                    } else if (stat.profileID == 4) { // medic
                        set.medic = value;
                    } else if (stat.profileID == 5) { // engineer
                        set.engineer = value;
                    } else if (stat.profileID == 6) { // heavy
                        set.heavy = value;
                    } else if (stat.profileID == 7) { // max
                        set.max = value;
                    }
                }

                for (const stat of this.data) {
                    if (stat.statName == "kills") {
                        setStat(this.kills, stat, this.type);
                    } else if (stat.statName == "play_time") {
                        setStat(this.timeAs, stat, this.type);
                    } else if (stat.statName == "score") {
                        setStat(this.score, stat, this.type);
                    }

                    if (stat.profileID == 1) {
                        this.infilUpdated = stat.timestamp;
                    } else if (stat.profileID == 3) {
                        this.lightAssaultUpdated = stat.timestamp;
                    } else if (stat.profileID == 4) {
                        this.medicUpdated = stat.timestamp;
                    } else if (stat.profileID == 5) {
                        this.engineerUpdated = stat.timestamp;
                    } else if (stat.profileID == 6) {
                        this.heavyUpdated = stat.timestamp;
                    } else if (stat.profileID == 7) {
                        this.maxUpdated = stat.timestamp;
                    }
                }
            }
        },

        computed: {
            totalTime: function(): number {
                return this.timeAs.infil + this.timeAs.lightAssault + this.timeAs.medic
                    + this.timeAs.engineer + this.timeAs.heavy + this.timeAs.max + 1;
            }

        },

        components: {
            InfoHover,
            CensusImage
        }
    });
    export default CharacterClassStats;

</script>