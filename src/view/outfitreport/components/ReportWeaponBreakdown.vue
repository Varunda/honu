<template>
    <div>
        <h2 class="wt-header">
            Weapons
        </h2>

        <h4>Weapon kills</h4>
        <div class="d-flex">
            <table class="table table-sm flex-grow-1 flex-basis-0">
                <tr class="table-secondary">
                    <td>Weapon</td>
                    <td>Kills</td>
                    <td>%</td>
                    <td>HSR%</td>
                </tr>

                <tr v-for="weapon in kills.slice(0, sliceSize)">
                    <td>
                        <a :href="'/i/' + weapon.itemID">
                            {{weapon.itemName}}
                        </a>
                    </td>
                    <td>{{weapon.kills}}</td>
                    <td>{{weapon.kills / report.kills.length * 100 | locale}}%</td>
                    <td>{{weapon.headshotKills / weapon.kills * 100 | locale}}%</td>
                </tr>

                <tr class="table-dark">
                    <td colspan="4">
                        Unique weapon kills: {{kills.length}} over {{report.kills.length}}
                        ({{report.kills.filter(iter => iter.isHeadshot == true).length / report.kills.length * 100 | locale}}% HSR)
                    </td>
                </tr>
            </table>

            <div class="flex-grow-1 flex-basis-0">
                <chart-block-pie-chart :data="killsBlock"
                    :show-percent="true" :show-total="true">
                </chart-block-pie-chart>
            </div>
        </div>

        <h4>Weapon deaths</h4>
        <div class="d-flex">
            <table class="table table-sm flex-grow-1 flex-basis-0">
                <tr class="table-secondary">
                    <td>Weapon</td>
                    <td>Deaths</td>
                    <td>%</td>
                    <td>HSR%</td>
                </tr>

                <tr v-for="weapon in deaths.slice(0, sliceSize)">
                    <td>
                        <a :href="'/i/' + weapon.itemID">
                            {{weapon.itemName}}
                        </a>
                    </td>
                    <td>{{weapon.kills}}</td>
                    <td>{{weapon.kills / report.kills.length * 100 | locale}}%</td>
                    <td>{{weapon.headshotKills / weapon.kills * 100 | locale}}%</td>
                </tr>

                <tr class="table-dark">
                    <td colspan="4">
                        Unique weapon deaths: {{deaths.length}} over {{report.deaths.length}}
                        ({{report.deaths.filter(iter => iter.isHeadshot == true).length / report.deaths.length * 100 | locale}}% HSR)
                    </td>
                </tr>
            </table>

            <div class="flex-grow-1 flex-basis-0">
                <chart-block-pie-chart :data="deathsBlock"
                    :show-percent="true" :show-total="true">
                </chart-block-pie-chart>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import { KillEvent } from "api/KillStatApi";

    import "filters/LocaleFilter";

    import { Block, BlockEntry } from "./charts/common";
    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";

    class WeaponEntry {
        public itemID: number = 0;
        public itemName: string = "";
        public kills: number = 0;
        public headshotKills: number = 0;
    }

    export const ReportWeaponBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                kills: [] as WeaponEntry[],
                deaths: [] as WeaponEntry[],

                killsBlock: new Block() as Block,
                deathsBlock: new Block() as Block,

                sliceSize: 10 as number
            }
        },

        mounted: function(): void {
            this.kills = this.make(this.report.kills);
            this.killsBlock.total = this.report.kills.length;
            this.killsBlock.entries = this.kills.map(iter => {
                return {
                    name: iter.itemName,
                    count: iter.kills
                };
            });


            this.deaths = this.make(this.report.deaths);
            this.deathsBlock.total = this.report.deaths.length;
            this.deathsBlock.entries = this.deaths.map(iter => {
                return {
                    name: iter.itemName,
                    count: iter.kills
                };
            });
        },

        methods: {
            make: function(events: KillEvent[]): WeaponEntry[] {
                const map: Map<number, WeaponEntry> = new Map();

                const noWeapon: WeaponEntry = new WeaponEntry();
                noWeapon.itemName = "<no weapon>";
                map.set(0, noWeapon);

                for (const kill of events) {
                    if (map.has(Number.parseInt(kill.weaponID)) == false) {
                        const entry: WeaponEntry = new WeaponEntry();

                        entry.itemID = Number.parseInt(kill.weaponID);
                        entry.itemName = this.report.items.get(entry.itemID)?.name ?? `<missing ${kill.weaponID}>`;
                        entry.kills = 0;
                        entry.headshotKills = 0;

                        map.set(entry.itemID, entry);
                    }

                    const entry: WeaponEntry = map.get(Number.parseInt(kill.weaponID))!;
                    ++entry.kills;
                    if (kill.isHeadshot == true) {
                        ++entry.headshotKills;
                    }

                    map.set(entry.itemID, entry);
                }

                return Array.from(map.values()).sort((a, b) => {
                    return b.kills - a.kills
                        || b.headshotKills - a.headshotKills
                        || b.itemName.localeCompare(a.itemName);
                });//.slice(0, this.sliceSize);
            }

        },

        components: {
            ChartBlockPieChart
        }
    });

    export default ReportWeaponBreakdown;
</script>