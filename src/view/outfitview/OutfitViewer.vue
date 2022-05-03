<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/outfitfinder">Outfits</a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <span v-if="outfit.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else>
                    {{outfit.data.name}}
                </span>
            </li>
        </honu-menu>

        <hr class="border" />

        <div v-if="outfit.state == 'loading'">
            Loading...
        </div>

        <div v-if="outfit.state == 'loaded'">
            <table class="table table-sm w-auto d-inline-block mr-2" style="vertical-align: top;">
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
                    <td><b>Leader</b></td>
                    <td>
                        <a :href="'/c/' + outfit.data.leaderID">
                            <span v-if="leader.state == 'loading'">
                                &lt;loading...&gt;
                            </span>
                            <span v-else-if="leader.state == 'loaded'">
                                {{leader.data | characterName}}
                            </span>
                        </a>

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

            <table v-if="members.state == 'loaded'" class="table table-sm w-auto d-inline-block mr-2" style="vertical-align: top;">
                <tr>
                    <td><b>Members</b></td>
                    <td>{{outfit.data.memberCount}}</td>
                </tr>

                <tr>
                    <td><b>Online</b></td>
                    <td>{{onlineCount}}</td>
                </tr>

                <tr>
                    <td>
                        <b>Mean login time</b>
                        <info-hover text="Only of loaded characters">
                        </info-hover>
                    </td>

                    <td>
                        {{meanLoginDate | moment}}
                    </td>
                </tr>

                <tr>
                    <td><b>Recent KPM</b></td>
                    <td>{{recentKPM | locale(2)}}</td>
                </tr>

                <tr>
                    <td><b>Recent KD</b></td>
                    <td>{{recentKD | locale(2)}}</td>
                </tr>

                <tr>
                    <td><b>Recent SPM</b></td>
                    <td>{{recentSPM | locale(2)}}</td>
                </tr>
            </table>

            <table v-if="members.state == 'loaded'" class="table table-sm w-auto d-inline-block" style="vertical-align: top;">
                <tr>
                    <td><b>Active (30 days)</b></td>
                    <td>
                        {{active30d.length}}
                        ({{active30d.length / members.data.length * 100 | locale(2)}}%)
                    </td>
                </tr>

                <tr>
                    <td><b>Active (7 days)</b></td>
                    <td>
                        {{active7d.length}}
                        ({{active7d.length / members.data.length * 100 | locale(2)}}%)
                    </td>
                </tr>

                <tr>
                    <td><b>Active (24 hours)</b></td>
                    <td>
                        {{active24h.length}}
                        ({{active24h.length / members.data.length * 100 | locale(2)}}%)
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
                :show-filters="true"
                default-sort-field="rankOrder" default-sort-order="asc"
                display-type="table">

                <a-col sort-field="online">
                    <a-header></a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.online == true" style="color: green;" title="Online">
                            ●
                        </span>
                        <span v-else style="color: red;" title="Offline">
                            ●
                        </span>
                    </a-body>
                </a-col>

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

                    <a-filter field="name" method="input" type="string"
                        :conditions="[ 'contains' ]">
                    </a-filter>

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
    import "filters/CharacterName";

    import { PsOutfit, OutfitApi, FlatExpandedOutfitMember } from "api/OutfitApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

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
                    {{data | locale(2)}}
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

                leader: Loadable.idle() as Loading<PsCharacter>,

                outfit: Loadable.idle() as Loading<PsOutfit>,
                members: Loadable.idle() as Loading<FlatExpandedOutfitMember[]>,

                activeCutoff: 1000 * 60 * 60 * 24 * 30 as number,
                meanLoginDate: new Date() as Date
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
                this.outfit = await OutfitApi.getByID(this.outfitID);

                if (this.outfit.state == "loaded") {
                    document.title = `Honu / Outfit / ${this.outfit.data.name}`;
                    this.leader = Loadable.loading();
                    this.leader = await CharacterApi.getByID(this.outfit.data.leaderID);

                    if (this.outfit.data.tag != null) {
                        const url = new URL(location.href);
                        url.searchParams.set("tag", this.outfit.data.tag);
                        history.replaceState({ path: url.href }, "", url.href);
                    }
                } else {
                    document.title = `Honu / Outfit / <not found>`;
                }
            },

            bindMembers: async function(): Promise<void> {
                this.members = Loadable.loading();
                this.members = await OutfitApi.getMembersFlat(this.outfitID);

                if (this.members.state == "loaded") {
                    const logins: Date[] = this.members.data.filter(iter => iter.lastLogin != null).map(iter => iter.lastLogin!);
                    const sum: number = logins.map(iter => iter.getTime()).reduce((acc, iter) => acc += iter, 0);
                    const avg: number = sum / logins.length;
                    this.meanLoginDate = new Date(avg);
                }
            }

        },

        computed: {
            onlineCount: function(): number {
                if (this.members.state != "loaded") {
                    return -1;
                }
                return this.members.data.filter(iter => iter.online == true).length;
            },

            active30d: function(): FlatExpandedOutfitMember[] {
                if (this.members.state != "loaded") {
                    return [];
                }

                const now: number = new Date().getTime();

                return this.members.data.filter(iter => {
                    return iter.lastLogin != null
                        && (now - iter.lastLogin.getTime()) <= this.activeCutoff
                });
            },

            active24h: function(): FlatExpandedOutfitMember[] {
                if (this.members.state != "loaded") {
                    return [];
                }

                const now: number = new Date().getTime();

                return this.members.data.filter(iter => {
                    return iter.lastLogin != null
                        && (now - iter.lastLogin.getTime()) <= (1000 * 60 * 60 * 24);
                });
            },

            active7d: function(): FlatExpandedOutfitMember[] {
                if (this.members.state != "loaded") {
                    return [];
                }

                const now: number = new Date().getTime();

                return this.members.data.filter(iter => {
                    return iter.lastLogin != null
                        && (now - iter.lastLogin.getTime()) <= (1000 * 60 * 60 * 24 * 7);
                });
            },

            recentKPM: function(): number {
                if (this.members.state != "loaded") {
                    return -1;
                }

                const set: FlatExpandedOutfitMember[] = this.active30d.filter(iter => iter.recentKPM != null && iter.recentKPM > 0);
                const acc: number = set.reduce((acc, iter) => acc += iter.recentKPM!, 0);

                return acc / set.length;
            },

            recentKD: function(): number {
                if (this.members.state != "loaded") {
                    return -1;
                }

                const set: FlatExpandedOutfitMember[] = this.active30d.filter(iter => iter.recentKD != null && iter.recentKD > 0);
                const acc: number = set.reduce((acc, iter) => acc += iter.recentKD!, 0);

                return acc / set.length;
            },

            recentSPM: function(): number {
                if (this.members.state != "loaded") {
                    return -1;
                }

                const set: FlatExpandedOutfitMember[] = this.active30d.filter(iter => iter.recentSPM != null && iter.recentSPM > 0);
                const acc: number = set.reduce((acc, iter) => acc += iter.recentSPM!, 0);

                return acc / set.length;
            },

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            QuickNumber,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default OutfitViewer;
</script>