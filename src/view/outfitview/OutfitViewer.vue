<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="#">Outfit</a>

                <span>/</span>

                <span v-if="outfit.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else>
                    {{outfit.data.name}}
                </span>
            </h1>
        </div>

        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <hr class="border" />

        <div v-if="outfit.state == 'loading'">
            Loading...
        </div>

        <div v-if="outfit.state == 'loaded'">
            <table class="table table-sm w-auto">
                <tr>
                    <td><b>Tag</b></td>
                    <td>{{outfit.data.tag}}</td>
                </tr>

                <tr>
                    <td><b>Name</b></td>
                    <td>'{{outfit.data.name}}'</td>
                </tr>

                <tr>
                    <td><b>Faction</b></td>
                    <td>{{outfit.data.factionID | faction}}</td>
                </tr>

                <tr>
                    <td><b>Date created</b></td>
                    <td>
                        {{outfit.data.dateCreated | moment}}
                        ({{outfit.data.dateCreated | timeAgo}})
                    </td>
                </tr>

                <tr>
                    <td>
                        {{new Date() | moment}}
                    </td>
                    <td>
                        {{new Date() | moment}}
                    </td>
                </tr>

                <tr>
                    <td><b>Census</b></td>
                    <td>
                        <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/outfit?outfit_id=' + outfit.data.id" target="_blank">
                            Census
                            <span class="fas fa-external-link-alt"></span>
                        </a>
                    </td>
                </tr>
            </table>
        </div>

        <div>
            <h2 class="wt-header">
                Members
            </h2>

            <a-table 
                :entries="members"
                default-sort-field="rankOrder"
                default-sort-order="asc"
                display-type="table">

                <a-col sort-field="prestigeRank">
                    <a-header>
                        <b>BR</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.prestigeRank}}
                    </a-body>
                </a-col>

                <a-col sort-field="name">
                    <a-header>
                        <b>Character</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a :href="'/c/' + entry.characterID">
                            {{entry.name}}
                        </a>
                    </a-body>
                </a-col>

                <a-col sort-field="rankOrder">
                    <a-header>
                        <b>Rank</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.rank}}
                    </a-body>
                </a-col>

                <a-col sort-field="memberSince">
                    <a-header>
                        <b>Joined</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.memberSince | moment}}
                        ({{entry.memberSince | timeAgo}})
                    </a-body>
                </a-col>

                <a-col sort-field="lastLogin">
                    <a-header>
                        <b>Last login</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.lastLogin | moment}}
                        ({{entry.lastLogin | timeAgo}})
                    </a-body>
                </a-col>

                <a-col sort-field="worldID">
                    <a-header>
                        <b>Server</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.worldID | world}}
                    </a-body>
                </a-col>

                <a-col sort-field="recentKD">
                    <a-header>
                        <b>Recent KD</b>
                        <info-hover text="KD within the last 30 days"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        <quick-number :data="entry.recentKD"></quick-number>
                    </a-body>
                </a-col>

                <a-col sort-field="recentKPM">
                    <a-header>
                        <b>Recent KPM</b>
                        <info-hover text="KPM within the last 30 days"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        <quick-number :data="entry.recentKPM"></quick-number>
                    </a-body>
                </a-col>

                <a-col sort-field="recentSPM">
                    <a-header>
                        <b>Recent SPM</b>
                        <info-hover text="SPM within the last 30 days"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        <quick-number :data="entry.recentSPM"></quick-number>
                    </a-body>
                </a-col>

            </a-table>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";

    import { PsOutfit, OutfitApi, FlatExpandedOutfitMember } from "api/OutfitApi";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    const QuickNumber = Vue.extend({
        props: {
            data: { type: Number, required: false }
        },

        template: `
            <span>
                <span v-if="data == null" title="This data is yet to be pulled by Honu">
                    --
                </span>
                <span v-else>
                    {{data | locale}}
                </span>
            </span>
        `
    });

    export const OutfitViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfitID: "0" as string,

                outfit: Loadable.idle() as Loading<PsOutfit | null>,
                members: Loadable.idle() as Loading<FlatExpandedOutfitMember[]>,
            }
        },

        created: function(): void {
            document.title = `Honu / Outfit / <loading...>`;
        },

        beforeMount: function(): void {
            this.parseOutfitIDFromUrl();
            this.bindOutfit();
            this.bindMembers();
        },

        methods: {
            parseOutfitIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const outfitID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(outfitID) == false) {
                    this.outfitID = parts[2];
                    console.log(`outfit id is ${this.outfitID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${outfitID}`;
                }
            },

            bindOutfit: async function(): Promise<void> {
                this.outfit = Loadable.loading();
                this.outfit = await Loadable.promise(OutfitApi.getByID(this.outfitID));

                if (this.outfit.state == "loaded") {
                    document.title = `Honu / Outfit / ${this.outfit.data?.name}`;
                } else {
                    document.title = `Honu / Outfit / <not found>`;
                }
            },

            bindMembers: async function(): Promise<void> {
                this.members = Loadable.loading();
                this.members = await Loadable.promise(OutfitApi.getMembersFlat(this.outfitID));
            }

        },

        components: {
            ATable,
            ACol,
            ABody,
            AFilter,
            AHeader,
            InfoHover,
            QuickNumber
        }

    });
    export default OutfitViewer;
</script>