<template>
    <collapsible header-text="Character interactions">
        <div>
            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-1)">
                    Players fought
                </h3>

                <a-table :entries="characterFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 1.5rem;"></faction-image>

                            <a :href="'/c/' + entry.id" style="color: var(--p-red-1)" class="bright-150">
                                {{entry.displayName}}
                            </a>
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            <b>Server</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.worldID | world}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            <b>Kills</b>
                            <info-hover text="How many times an input character killed this character"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                            ({{entry.kills / wrapped.kills.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            <b>Deaths</b>
                            <info-hover text="How many times an input character was killed by this character. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                            ({{entry.deaths / wrapped.deaths.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="kd">
                        <a-header>
                            K/D
                            <info-hover text="Kills / Deaths. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotKills">
                        <a-header>
                            Headshot kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotKills | locale(0)}}
                            ({{entry.headshotKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotDeaths">
                        <a-header>
                            Headshot deaths (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotDeaths | locale(0)}}
                            ({{entry.headshotDeathRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hipfire kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale(0)}}
                            ({{entry.hipKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale(0)}}
                            ({{entry.adsKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamkills">
                        <a-header>
                            Teamkills
                            <info-hover text="How many times you teamkilled this character"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamkills | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamdeaths">
                        <a-header>
                            Teamdeaths
                            <info-hover text="How many times you were teamkilled by this character"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamdeaths | locale(0)}}
                        </a-body>
                    </a-col>
                </a-table>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-2)">
                    Outfits fought
                </h3>

                <a-table :entries="outfitFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Outfit</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <span v-if="entry.id.startsWith('0')" style="color: var(--p-red-2)" class="bright-150">
                                {{entry.displayName}}
                            </span>
                            <span v-else>
                                <a :href="'/o/' + entry.id" style="color: var(--p-red-2)" class="bright-150">
                                    {{entry.displayName}}
                                </a>
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            <b>Kills</b>
                            <info-hover text="How many times an input character killed this character."></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                            ({{entry.kills / wrapped.kills.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            <b>Deaths</b>
                            <info-hover text="How many times an input character was killed by this character. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                            ({{entry.deaths / wrapped.deaths.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="kd">
                        <a-header>
                            K/D
                            <info-hover text="Kills / Deaths. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotKills">
                        <a-header>
                            Headshot kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotKills | locale(0)}}
                            ({{entry.headshotKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotDeaths">
                        <a-header>
                            Headshot deaths (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotDeaths | locale(0)}}
                            ({{entry.headshotDeathRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hipfire kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale(0)}}
                            ({{entry.hipKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale(0)}}
                            ({{entry.adsKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamkills">
                        <a-header>
                            Teamkills
                            <info-hover text="How many times you teamkilled members of this outfit"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamkills | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamdeaths">
                        <a-header>
                            Teamdeaths
                            <info-hover text="How many times you were teamkilled by members of this outfit"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamdeaths | locale(0)}}
                        </a-body>
                    </a-col>
                </a-table>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-3)">
                    Players supported
                </h3>

                <a-table 
                    :entries="characterSupport"
                    :paginate="true"
                    :show-filters="true"
                    :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                    default-sort-field="heals" default-sort-order="desc"
                    class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                            :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <a :href="'/c/' + entry.id" style="color: var(--p-red-3)" class="bright-150">
                                {{entry.displayName}}
                            </a>
                        </a-body>
                    </a-col>

                    <a-col sort-field="heals">
                        <a-header>
                            <b>Healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.heals | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="healthHealed">
                        <a-header>
                            <b>Health healed</b>
                            <info-hover text="Estimate! This estimate tends to under count, so this value is likely higher"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            <span :title="entry.healthHealed | locale(0)">
                                {{entry.healthHealed | compact}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="revives">
                        <a-header>
                            <b>Revived</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.revives | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="resupplies">
                        <a-header>
                            <b>Resupplied</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.resupplies | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="maxRepairs">
                        <a-header>
                            <b>MAX repaired</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.maxRepairs | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="shieldRepairs">
                        <a-header>
                            <b>Shield repairs</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.shieldRepairs | locale}}
                        </a-body>
                    </a-col>

                </a-table>
            </div>


            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-4)">
                    Outfits supported
                </h3>

                <a-table 
                    :entries="outfitSupport"
                    :paginate="true"
                    :show-filters="true"
                    :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                    default-sort-field="heals" default-sort-order="desc"
                    class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Outfit</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                            :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <span v-if="entry.id.startsWith('0')" style="color: var(--p-red-4)" class="bright-150">
                                {{entry.displayName}}
                            </span>
                            <span v-else>
                                <a :href="'/o/' + entry.id" style="color: var(--p-red-4)" class="bright-150">
                                    {{entry.displayName}}
                                </a>
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="heals">
                        <a-header>
                            <b>Healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.heals | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="healthHealed">
                        <a-header>
                            <b>Health healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            <span :title="entry.healthHealed | locale(0)">
                                {{entry.healthHealed | compact}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="revives">
                        <a-header>
                            <b>Revived</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.revives | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="resupplies">
                        <a-header>
                            <b>Resupplied</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.resupplies | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="maxRepairs">
                        <a-header>
                            <b>MAX repaired</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.maxRepairs | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="shieldRepairs">
                        <a-header>
                            <b>Shield repairs</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.shieldRepairs | locale}}
                        </a-body>
                    </a-col>

                </a-table>
            </div>
        </div>
    </collapsible>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import FactionImage from "components/FactionImage";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // util
    import ColorUtils, { RGB } from "util/Color";

    // models
    import { EntityFought, EntitySupported, Entity } from "../common";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CompactFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";

    // #E74C3C (231, 76, 60)
    // #F39C12 (243, 158, 18)

    const red: RGB = {
        red: 231,
        green: 76,
        blue: 60
    };

    const green: RGB = {
        red: 243,
        green: 158,
        blue: 18
    };

    export const WrappedViewCharacterInteractions = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                characterFight: Loadable.idle() as Loading<EntityFought[]>,
                characterSupport: Loadable.idle() as Loading<EntitySupported[]>,

                outfitFight: Loadable.idle() as Loading<EntityFought[]>,
                outfitSupport: Loadable.idle() as Loading<EntitySupported[]>,

                colors: [
                    ColorUtils.rgbToString(red),
                    ColorUtils.rgbToString(ColorUtils.colorGradient(0.33, red, green)),
                    ColorUtils.rgbToString(ColorUtils.colorGradient(0.66, red, green)),
                    ColorUtils.rgbToString(green),
                ] as string[]
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {
            makeAll: function(): void {
                this.characterFight = Loadable.loaded(this.wrapped.extra.characterFight);
                this.outfitFight = Loadable.loaded(this.wrapped.extra.outfitFight);
                this.characterSupport = Loadable.loaded(this.wrapped.extra.characterSupport);
                this.outfitSupport = Loadable.loaded(this.wrapped.extra.outfitSupport);
            }
        },

        components: {
            Collapsible,
            InfoHover,
            FactionImage,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewCharacterInteractions;
</script>