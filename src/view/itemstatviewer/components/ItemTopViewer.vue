<template>
    <div>
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

            <div class="input-cell" style="grid-column: 1 / span 2;">
                <button type="button" class="btn btn-primary w-100" @click="load">
                    Load
                </button>
            </div>
        </div>

        <hr />

        <div>
            <a-table
                :entries="entries"
                display-type="table" row-padding="compact"
                :paginate="false">

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
            </a-table>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ColorUtils from "util/Color";
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";

    import { ATable, AFilter, AHeader, ABody, ACol } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import { ExpandedWeaponStatEntry, CharacterWeaponStatApi } from "api/CharacterWeaponStatApi";

    export const ItemTopViewer = Vue.extend({
        props: {
            ItemId: { type: String, required: true }
        },

        mounted: function(): void {
            this.loadKpm();
        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,

                column: "kpm" as "kd" | "kpm" | "hsr" | "acc" | "kills",

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

                kd: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                kpm: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                acc: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                hsr: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                kills: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>
            };
        },

        methods: {
            load: function(): void {
                if (this.column == "kd") {
                    this.loadKd();
                } else if (this.column == "kpm") {
                    this.loadKpm();
                } else if (this.column == "acc") {
                    this.loadAcc();
                } else if (this.column == "hsr") {
                    this.loadHsr();
                } else if (this.column == "kills") {
                    this.loadKills();
                }
            },

            loadKd: function(): void {
                this.column = "kd";

                if (this.kd.state == "idle") {
                    this.kd = Loadable.loading();
                    CharacterWeaponStatApi.getTopKD(this.ItemId, this.worlds, this.factions).then(iter => {
                        this.kd = iter;
                        this.entries = this.kd;
                    });
                }
                this.entries = this.kd;
            },

            loadKpm: function(): void {
                this.column = "kpm";

                if (this.kpm.state == "idle") {
                    console.log(`doing first load of KPM`);
                    this.kpm = Loadable.loading();
                    CharacterWeaponStatApi.getTopKPM(this.ItemId, this.worlds, this.factions).then(iter => {
                        this.kpm = iter;
                        this.entries = this.kpm;
                    });
                }

                this.entries = this.kpm;
            },

            loadAcc: function(): void {
                this.column = "acc";

                if (this.acc.state == "idle") {
                    this.acc = Loadable.loading();
                    CharacterWeaponStatApi.getTopAccuracy(this.ItemId, this.worlds, this.factions).then(iter => {
                        this.acc = iter;
                        this.entries = this.acc;
                    });
                }

                this.entries = this.acc;
            },

            loadHsr: function(): void {
                this.column = "hsr";

                if (this.hsr.state == "idle") {
                    this.hsr = Loadable.loading();
                    CharacterWeaponStatApi.getTopHeadshotRatio(this.ItemId, this.worlds, this.factions).then(iter => {
                        this.hsr = iter;
                        this.entries = this.hsr;
                    });
                }

                this.entries = this.hsr;
            },

            loadKills: function(): void {
                this.column = "kills";

                if (this.kills.state == "idle") {
                    this.kills = Loadable.loading();
                    CharacterWeaponStatApi.getTopKills(this.ItemId, this.worlds, this.factions).then(iter => {
                        this.kills = iter;
                        this.entries = this.kills;
                    });
                }

                this.entries = this.kills;
            },

            columnStyle: function(col: string): object {
                return {
                    margin: this.column == col ? "-0.3rem" : "",
                    padding: this.column == col ? "0.3rem" : "",
                };
            },

            clearData: function(): void {
                this.kd = Loadable.idle();
                this.kpm = Loadable.idle();
                this.acc = Loadable.idle();
                this.hsr = Loadable.idle();
                this.kills = Loadable.idle();
            },
        },

        watch: {
            world: {
                handler: function(): void {
                    console.log("yuh");
                    this.clearData();
                },
                deep: true
            },

            faction: {
                handler: function(): void {
                    this.clearData();
                },
                deep: true
            }
        },

        computed: {
            worlds: function(): number[] {
                let worlds: number[] = [];

                if (this.world.connery == true) { worlds.push(1); }
                if (this.world.cobalt == true) { worlds.push(13); }
                if (this.world.emerald == true) { worlds.push(17); }
                if (this.world.miller == true) { worlds.push(10); }
                if (this.world.soltech == true) { worlds.push(40); }
                if (this.world.jaeger == true) { worlds.push(19); }

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
            ToggleButton
        }

    });
    export default ItemTopViewer;
</script>