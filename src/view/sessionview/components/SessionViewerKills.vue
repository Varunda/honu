<template>
    <div class="d-flex">

        <div class="flex-grow-1 flex-basis-0">
            <h3>Weapons</h3>

            <table class="table table-sm w-auto d-inline-block" style="vertical-align: top;">
                <thead>
                    <tr class="table-secondary">
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
                            <a :href="'/i/' + entry[0]">
                                <span v-if="groupedKillWeapons.get(entry[0])">
                                    {{groupedKillWeapons.get(entry[0]).name}}
                                </span>
                                <span v-else>
                                    &lt;missing {{entry[0]}}&gt;
                                </span>
                            </a>
                        </td>

                        <td>
                            {{entry[1].length}}
                        </td>

                        <td>
                            {{entry[1].filter(iter => iter.isHeadshot == true).length}}
                        </td>

                        <td>
                            {{entry[1].filter(iter => iter.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%
                        </td>

                        <td>
                            {{entry[1].length / kills.length * 100 | fixed | locale}}%
                        </td>
                    </tr>

                    <tr class="table-secondary">
                        <td>
                            <b>Total</b>
                        </td>

                        <td>
                            {{kills.length | locale}}
                        </td>

                        <td colspan="3">
                            {{kills.filter(iter => iter.event.isHeadshot == true).length / kills.length * 100 | fixed | locale}}%
                        </td>
                    </tr>
                </tbody>
            </table>

            <canvas id="chart-kills-weapon-usage" style="max-height: 300px; max-width: 50%;" class="d-inline-block mb-2"></canvas>
        </div>

        <div class="flex-grow-1 flex-basis-0">
            <h3>Outfits</h3>

            <table class="table table-sm">
                <thead>
                    <tr class="table-secondary">
                        <th>Outfit</th>
                        <th>Kills</th>
                        <th>Deaths</th>
                        <th>K/D</th>
                    </tr>
                </thead>

                <tbody>
                    <tr v-for="entry in outfitData">
                        <td>
                            <span v-if="entry.outfitID == '0'">
                                no outfit
                            </span>

                            <a v-else :href="'/o/' + entry.outfitID">
                                <span v-if="entry.outfitTag != null">
                                    [{{entry.outfitTag}}]
                                </span>
                                {{entry.outfitName}}
                            </a>
                        </td>
                        <td>
                            {{entry.kills}}
                        </td>
                        <td>
                            {{entry.deaths}}
                        </td>
                        <td>
                            {{entry.kills / Math.max(entry.deaths, 1) | fixed}}
                        </td>
                    </tr>
                </tbody>
            </table>

        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";
    import * as moment from "moment";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import ColorUtils from "util/Color";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";

    import ChartTimestamp from "./ChartTimestamp.vue";

    interface OutfitKD {
        outfitID: string;
        outfitTag: string | null;
        outfitName: string | null;
        kills: number;
        deaths: number;
    }

    export const SessionViewerKills = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            deaths: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
        },

        data: function() {
            return {
                chart: null as Chart | null,

                kpmData: [] as Date[],

                outfitData: [] as OutfitKD[],
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.generateKillWeaponChart();
            });

            this.kpmData = this.kills.map(iter => iter.event.timestamp);
            this.generateOutfitData();
        },

        methods: {
            generateKillWeaponChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const groupedEvents: Map<string, KillEvent[]> = this.groupedKillEvents;

                const arr = Array.from(groupedEvents.entries()).sort((a, b) => b[1].length - a[1].length);

                const ctx = (document.getElementById("chart-kills-weapon-usage") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: arr.map((iter) => {
                            const weaponID: string = iter[0];
                            const weaponName: string = `${this.groupedKillWeapons.get(weaponID)?.name ?? `<missing ${weaponID}>`}`;
                            return `${weaponName} - ${(iter[1].length / this.kills.length * 100).toFixed(2)}%`;
                        }),
                        datasets: [{
                            data: arr.map(iter => iter[1].length),
                            backgroundColor: ColorUtils.randomColors(Math.random(), arr.length)
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
                        },
                    }
                });
            },

            generateOutfitData: function(): void {
                this.outfitData = [];

                const outfitMap: Map<string, OutfitKD> = new Map();

                function getOutfit(character: PsCharacter): OutfitKD {
                    let outfit: OutfitKD | null = outfitMap.get(character.outfitID ?? "0") || null;
                    if (outfit == null) {
                        if (character.outfitID == null) {
                            outfit = {
                                outfitID: "0",
                                outfitTag: null,
                                outfitName: "<no outfit>",
                                kills: 0,
                                deaths: 0
                            };
                        } else {
                            outfit = {
                                outfitID: character.outfitID,
                                outfitTag: character.outfitTag,
                                outfitName: character.outfitName,
                                kills: 0,
                                deaths: 0
                            };
                        }

                        outfitMap.set(outfit.outfitID, outfit);
                    }
                    return outfit;
                }

                for (const event of this.kills) {
                    if (event.killed == null) {
                        continue;
                    }

                    const outfit: OutfitKD = getOutfit(event.killed);
                    ++outfit.kills;
                }

                for (const event of this.deaths) {
                    if (event.attacker == null) {
                        continue;
                    }

                    const outfit: OutfitKD = getOutfit(event.attacker);
                    ++outfit.deaths;
                }

                this.outfitData = Array.from(outfitMap.values()).sort((a, b) => (b.kills + b.deaths) - (a.kills + a.deaths)).slice(0, 8);
            }

        },

        computed: {
            groupedKillEvents: function(): Map<string, KillEvent[]> {
                return this.kills.reduce(
                    (entryMap: Map<string, KillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event.event]),
                    new Map()
                );
            },

            groupedKillWeapons: function(): Map<string, PsItem> {
                const map: Map<string, PsItem> = new Map();
                for (const iter of this.kills) {
                    if (map.has(iter.event.weaponID) == false && iter.item != null) {
                        map.set(iter.event.weaponID, iter.item);
                    }
                }

                return map;
            },

            groupedKillEventsArray: function(): any[] {
                return Array.from(this.groupedKillEvents.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            groupedOutfitEvents: function(): Map<string, KillEvent[]> {
                return this.kills.filter(iter => iter.killed != null && iter.event.attackerTeamID != iter.event.killedTeamID).reduce(
                    (entryMap: Map<string, KillEvent[]>, event: ExpandedKillEvent) => {
                        const entry: string = event.killed!.outfitID == null ? "No outfit" : `[${event.killed!.outfitTag}] ${event.killed!.outfitName}`;
                        return entryMap.set(entry, [...entryMap.get(entry) || [], event.event]);
                    },
                    new Map()
                );
            },

            groupedOutfitEventsArray: function(): any[] {
                return Array.from(this.groupedOutfitEvents.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            durationInSeconds: function(): number {
                return ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000;
            }
        },

        components: {
            ChartTimestamp
        }

    });
    export default SessionViewerKills;
</script>