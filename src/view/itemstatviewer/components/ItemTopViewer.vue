<template>
    <div>
        <h4 v-if="displayedStats.state == 'loaded' && displayedStats.data.length == 0" class="alert alert-warning text-center">
            There is no top data stored in the database. This means it either has yet to be generated, or this item you
            are viewing is not a weapon
        </h4>

        <h3 v-if="lastUpdated != null" class="alert alert-secondary text-center">
            This data was last updated on: {{lastUpdated | moment}}
        </h3>

        <div class="input-grid-col2" style="grid-template-columns: min-content min-content; row-gap: 0.5rem; justify-content: center">
            <div class="input-cell mr-2">
                Stat
            </div>

            <div class="btn-group input-cell">
                <button @click="loadKpm" type="button" class="btn" :class="[ column == 'kpm' ? 'btn-primary' : 'btn-secondary' ]">
                    KPM
                </button>

                <button @click="loadKd" type="button" class="btn" :class="[ column == 'kd' ? 'btn-primary' : 'btn-secondary' ]">
                    KD
                </button>

                <button @click="loadAcc" type="button" class="btn" :class="[ column == 'acc' ? 'btn-primary' : 'btn-secondary' ]">
                    Accuracy
                </button>

                <button @click="loadHsr" type="button" class="btn" :class="[ column == 'hsr' ? 'btn-primary' : 'btn-secondary' ]">
                    Headshot Ratio
                </button>

                <button @click="loadKills" type="button" class="btn" :class="[ column == 'kills' ? 'btn-primary' : 'btn-secondary' ]">
                    Kills
                </button>

                <button @click="loadVKPM" type="button" class="btn" :class="[ column == 'vkpm' ? 'btn-primary' : 'btn-secondary' ]">
                    Vehicle KPM
                </button>
            </div>

            <div class="input-cell mr-2">
                Servers
            </div>

            <div class="btn-group input-cell">
                <toggle-button v-model="world.connery">
                    Connery
                </toggle-button>

                <toggle-button v-model="world.cobalt">
                    Cobalt
                </toggle-button>

                <toggle-button v-model="world.emerald">
                    Emerald
                </toggle-button>

                <toggle-button v-model="world.miller">
                    Miller
                </toggle-button>

                <toggle-button v-model="world.soltech">
                    SolTech
                </toggle-button>

                <toggle-button v-model="world.jaeger">
                    Jaeger
                </toggle-button>
            </div>

            <div class="input-cell mr-2">
                Factions
            </div>

            <div class="btn-group input-cell">
                <toggle-button v-model="faction.vs" :true-color="factionColors.VS">
                    VS
                </toggle-button>

                <toggle-button v-model="faction.nc" :true-color="factionColors.NC">
                    NC
                </toggle-button>

                <toggle-button v-model="faction.tr" :true-color="factionColors.TR">
                    TR
                </toggle-button>

                <toggle-button v-model="faction.ns" :true-color="factionColors.NS">
                    NS
                </toggle-button>
            </div>
        </div>

        <hr />

        <div>

            <a-table
                :entries="displayedStats"
                display-type="table" row-padding="compact"
                :paginate="false">

                <a-col>
                    <a-header>
                        <b>Rank</b>
                    </a-header>

                    <a-body>
                        <a-rank></a-rank>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Character</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a :href="'/c/' + entry.character.id">
                            <span v-if="entry.character.outfitID != null">
                                [{{entry.character.outfitTag}}]
                            </span>
                            {{entry.character.name}}
                        </a>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Faction</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.character.factionID | faction}}
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Server</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.character.worldID | world}}
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Kills</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'kills' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.kills}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Deaths</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="text-center d-inline-block w-100">
                            {{entry.entry.deaths}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Time with</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="text-center d-inline-block w-100">
                            {{entry.entry.secondsWith | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>KPM</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'kpm' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.killsPerMinute | locale}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>KD</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'kd' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.killDeathRatio | locale}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Accuracy</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'acc' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.accuracy * 100 | locale}}%
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Headshot Ratio</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'hsr' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.headshotRatio * 100 | locale}}%
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Vehicle Kills</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'vkills' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.vehicleKills | locale}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <span class="text-center d-inline-block w-100">
                            <b>Vehicle KPM</b>
                        </span>
                    </a-header>

                    <a-body v-slot="entry">
                        <span class="w-100 h-100 d-inline-block text-center" :class="[ column == 'vkpm' ? 'selected-column table-secondary' : '' ]">
                            {{entry.entry.vehicleKillsPerMinute | locale}}
                        </span>
                    </a-body>
                </a-col>

            </a-table>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ColorUtils from "util/Color";
    import WorldUtils from "util/World";

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";

    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import { ExpandedWeaponStatEntry, CharacterWeaponStatApi, PercentileCacheType } from "api/CharacterWeaponStatApi";
    import { ExpandedWeaponStatTop, WeaponStatTopApi } from "api/WeaponStatTopApi";

    export const ItemTopViewer = Vue.extend({
        props: {
            ItemId: { type: String, required: true }
        },

        mounted: function(): void {
            this.loadAll();
            this.loadKpm();
        },

        data: function() {
            return {
                column: "kpm" as "kd" | "kpm" | "vkpm" | "hsr" | "acc" | "kills" | "vkills",
                count: 50 as number,

                world: {
                    connery: true as boolean,
                    cobalt: true as boolean,
                    emerald: true as boolean,
                    miller: true as boolean,
                    soltech: true as boolean,
                    jaeger: true as boolean
                },

                faction: {
                    vs: true as boolean,
                    nc: true as boolean,
                    tr: true as boolean,
                    ns: true as boolean,
                },

                all: Loadable.idle() as Loading<ExpandedWeaponStatTop[]>,
            };
        },

        methods: {
            loadAll: async function(): Promise<void> {
                this.all = Loadable.loading();
                this.all = await WeaponStatTopApi.getTopAll(Number.parseInt(this.ItemId));
            },

            loadKd: function(): void {
                this.column = "kd";
                this.count = 50;
            },

            loadKpm: function(): void {
                this.column = "kpm";
                this.count = 50;
            },

            loadAcc: function(): void {
                this.column = "acc";
                this.count = 50;
            },

            loadHsr: function(): void {
                this.column = "hsr";
                this.count = 50;
            },

            loadKills: function(): void {
                this.column = "kills";
                this.count = 200;
            },

            loadVKills: function(): void {
                this.column = "vkills";
                this.count = 50;
            },

            loadVKPM: function(): void {
                this.column = "vkpm";
                this.count = 50;
            },

            columnStyle: function(col: string): object {
                return {
                    margin: this.column == col ? "-0.3rem" : "",
                    padding: this.column == col ? "0.3rem" : "",
                };
            },
        },

        computed: {
            lastUpdated: function(): Date | null {
                if (this.all.state != "loaded") {
                    return null;
                }

                if (this.all.data.length == 0) {
                    return null;
                }

                return this.all.data[0].entry.timestamp;
            },

            displayedStats: function(): Loading<ExpandedWeaponStatTop[]> {
                if (this.all.state != "loaded") {
                    return Loadable.rewrap(this.all);
                }

                let typeID: number = 0;
                let sortFunc: ((iter: ExpandedWeaponStatTop) => number) | null = null;

                if (this.column == "kd") {
                    typeID = PercentileCacheType.KD;
                    sortFunc = iter => iter.entry.killDeathRatio;
                } else if (this.column == "kpm") {
                    typeID = PercentileCacheType.KPM;
                    sortFunc = iter => iter.entry.killsPerMinute;
                } else if (this.column == "vkpm") {
                    typeID = PercentileCacheType.VKPM;
                    sortFunc = iter => iter.entry.vehicleKillsPerMinute;
                } else if (this.column == "acc") {
                    typeID = PercentileCacheType.ACC;
                    sortFunc = iter => iter.entry.accuracy;
                } else if (this.column == "hsr") {
                    typeID = PercentileCacheType.HSR;
                    sortFunc = iter => iter.entry.headshotRatio;
                } else if (this.column == "kills") {
                    typeID = PercentileCacheType.KILLS;
                    sortFunc = iter => iter.entry.kills;
                } else if (this.column == "vkills") {
                    typeID = PercentileCacheType.VKPM;
                    sortFunc = iter => iter.entry.vehicleKillsPerMinute;
                } else {
                    throw `Unchecked value of column: '${this.column}'`;
                }

                if (sortFunc == null) {
                    throw `sortFunc is null somehow`;
                }

                const type: ExpandedWeaponStatTop[] = this.all.data.filter((iter: ExpandedWeaponStatTop) => {
                    return iter.entry.typeID == typeID
                        && iter.character != null
                        && this.worlds.indexOf(iter.character.worldID) > -1
                        && this.factions.indexOf(iter.character.factionID) > -1;
                }).sort((a, b) => sortFunc!(b) - sortFunc!(a)).slice(0, this.count);

                return Loadable.loaded(type);
            },

            worlds: function(): number[] {
                let worlds: number[] = [];

                if (this.world.connery == true) { worlds.push(WorldUtils.Connery ); }
                if (this.world.cobalt == true) { worlds.push(WorldUtils.Cobalt); }
                if (this.world.emerald == true) { worlds.push(WorldUtils.Emerald); }
                if (this.world.miller == true) { worlds.push(WorldUtils.Miller); }
                if (this.world.soltech == true) { worlds.push(WorldUtils.SolTech); }
                if (this.world.jaeger == true) { worlds.push(WorldUtils.Jaeger); }

                return worlds;
            },

            factions: function(): number[] {
                let factions: number[] = [];

                if (this.faction.vs == true) { factions.push(1); }
                if (this.faction.nc == true) { factions.push(2); }
                if (this.faction.tr == true) { factions.push(3); }
                if (this.faction.ns == true) { factions.push(4); }

                return factions;
            },

            factionColors: function() {
                return {
                    VS: ColorUtils.VS,
                    NC: ColorUtils.NC,
                    TR: ColorUtils.TR,
                    NS: ColorUtils.NS
                };
            }
        },

        components: {
            ATable,
            AFilter,
            AHeader,
            ABody,
            ACol,
            ARank,
            ToggleButton
        }
    });
    export default ItemTopViewer;
</script>