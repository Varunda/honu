<template>
    <div>
        <div>
            <div class="btn-group">
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
        </div>

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

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/WorldNameFilter";

    import { ATable, AFilter, AHeader, ABody, ACol } from "components/ATable";

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

                kd: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                kpm: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                acc: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                hsr: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>,
                kills: Loadable.idle() as Loading<ExpandedWeaponStatEntry[]>
            };
        },

        methods: {
            loadKd: function(): void {
                this.column = "kd";

                if (this.kd.state == "idle") {
                    this.kd = Loadable.loading();
                    Loadable.promise(CharacterWeaponStatApi.getTopKD(this.ItemId))
                        .then(iter => { this.kd = iter; this.entries = this.kd; });
                } else {
                    this.entries = this.kd;
                }
            },

            loadKpm: function(): void {
                this.column = "kpm";

                if (this.kpm.state == "idle") {
                    this.kpm = Loadable.loading();
                    Loadable.promise(CharacterWeaponStatApi.getTopKPM(this.ItemId))
                        .then(iter => { this.kpm = iter; this.entries = this.kpm; });
                } else {
                    this.entries = this.kpm;
                }
            },

            loadAcc: function(): void {
                this.column = "acc";

                if (this.acc.state == "idle") {
                    this.acc = Loadable.loading();
                    Loadable.promise(CharacterWeaponStatApi.getTopAccuracy(this.ItemId))
                        .then(iter => { this.acc = iter; this.entries = this.acc; });
                } else {
                    this.entries = this.acc;
                }
            },

            loadHsr: function(): void {
                this.column = "hsr";

                if (this.hsr.state == "idle") {
                    this.hsr = Loadable.loading();
                    Loadable.promise(CharacterWeaponStatApi.getTopHeadshotRatio(this.ItemId))
                        .then(iter => { this.hsr = iter; this.entries = this.hsr; });
                } else {
                    this.entries = this.hsr;
                }
            },

            loadKills: function(): void {
                this.column = "kills";

                if (this.kills.state == "idle") {
                    this.kills = Loadable.loading();
                    Loadable.promise(CharacterWeaponStatApi.getTopKills(this.ItemId))
                        .then(iter => { this.kills = iter; this.entries = this.kills; });
                } else {
                    this.entries = this.kills;
                }
            },

            columnStyle: function(col: string): object {
                return {
                    margin: this.column == col ? "-0.3rem" : "",
                    padding: this.column == col ? "0.3rem" : "",
                };
            }
        },

        components: {
            ATable,
            AFilter,
            AHeader,
            ABody,
            ACol
        }

    });
    export default ItemTopViewer;
</script>