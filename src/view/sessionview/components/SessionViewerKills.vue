<template>
    <div>
        <table class="table table-sm w-auto d-inline-block" style="vertical-align: top;">
            <thead>
                <tr>
                    <th>Weapon</th>
                    <th>Kills</th>
                    <th>HS kills</th>
                    <th>HSR</th>
                    <th>%</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="entry in groupedKillEventsArray">
                    <td>
                        <span v-if="groupedKillWeapons.get(entry[0])">
                            {{groupedKillWeapons.get(entry[0]).name}}
                        </span>
                        <span v-else>
                            &lt;missing {{entry[0]}}&gt;
                        </span>
                    </td>

                    <td>
                        {{entry[1].length | locale}}
                    </td>

                    <td>
                        {{entry[1].filter(iter => iter.isHeadshot == true).length}}
                    </td>

                    <td>
                        {{entry[1].filter(iter => iter.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%
                    </td>

                    <td>
                        {{entry[1].length / kills.data.length * 100 | fixed | locale}}%
                    </td>
                </tr>
            </tbody>
        </table>

        <canvas id="chart-kills-weapon-usage" style="max-height: 300px; max-width: 25%" class="w-auto d-inline-block"></canvas>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import { randomRGB, rgbToString } from "util/Color";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";

    export const SessionViewerKills = Vue.extend({
        props: {
            kills: { type: Object as PropType<Loading<ExpandedKillEvent[]>>, required: true }
        },

        data: function() {
            return {
                chart: null as Chart | null
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.generateKillWeaponChart();
            });
        },

        methods: {
            generateKillWeaponChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                }

                const groupedEvents: Map<string, KillEvent[]> = this.groupedKillEvents;

                const ctx = (document.getElementById("chart-kills-weapon-usage") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: Array.from(groupedEvents.entries()).map((iter) => {
                            const weaponID: string = iter[0];
                            const weaponName: string = `${this.groupedKillWeapons.get(weaponID)?.name ?? `<missing ${weaponID}>`}`;
                            return `${weaponName} - ${(iter[1].length / (this.kills as any).data.length * 100).toFixed(2)}%`;
                        }),
                        datasets: [{
                            data: Array.from(groupedEvents.values()).map(iter => iter.length),
                            backgroundColor: Array.from(groupedEvents.keys()).map(_ => rgbToString(randomRGB()))
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: "right",
                                labels: {
                                    color: "#fff"
                                }
                            }
                        }
                    }
                });
            },

        },

        computed: {
            groupedKillEvents: function(): Map<string, KillEvent[]> {
                if (this.kills.state != "loaded") {
                    return new Map();
                }

                return this.kills.data.reduce(
                    (entryMap: Map<string, KillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event.event]),
                    new Map()
                );
            },

            groupedKillWeapons: function(): Map<string, PsItem> {
                if (this.kills.state != "loaded") {
                    return new Map();
                }

                const map: Map<string, PsItem> = new Map();
                for (const iter of this.kills.data) {
                    if (map.has(iter.event.weaponID) == false && iter.item != null) {
                        map.set(iter.event.weaponID, iter.item);
                    }
                }

                return map;
            },

            groupedKillEventsArray: function(): any[] {
                return Array.from(this.groupedKillEvents.entries());
            }
        }

    });
    export default SessionViewerKills;
</script>