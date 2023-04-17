<template>
    <collapsible header-text="Weapons">

        <h3 class="wt-header" style="background-color: var(--cyan);">
            Kills
        </h3>

        <div class="row">
            <div class="col-12">
                <h4>
                    Weapon kills
                    <info-hover text="What weapons the tracked players got kills with during the time period"></info-hover>
                </h4>

                <weapon-breakdown :entries="kills" :block="killsBlock"
                    :link-item="true" :total="totalKills" :headshot-total="totalHeadshotKills">
                </weapon-breakdown>
            </div>

            <div class="col-12">
                <h4>
                    Weapon type kills
                </h4>

                <weapon-breakdown :entries="killTypes" :block="killTypesBlock" 
                    :link-item="false" :total="totalKills" :headshot-total="totalHeadshotKills">
                </weapon-breakdown>
            </div>
        </div>

        <h3 class="wt-header" style="background-color: var(--red);">
            Deaths
        </h3>

        <div class="row">
            <div class="col-12">
                <h4>
                    Weapon deaths
                    <info-hover text="What weapons tracked characters died to"></info-hover>
                </h4>

                <weapon-breakdown :entries="deaths" :block="deathsBlock"
                    :link-item="true" :total="totalDeaths" :headshot-total="totalHeadshotDeaths">
                </weapon-breakdown>
            </div>

            <div class="col-12">
                <h4>
                    Weapon type deaths
                </h4>

                <weapon-breakdown :entries="deathTypes" :block="deathTypesBlock"
                    :link-item="false" :total="totalDeaths" :headshot-total="totalHeadshotDeaths">
                </weapon-breakdown>
            </div>
        </div>

    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata, ReportParameters } from "../Report";

    import { KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { ItemCategory } from "api/ItemCategoryApi";
    import { FireGroupToFireMode } from "api/FireGroupToFireModeApi";

    import "filters/LocaleFilter";

    import { Block, BlockEntry } from "./charts/common";
    import { WeaponBreakdownEntry } from "./weapons/common";

    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";
    import WeaponBreakdown from "./weapons/WeaponBreakdown.vue";

    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";

    function breakdownToBlock(entries: WeaponBreakdownEntry[], total: number): Block {
        const block: Block = new Block();

        block.total = total;
        block.entries = entries.map(iter => {
            return {
                name: iter.name,
                count: iter.kills
            };
        });

        return block;

    }

    export const ReportWeaponBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                kills: [] as WeaponBreakdownEntry[],
                killTypes: [] as WeaponBreakdownEntry[],
                killsBlock: new Block() as Block,
                killTypesBlock: new Block() as Block,

                deaths: [] as WeaponBreakdownEntry[],
                deathTypes: [] as WeaponBreakdownEntry[],
                deathsBlock: new Block() as Block,
                deathTypesBlock: new Block() as Block,

                sliceSize: 10 as number
            }
        },

        mounted: function(): void {
            this.kills = this.make(this.report.kills);
            this.killsBlock = breakdownToBlock(this.kills, this.totalKills);

            this.killTypes = this.makeType(this.report.kills);
            this.killTypesBlock = breakdownToBlock(this.killTypes, this.totalKills);

            this.deaths = this.make(this.report.deaths);
            this.deathsBlock = breakdownToBlock(this.deaths, this.totalDeaths);

            this.deathTypes = this.makeType(this.report.deaths);
            this.deathTypesBlock = breakdownToBlock(this.deathTypes, this.totalDeaths);
        },

        methods: {
            makeType: function(events: KillEvent[]): WeaponBreakdownEntry[] {
                const map: Map<number, WeaponBreakdownEntry> = new Map();

                for (const kill of events) {
                    const item: PsItem | undefined = this.report.items.get(kill.weaponID);
                    const typeID: number = item?.categoryID ?? 0;
                    const cat: ItemCategory | undefined = this.report.itemCategories.get(typeID);

                    if (map.has(typeID) == false) {
                        const entry: WeaponBreakdownEntry = {
                            id: typeID,
                            name: typeID == 0 ? "<unknown weapon>" : `${(cat?.name ?? `<missing ${typeID}>`)}`,
                            kills: 0,
                            headshotKills: 0,
                            hipKills: 0,
                            adsKills: 0
                        };

                        map.set(entry.id, entry);
                    }

                    const entry: WeaponBreakdownEntry = map.get(typeID)!;
                    ++entry.kills;
                    if (kill.isHeadshot == true) {
                        ++entry.headshotKills;
                    }

                    const fireModeIndex: number | null = this.getFireModeIndex(kill.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++entry.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++entry.adsKills;
                    } else if (fireModeIndex != null) {
                        console.log(`got unchecked fireModeIndex ${fireModeIndex} from fire mode ${kill.attackerFireModeID}: ${JSON.stringify(kill)}`);
                    } else {
                        console.warn(`missing or have 0 fireModes for ${kill.attackerFireModeID}`);
                    }

                    map.set(entry.id, entry);
                }

                return Array.from(map.values()).sort((a, b) => {
                    return b.kills - a.kills
                        || b.headshotKills - a.headshotKills
                        || b.name.localeCompare(a.name);
                });
            },

            make: function(events: KillEvent[]): WeaponBreakdownEntry[] {
                const map: Map<number, WeaponBreakdownEntry> = new Map();

                const noWeapon: WeaponBreakdownEntry = {
                    id: 0,
                    name: "<no weapon>",
                    kills: 0,
                    headshotKills: 0,
                    adsKills: 0,
                    hipKills: 0
                };
                map.set(0, noWeapon);

                for (const kill of events) {
                    if (map.has(kill.weaponID) == false) {
                        const entry: WeaponBreakdownEntry = {
                            id: kill.weaponID,
                            name: this.report.items.get(kill.weaponID)?.name ?? `<missing ${kill.weaponID}>`,
                            kills: 0,
                            headshotKills: 0,
                            adsKills: 0,
                            hipKills: 0
                        };

                        map.set(entry.id, entry);
                    }

                    const entry: WeaponBreakdownEntry = map.get(kill.weaponID)!;
                    ++entry.kills;
                    if (kill.isHeadshot == true) {
                        ++entry.headshotKills;
                    }

                    const fireModeIndex: number | null = this.getFireModeIndex(kill.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++entry.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++entry.adsKills;
                    } else if (fireModeIndex != null) {
                        console.log(`got unchecked fireModeIndex ${fireModeIndex} from fire mode ${kill.attackerFireModeID}: ${JSON.stringify(kill)}`);
                    } else {
                        console.warn(`missing or have 0 fireModes for ${kill.attackerFireModeID}`);
                    }

                    map.set(entry.id, entry);
                }

                return Array.from(map.values()).sort((a, b) => {
                    return b.kills - a.kills
                        || b.headshotKills - a.headshotKills
                        || b.name.localeCompare(a.name);
                });
            },

            getFireModeIndex: function(fireModeID: number): number | null {
                const fireModes: FireGroupToFireMode[] | undefined = this.report.fireModeXrefs.get(fireModeID);
                if (fireModes != undefined && fireModes.length > 0) {
                    const fireModeIndex: number = fireModes[0].fireModeIndex;

                    return fireModeIndex;
                }

                return null;
            }
        },

        computed: {

            totalKills: function(): number {
                return this.report.kills.length;
            },

            totalHeadshotKills: function(): number {
                return this.report.kills.filter(iter => iter.isHeadshot == true).length;
            },

            totalDeaths: function(): number {
                return this.report.deaths.length;
            },

            totalHeadshotDeaths: function(): number {
                return this.report.deaths.filter(iter => iter.isHeadshot == true).length;
            }

        },

        components: {
            ChartBlockPieChart,
            InfoHover,
            Collapsible,
            WeaponBreakdown
        }
    });

    export default ReportWeaponBreakdown;
</script>