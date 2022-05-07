<template>
    <a-table
        :entries="outfits" name="outfit-kill-board"
        :show-filters="true" :default-page-size="25" :show-footer="true"
        default-sort-field="killScore" default-sort-order="desc"
        display-type="table" row-padding="compact">

        <a-col sort-field="outfitDisplay">
            <a-header>
                <b>Outfit</b>
                <info-hover text="An outfit must have had at least 5 members online during the course of the alert"></info-hover>
            </a-header>

            <a-filter field="outfitDisplay" type="string" method="input"
                :conditions="[ 'contains' ]">
            </a-filter>

            <a-body v-slot="entry">
                <a :href="'/o/' + entry.outfitID" :style="{ color: getFactionColor(entry.factionID) }">
                    {{entry.outfitDisplay}}
                </a>

                <span v-if="alert.duration <= 28800">
                    -

                    <a href="#" @click="openReport(entry.outfitID)">
                        Report
                        <span class="fas fa-external-link-alt"></span>
                    </a>
                </span>
            </a-body>
        </a-col>

        <a-col sort-field="factionID">
            <a-header>
                <b>Faction</b>
            </a-header>

            <a-filter field="factionID" type="number" method="dropdown" :source="sources.factions" source-key="key" source-value="value"
                :conditions="['equals']">
            </a-filter>

            <a-body v-slot="entry">
                {{entry.factionID | faction}}
            </a-body>
        </a-col>

        <a-col sort-field="killScore">
            <a-header>
                <b>K%/P</b>
                <info-hover text="(Kills of outfit) / (Kills of faction) / (Outfit members who participated) * 100"></info-hover>
            </a-header>

            <a-body v-slot="entry">
                {{entry.killScore | locale(2)}}%
            </a-body>
        </a-col>

        <a-col sort-field="kills">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.kills}}
            </a-body>
        </a-col>

        <a-col sort-field="kpm">
            <a-header>
                <b>KPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.kpm | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="deaths">
            <a-header>
                <b>Deaths</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.deaths}}
            </a-body>
        </a-col>

        <a-col sort-field="kd">
            <a-header>
                <b>K/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.kd | locale}}
            </a-body>
        </a-col>

        <a-col sort-field="members">
            <a-header>
                <b>Members</b>
                <info-hover text="How many members over the course of the alert participated"></info-hover>
            </a-header>

            <a-body v-slot="entry">
                {{entry.members}}
            </a-body>
        </a-col>
    </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import InfoHover from "components/InfoHover.vue";
    import ATable, { ACol, ABody, AFilter, AHeader, AFooter } from "components/ATable";

    import { PsAlert } from "api/AlertApi";

    export const AlertOutfitKillBoard = Vue.extend({
        props: {
            alert: { type: Object as PropType<PsAlert>, required: true },
            outfits: { type: Object, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            },

            openReport: function(outfitID: string): void {
                const outfit: any = this.outfits.data.find((iter: any) => iter.outfitID == outfitID);
                if (!outfit) {
                    console.warn(`cannot generate report for ${outfitID}, not in outfits`);
                    return;
                }

                const start: number = Math.floor(this.alert.timestamp.getTime() / 1000);
                const end: number = start + this.alert.duration;

                const gen: string = `${start},${end},${outfit.factionID};o${outfitID};`;
                const url: string = `${location.origin}/report/${btoa(gen)}`;

                console.log(gen, btoa(gen), url);

                window.open(url, "_blank");
            }
        },

        computed: {
            sources: function() {
                return {
                    factions: [
                        { key: "All", value: null },
                        { key: "VS", value: 1 },
                        { key: "NC", value: 2 },
                        { key: "TR", value: 3 },
                        { key: "NS", value: 4 },
                    ]
                }
            }, 

            total: function() {
                return {
                    kills: (this.outfits.state == "loaded") ? this.outfits.data.reduce((acc: number, iter: any) => acc += iter.kills, 0) : 0
                }
            },

            disabled: function(): Loading<any> {
                return Loadable.idle();
            }
        },

        watch: {
            outfits: {
                deep: true,
                handler: function(): void {
                    console.log(`AlertOutfitKillBoard> outfits updated`);
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader, AFooter,
            InfoHover
        }
    });
    export default AlertOutfitKillBoard;
</script>
