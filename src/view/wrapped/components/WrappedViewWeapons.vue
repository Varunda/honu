<template>
    <div>
        <collapsible header-text="Weapons">

            <div>
                <h3 class="wt-header" style="background-color: var(--purple)">
                    Weapons
                </h3>

                <a-table
                         :entries="weaponData"
                         :paginate="true" :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc">

                    <a-col sort-field="name">
                        <a-header>
                            Weapon
                        </a-header>

                        <a-body v-slot="entry">
                            <a v-if="entry.id != 0" :href="'/i/' + entry.id">
                                {{entry.name}}
                            </a>
                            <span v-else>
                                {{entry.name}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            Kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshots">
                        <a-header>
                            Headshots
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshots | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hip kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale}}
                        </a-body>
                    </a-col>

                </a-table>
            </div>

        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // models
    import { PsItem } from "api/ItemApi";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    class WrappedWeaponData {
        public id: number = 0;
        public item: PsItem | null = null;
        public name: string = "";

        public kills: number = 0;
        public headshots: number = 0;
        public adsKills: number = 0;
        public hipKills: number = 0;
    }

    export const WrappedViewWeapons = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                weaponData: Loadable.idle() as Loading<WrappedWeaponData[]>,

                filter: {
                    infil: true as boolean,
                    lightAssault: true as boolean,
                    medic: true as boolean,
                    engi: true as boolean,
                    heavy: true as boolean,
                    max: true as boolean
                }
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {
            makeAll: function(): void {
                this.makeWeaponData();
            },

            makeWeaponData: function(): void {
                const map: Map<number, WrappedWeaponData> = new Map();

                for (const ev of this.wrapped.kills) {
                    let data: WrappedWeaponData | undefined = map.get(ev.weaponID);

                    if (data == undefined) {
                        data = new WrappedWeaponData();
                        data.id = ev.weaponID;
                        data.item = this.wrapped.items.get(ev.weaponID) ?? null;

                        if (ev.weaponID == 0) {
                            data.name = "no weapon";
                        } else {
                            data.name = data.item?.name ?? `<missing ${ev.weaponID}>`;
                        }
                    }

                    ++data.kills;

                    if (ev.isHeadshot == true) {
                        ++data.headshots;
                    }

                    const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, ev.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++data.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++data.adsKills;
                    }

                    map.set(ev.weaponID, data);
                }

                this.weaponData = Loadable.loaded(Array.from(map.values()));
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ToggleButton,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewWeapons;
</script>