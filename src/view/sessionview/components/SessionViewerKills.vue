<template>
    <div>
        <h3 class="wt-header" style="background-color: var(--purple)">
            Versus
        </h3>

        <div class="d-flex">
            <div class="mr-3">
                <h3>Outfits</h3>

                <table class="table table-sm" style="width: auto;">
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

            <div class="">
                <h3>Classes</h3>

                <table class="table table-sm" style="width: auto;">
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

        <h3 class="wt-header" style="background-color: var(--green)">
            Kills
        </h3>

        <div class="d-flex flex-wrap">
            <div class="mr-3">
                <h3>Weapon kills</h3>
                <table class="table table-sm" style="vertical-align: top; width: auto;">
                    <thead>
                        <tr class="table-secondary th-border-top-0">
                            <th>Weapon</th>
                            <th>Kills</th>
                            <th>
                                %
                                <info-hover text="What percent of kills in this session were made with this weapon"></info-hover>
                            </th>
                            <th>
                                HSR (%)
                                <info-hover text="What percent of kills (not shots) were made with a headshot with this weapon"></info-hover>
                            </th>
                            <th>
                                Hip (%)
                                <info-hover text="What percent of kills (not shots) were made while in hipfire with this weapon"></info-hover>
                            </th>
                            <th>
                                ADS (%)
                                <info-hover text="What percent of kills (not shots) were made while aiming down sights with this weapon"></info-hover>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="entry in groupedKillEventsArray">
                            <td>
                                <span v-if="entry[0] == 0">
                                    &lt;no weapon&gt;
                                </span>
                                <a v-else :href="'/i/' + entry[0]">
                                    <span v-if="weaponMap.get(entry[0])">
                                        {{weaponMap.get(entry[0]).name}}
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
                                {{entry[1].length / kills.length * 100 | fixed | locale}}%
                            </td>

                            <td>
                                {{entry[1].filter(iter => iter.event.isHeadshot == true).length}}
                                ({{entry[1].filter(iter => iter.event.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%)
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>
                        </tr>

                        <tr class="table-secondary th-border-top-0">
                            <td>
                                <b>Total</b>
                            </td>

                            <td colspan="2">
                                {{kills.length | locale}}
                            </td>

                            <td colspan="3">
                                {{kills.filter(iter => iter.event.isHeadshot == true).length / kills.length * 100 | fixed | locale}}%
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="flex-grow-1">
                <canvas id="chart-kills-weapon-usage" style="max-height: 300px; max-width: 500px;" class="mb-2"></canvas>
            </div>
        </div>

        <div class="flex-grow-1">
            <h3>Weapon kill types</h3>
            <table class="table table-sm" style="vertical-align: top; width: auto;">
                <thead>
                    <tr class="table-secondary th-border-top-0">
                        <th>Type</th>
                        <th>Kills</th>
                        <th>
                            %
                            <info-hover text="What percent of kills in this session were made with this weapon type"></info-hover>
                        </th>
                        <th>
                            HSR (%)
                            <info-hover text="What percent of kills (not shots) were made with a headshot with this weapon type"></info-hover>
                        </th>
                        <th>
                            Hip (%)
                            <info-hover text="What percent of kills (not shots) were made while in hipfire with this weapon type"></info-hover>
                        </th>
                        <th>
                            ADS (%)
                            <info-hover text="What percent of kills (not shots) were made while aiming down sights with this weapon type"></info-hover>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    <tr v-for="entry in groupedKillEventsTypeArray">
                        <td>
                            <span v-if="entry[0] == 0">
                                &lt;no type&gt;
                            </span>
                            <span v-else-if="weaponCategoryMap.get(entry[0])">
                                {{weaponCategoryMap.get(entry[0]).name}}
                            </span>
                            <span v-else>
                                &lt;missing {{entry[0]}}&gt;
                            </span>
                        </td>

                        <td>
                            {{entry[1].length}}
                        </td>

                        <td>
                            {{entry[1].length / kills.length * 100 | fixed | locale}}%
                        </td>

                        <td>
                            {{entry[1].filter(iter => iter.event.isHeadshot == true).length}}
                            ({{entry[1].filter(iter => iter.event.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%)
                        </td>

                        <td>
                            <span v-if="entry[0] == 0">
                                --
                            </span>
                            <span v-else>
                                {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length}}
                                ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length / entry[1].length * 100 | locale(2)}}%)
                            </span>
                        </td>

                        <td>
                            <span v-if="entry[0] == 0">
                                --
                            </span>
                            <span v-else>
                                {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length}}
                                ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length / entry[1].length * 100 | locale(2)}}%)
                            </span>
                        </td>
                    </tr>

                    <tr class="table-secondary th-border-top-0">
                        <td>
                            <b>Total</b>
                        </td>

                        <td colspan="2">
                            {{kills.length | locale}}
                        </td>

                        <td colspan="3">
                            {{kills.filter(iter => iter.event.isHeadshot == true).length / kills.length * 100 | fixed | locale}}%
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <h3 class="wt-header" style="background-color: var(--red)">
            Deaths
        </h3>

        <div class="d-flex flex-wrap">
            <div class="mr-3">
                <h3>Weapon deaths</h3>
                <table class="table table-sm" style="vertical-align: top; width: auto;">
                    <thead>
                        <tr class="table-secondary th-border-top-0">
                            <th>Weapon</th>
                            <th>Deaths</th>
                            <th>
                                %
                                <info-hover text="What percent of deaths in this session were from this weapon"></info-hover>
                            </th>
                            <th>
                                HSR (%)
                                <info-hover text="What percent of deaths (not shots) were from a headshot with this weapon"></info-hover>
                            </th>
                            <th>
                                Hip (%)
                                <info-hover text="What percent of deaths (not shots) were from hipfire with this weapon"></info-hover>
                            </th>
                            <th>
                                ADS (%)
                                <info-hover text="What percent of deaths (not shots) were from aiming down sights with this weapon"></info-hover>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="entry in groupedDeathEventsArray">
                            <td>
                                <span v-if="entry[0] == 0">
                                    &lt;no weapon&gt;
                                </span>
                                <a v-else :href="'/i/' + entry[0]">
                                    <span v-if="weaponMap.get(entry[0])">
                                        {{weaponMap.get(entry[0]).name}}
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
                                {{entry[1].length / deaths.length * 100 | fixed | locale}}%
                            </td>

                            <td>
                                {{entry[1].filter(iter => iter.event.isHeadshot == true).length}}
                                ({{entry[1].filter(iter => iter.event.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%)
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>
                        </tr>

                        <tr class="table-secondary th-border-top-0">
                            <td>
                                <b>Total</b>
                            </td>

                            <td colspan="2">
                                {{deaths.length | locale}}
                            </td>

                            <td colspan="3">
                                {{deaths.filter(iter => iter.event.isHeadshot == true).length / deaths.length * 100 | fixed | locale}}%
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div>
                <h3>Weapon death types</h3>
                <table class="table table-sm" style="vertical-align: top; width: auto;">
                    <thead>
                        <tr class="table-secondary th-border-top-0">
                            <th>Weapon</th>
                            <th>Deaths</th>
                            <th>
                                %
                                <info-hover text="What percent of deaths in this session were from this weapon"></info-hover>
                            </th>
                            <th>
                                HSR (%)
                                <info-hover text="What percent of deaths (not shots) were from a headshot with this weapon"></info-hover>
                            </th>
                            <th>
                                Hip (%)
                                <info-hover text="What percent of deaths (not shots) were from hipfire with this weapon"></info-hover>
                            </th>
                            <th>
                                ADS (%)
                                <info-hover text="What percent of deaths (not shots) were from aiming down sights with this weapon"></info-hover>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="entry in groupedDeathEventsTypeArray">
                            <td>
                                <span v-if="entry[0] == 0">
                                    &lt;no type&gt;
                                </span>
                                <span v-else-if="weaponCategoryMap.get(entry[0])">
                                    {{weaponCategoryMap.get(entry[0]).name}}
                                </span>
                                <span v-else>
                                    &lt;missing {{entry[0]}}&gt;
                                </span>
                            </td>

                            <td>
                                {{entry[1].length}}
                            </td>

                            <td>
                                {{entry[1].length / deaths.length * 100 | fixed | locale}}%
                            </td>

                            <td>
                                {{entry[1].filter(iter => iter.event.isHeadshot == true).length}}
                                ({{entry[1].filter(iter => iter.event.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%)
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 0).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>

                            <td>
                                <span v-if="entry[0] == 0">
                                    --
                                </span>
                                <span v-else>
                                    {{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length}}
                                    ({{entry[1].filter(iter => iter.fireGroupToFireMode != null && iter.fireGroupToFireMode.fireModeIndex == 1).length / entry[1].length * 100 | locale(2)}}%)
                                </span>
                            </td>
                        </tr>

                        <tr class="table-secondary th-border-top-0">
                            <td>
                                <b>Total</b>
                            </td>

                            <td colspan="2">
                                {{deaths.length | locale}}
                            </td>

                            <td colspan="3">
                                {{deaths.filter(iter => iter.event.isHeadshot == true).length / deaths.length * 100 | fixed | locale}}%
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
    import { Loading, Loadable } from "Loading";

    import { FullKillEvent } from "../common";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { ItemCategory } from "api/ItemCategoryApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import InfoHover from "components/InfoHover.vue";
    import { PsVehicle, VehicleApi } from "api/VehicleApi";

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
            kills: { type: Array as PropType<FullKillEvent[]>, required: true },
            deaths: { type: Array as PropType<FullKillEvent[]>, required: true },
        },

        data: function() {
            return {
                chart: null as Chart | null,

                kpmData: [] as Date[],

                outfitData: [] as OutfitKD[],
                classData: [] as ClassKD[],

                vehicles: Loadable.idle() as Loading<PsVehicle[]>
            }
        },

        mounted: function(): void {
            this.$nextTick(async () => {
                this.generateKillWeaponChart();
                await this.bindVehicles();
            });

            this.kpmData = this.kills.map(iter => iter.event.timestamp);
            this.generateOutfitData();
            this.generateClassData();
        },

        methods: {

            bindVehicles: async function(): Promise<void> {
                this.vehicles = Loadable.loading();
                this.vehicles = await VehicleApi.getAll();
            },

            generateKillWeaponChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const groupedEvents: Map<number, ExpandedKillEvent[]> = this.groupedKillEvents;

                const arr = Array.from(groupedEvents.entries()).sort((a, b) => b[1].length - a[1].length);

                const ctx = (document.getElementById("chart-kills-weapon-usage") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: arr.map((iter) => {
                            const weaponID: number = iter[0];
                            const weaponName: string = `${weaponID == 0 ? "no weapon" : this.weaponMap.get(weaponID)?.name ?? `<missing ${weaponID}>`}`;
                            return `${weaponName} - ${(iter[1].length / this.kills.length * 100).toFixed(2)}%`;
                        }),
                        datasets: [{
                            data: arr.map(iter => iter[1].length),
                            backgroundColor: ColorUtils.randomColors(Math.random(), arr.length)
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: {
                            legend: {
                                position: "right",
                                align: "start",
                                labels: {
                                    color: "#fff",
                                    textAlign: "left"
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
                    if (ev.event.killedLoadoutID == 0) {
                        continue;
                    }

                    const name: string = LoadoutUtils.getLoadoutName(ev.event.killedLoadoutID);
                    const clazz: ClassKD = getClass(name);

                    ++clazz.kills;
                    if (ev.event.isHeadshot == true) {
                        ++clazz.killHeadshots;
                    }
                }

                for (const ev of this.deaths) {
                    if (ev.event.attackerLoadoutID == 0) {
                        continue;
                    }

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

        // i am aware this code is shit, oh well
        computed: {
            groupedKillEvents: function(): Map<number, ExpandedKillEvent[]> {
                return this.kills.reduce(
                    (entryMap: Map<number, ExpandedKillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event]),
                    new Map()
                );
            },

            groupedKillEventsArray: function(): any[] {
                return Array.from(this.groupedKillEvents.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            groupedDeathEvents: function(): Map<number, ExpandedKillEvent[]> {
                return this.deaths.reduce(
                    (entryMap: Map<number, ExpandedKillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event]),
                    new Map()
                );
            },

            groupedDeathEventsArray: function(): any[] {
                return Array.from(this.groupedDeathEvents.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            groupedKillEventsType: function(): Map<number, FullKillEvent[]> {
                return this.kills.reduce(
                    (entryMap: Map<number, FullKillEvent[]>, event: FullKillEvent) => {
                        const itemCatID: number = event.itemCategory?.id ?? 0;
                        return entryMap.set(itemCatID, [...entryMap.get(itemCatID) || [], event]);
                    },
                    new Map()
                );
            },

            groupedKillEventsTypeArray: function(): any[] {
                return Array.from(this.groupedKillEventsType.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            groupedDeathEventsType: function(): Map<number, FullKillEvent[]> {
                return this.deaths.reduce(
                    (entryMap: Map<number, FullKillEvent[]>, event: FullKillEvent) => {
                        const itemCatID: number = event.itemCategory?.id ?? -1;
                        return entryMap.set(itemCatID, [...entryMap.get(itemCatID) || [], event]);
                    },
                    new Map()
                );
            },

            groupedDeathEventsTypeArray: function(): any[] {
                return Array.from(this.groupedDeathEventsType.entries()).sort((a, b) => b[1].length - a[1].length);
            },

            weaponMap: function(): Map<number, PsItem> {
                const map: Map<number, PsItem> = new Map();
                for (const iter of [...this.kills, ...this.deaths]) {
                    if (map.has(iter.event.weaponID) == false && iter.item != null) {
                        map.set(iter.event.weaponID, iter.item);
                    }
                }

                return map;
            },

            weaponCategoryMap: function(): Map<number, ItemCategory> {
                const map: Map<number, ItemCategory> = new Map();
                for (const iter of [...this.kills, ...this.deaths]) {
                    if (iter.itemCategory == null) {
                        continue;
                    }

                    if (map.has(iter.itemCategory.id) == true) {
                        continue;
                    }

                    map.set(iter.itemCategory.id, iter.itemCategory);
                }

                return map;
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