<template>
    <div>
        <div class="row">
            <div class="col-12 col-lg-6">
                <h3>Weapons</h3>

                <div class="row">
                    <div class="col-12 col-lg-6">
                        <table class="table table-sm w-100" style="vertical-align: top;">
                            <thead>
                                <tr class="table-secondary th-border-top-0">
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

                                <tr class="table-secondary th-border-top-0">
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
                    </div>

                    <div class="col-12 col-lg-6">
                        <canvas id="chart-kills-weapon-usage" style="max-height: 300px;" class="mb-2"></canvas>
                    </div>
                </div>
            </div>

            <div class="col-12 col-lg-6">
                <h3>Outfits</h3>

                <table class="table table-sm">
                    <thead>
                        <tr class="table-secondary th-border-top-0">
                            <th>Outfit</th>
                            <th>
                                Players
                                <info-hover text="How many unique players in this outfit were encountered"></info-hover>
                            </th>
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
                                {{entry.unique.size}}
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

            <div class="col-12 col-lg-6">
                <h3>Classes</h3>

                <table class="table table-sm">
                    <thead>
                        <tr class="table-secondary th-border-top-0">
                            <th>Class</th>
                            <th>
                                Kills
                                <info-hover text="Number of kills against this class"></info-hover>
                            </th>
                            <th>
                                Deaths
                                <info-hover text="Number of deaths from this class"></info-hover>
                            </th>
                            <th>K/D</th>
                            <th>
                                Kill HSR%
                                <info-hover text="Number of headshot kills against this class"></info-hover>
                            </th>
                            <th>
                                Death HSR%
                                <info-hover text="Number of headshot deaths from this class"></info-hover>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="clazz in classData">
                            <td>
                                <img :src="'/img/classes/' + clazz.icon" height="24" />
                                {{clazz.name}}
                            </td>
                            <td>
                                {{clazz.kills}}
                            </td>
                            <td>{{clazz.deaths}}</td>
                            <td>{{clazz.kills / Math.max(clazz.deaths, 1) | fixed}}</td>
                            <td>
                                {{clazz.killHeadshots}}
                                ({{clazz.killHeadshots / Math.max(1, clazz.kills) * 100 | fixed(2)}}%)
                            </td>
                            <td>
                                {{clazz.deathHeadshots}}
                                ({{clazz.deathHeadshots / Math.max(1, clazz.deaths) * 100 | fixed(2)}}%)
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import InfoHover from "components/InfoHover.vue";

    type OutfitKD = {
        outfitID: string;
        outfitTag: string | null;
        outfitName: string | null;
        kills: number;
        deaths: number;
        unique: Set<string>;
    }

    class ClassKD {
        public name: string = "";
        public icon: string = "";
        public kills: number = 0;
        public deaths: number = 0;
        public killHeadshots: number = 0;
        public deathHeadshots: number = 0;

        public constructor(name: string, icon: string) {
            this.name = name;
            this.icon = icon;
        }
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
                classData: [] as ClassKD[],
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.generateKillWeaponChart();
            });

            this.kpmData = this.kills.map(iter => iter.event.timestamp);
            this.generateOutfitData();
            this.generateClassData();
        },

        methods: {
            generateKillWeaponChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const groupedEvents: Map<number, KillEvent[]> = this.groupedKillEvents;

                const arr = Array.from(groupedEvents.entries()).sort((a, b) => b[1].length - a[1].length);

                const ctx = (document.getElementById("chart-kills-weapon-usage") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: arr.map((iter) => {
                            const weaponID: number = iter[0];
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

            generateClassData: function(): void {
                this.classData = [];

                const infil: ClassKD = new ClassKD("Infiltrator", "icon_infil.png");
                const lightAssault: ClassKD = new ClassKD("Light Assault", "icon_light.png");
                const medic: ClassKD = new ClassKD("Medic", "icon_medic.png");
                const engi: ClassKD = new ClassKD("Engineer", "icon_engi.png");
                const heavy: ClassKD = new ClassKD("Heavy Assault", "icon_heavy.png");
                const max: ClassKD = new ClassKD("MAX", "icon_max.png");

                //LoadoutUtils.getLoadoutName

                function getClass(name: string): ClassKD {
                    if (name == LoadoutUtils.NAME_INFILTRATOR) {
                        return infil;
                    } else if (name == LoadoutUtils.NAME_LIGHT_ASSAULT) {
                        return lightAssault;
                    } else if (name == LoadoutUtils.NAME_MEDIC) {
                        return medic;
                    } else if (name == LoadoutUtils.NAME_ENGINEER) {
                        return engi;
                    } else if (name == LoadoutUtils.NAME_HEAVY_ASSAULT) {
                        return heavy;
                    } else if (name == LoadoutUtils.NAME_MAX) {
                        return max;
                    }

                    throw `Unchecked loadout name: '${name}'`;
                }

                for (const ev of this.kills) {
                    const name: string = LoadoutUtils.getLoadoutName(ev.event.killedLoadoutID);
                    const clazz: ClassKD = getClass(name);

                    ++clazz.kills;
                    if (ev.event.isHeadshot == true) {
                        ++clazz.killHeadshots;
                    }
                }

                for (const ev of this.deaths) {
                    const name: string = LoadoutUtils.getLoadoutName(ev.event.attackerLoadoutID);
                    const clazz: ClassKD = getClass(name);

                    ++clazz.deaths;
                    if (ev.event.isHeadshot == true) {
                        ++clazz.deathHeadshots;
                    }
                }

                this.classData = [
                    infil, lightAssault, medic,
                    engi, heavy, max
                ];
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
                                deaths: 0,
                                unique: new Set()
                            };
                        } else {
                            outfit = {
                                outfitID: character.outfitID,
                                outfitTag: character.outfitTag,
                                outfitName: character.outfitName,
                                kills: 0,
                                deaths: 0,
                                unique: new Set()
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
                    outfit.unique.add(event.event.killedCharacterID);
                }

                for (const event of this.deaths) {
                    if (event.attacker == null) {
                        continue;
                    }

                    const outfit: OutfitKD = getOutfit(event.attacker);
                    ++outfit.deaths;
                    outfit.unique.add(event.event.attackerCharacterID);
                }

                this.outfitData = Array.from(outfitMap.values()).sort((a, b) => (b.kills + b.deaths) - (a.kills + a.deaths)).slice(0, 8);
            }

        },

        computed: {
            groupedKillEvents: function(): Map<number, KillEvent[]> {
                return this.kills.reduce(
                    (entryMap: Map<number, KillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event.event]),
                    new Map()
                );
            },

            groupedKillWeapons: function(): Map<number, PsItem> {
                const map: Map<number, PsItem> = new Map();
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
            ChartTimestamp,
            InfoHover
        }

    });
    export default SessionViewerKills;
</script>