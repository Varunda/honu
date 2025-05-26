<template>
    <div>
        <h3 class="wt-header" style="background-color: var(--purple)">
            Versus
        </h3>

        <div class="d-flex">
            <div class="mr-4">
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
                                <span v-if="entry.outfitID.startsWith('0') == true">
                                    {{entry.outfitName}}
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
                                Killed
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

        <div>
            <h3 class="wt-header" style="background-color: var(--green)">
                Kills
            </h3>

            <div class="d-flex flex-wrap justify-content-center">
                <div class="flex-grow-1 flex-basis-0 mr-3">
                    <h3>Weapon kills</h3>

                    <session-view-kills-item-table :entries="uses.kill" :show-category="true" :total="kills.length">
                    </session-view-kills-item-table>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <canvas id="chart-kills-weapon-usage" style="max-height: 300px; width: 500px; max-width: 500px;" class="mx-auto mb-2"></canvas>
                </div>
            </div>

            <div class="d-flex">
                <div class="flex-grow-1 flex-basis-0">
                    <h3>Weapon kill types</h3>

                    <session-view-kills-item-table :entries="uses.killType" :show-category="false" :total="kills.length">
                    </session-view-kills-item-table>
                </div>

                <div class="flex-grow-1"></div>
            </div>
        </div>

        <div>

        <h3 class="wt-header" style="background-color: var(--red)">
            Deaths
        </h3>
            <div class="d-flex flex-wrap">
                <div class="flex-grow-1 flex-basis-0 mr-3">
                    <h3>Weapon deaths</h3>

                    <session-view-kills-item-table :entries="uses.death" :show-category="true" :total="deaths.length">
                    </session-view-kills-item-table>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <h3>Weapon death types</h3>

                    <session-view-kills-item-table :entries="uses.deathType" :show-category="false" :total="deaths.length">
                    </session-view-kills-item-table>
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import { FullKillEvent, FullVehicleDestroyEvent } from "../common";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";
    import FactionUtils from "util/Faction";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { ItemCategory } from "api/ItemCategoryApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";
    import { PsVehicle, VehicleApi } from "api/VehicleApi";
    import { ExpandedVehicleDestroyEvent } from "api/VehicleDestroyEventApi";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import SessionViewKillsItemTable from "./SessionViewKillsItemTable.vue";
    import InfoHover from "components/InfoHover.vue";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

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

    class WeaponUses {
        public id: number = 0;
        public name: string = "";
        public categoryID: number = 0;
        public categoryName: string = "";
        public uses: number = 0;
        public headshot: number = 0;
        public hip: number = 0;
        public ads: number = 0;
        public vehicleUses: number = 0;
    }

    export const SessionViewerKills = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Array as PropType<FullKillEvent[]>, required: true },
            deaths: { type: Array as PropType<FullKillEvent[]>, required: true },
            vehicleKills: { type: Array as PropType<FullVehicleDestroyEvent[]>, required: true },
            vehicleDeaths: { type: Array as PropType<FullVehicleDestroyEvent[]>, required: true },
        },

        data: function() {
            return {
                chart: null as Chart | null,

                kpmData: [] as Date[],

                outfitData: [] as OutfitKD[],
                classData: [] as ClassKD[],

                uses: {
                    kill: Loadable.idle() as Loading<WeaponUses[]>,
                    killType: Loadable.idle() as Loading<WeaponUses[]>,
                    death: Loadable.idle() as Loading<WeaponUses[]>,
                    deathType: Loadable.idle() as Loading<WeaponUses[]>,
                },

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
            this.generateAllWeaponUses();
        },

        methods: {

            bindVehicles: async function(): Promise<void> {
                this.vehicles = Loadable.loading();
                this.vehicles = await VehicleApi.getAll();
            },

            generateAllWeaponUses: function(): void {
                this.uses.kill = Loadable.loaded(this.generateWeaponUses({ weapon: this.kills, vehicle: this.vehicleKills }, "item"));
                this.uses.killType = Loadable.loaded(this.generateWeaponUses({ weapon: this.kills, vehicle: this.vehicleKills }, "category"));
                this.uses.death = Loadable.loaded(this.generateWeaponUses({ weapon: this.deaths, vehicle: this.vehicleDeaths }, "item"));
                this.uses.deathType = Loadable.loaded(this.generateWeaponUses({ weapon: this.deaths, vehicle: this.vehicleDeaths }, "category"));
            },

            generateWeaponUses: function(events: { weapon: FullKillEvent[], vehicle: FullVehicleDestroyEvent[] }, selector: "item" | "category"): WeaponUses[] {

                const map: Map<number, WeaponUses> = new Map();
                for (const kill of events.weapon) {
                    let entryID: number;

                    if (selector == "item") {
                        entryID = kill.event.weaponID;
                    } else if (selector == "category") {
                        entryID = kill.itemCategory?.id ?? -1;
                    } else {
                        throw `unchecked selector ${selector}`;
                    }

                    let uses: WeaponUses | undefined = map.get(entryID);

                    if (uses == undefined) {
                        uses = new WeaponUses();
                        uses.id = entryID;

                        if (kill.event.weaponID == 0) {
                            uses.name = "no weapon";
                            uses.categoryID = -1;
                            uses.categoryName = "no weapon";
                        } else {
                            if (selector == "item") {
                                uses.name = kill.item?.name ?? `<missing ${entryID}>`;
                            } else if (selector == "category") {
                                uses.name = kill.itemCategory?.name ?? `$<missing ${kill.item?.categoryID ?? -1}>`;
                            }
                            // this info is redundant for the category lists, but whatever lol
                            uses.categoryID = kill.item?.categoryID ?? -1;
                            uses.categoryName = kill.itemCategory?.name ?? `<missing ${kill.item?.categoryID ?? -1}>`;
                        }
                    }

                    ++uses.uses;

                    if (kill.event.isHeadshot == true) {
                        ++uses.headshot;
                    }

                    if (kill.fireGroupToFireMode?.fireModeIndex == 0) {
                        ++uses.hip;
                    } else if (kill.fireGroupToFireMode?.fireModeIndex == 1) {
                        ++uses.ads;
                    } else if (kill.fireGroupToFireMode == null) {
                        console.warn(`missing fireGroupToFireMode for ${kill.event.attackerFireModeID}`);
                    }

                    map.set(entryID, uses);
                }

                for (const vkill of events.vehicle) {
                    let entryID: number;

                    if (selector == "item") {
                        entryID = vkill.event.attackerWeaponID;
                    } else if (selector == "category") {
                        entryID = vkill.item?.categoryID ?? -1;
                    } else {
                        throw `unchecked selector ${selector}`;
                    }

                    let uses: WeaponUses | undefined = map.get(entryID);

                    if (uses == undefined) {
                        uses = new WeaponUses();
                        uses.id = entryID;

                        if (vkill.event.attackerWeaponID == 0) {
                            uses.name = "no weapon";
                            uses.categoryID = -1;
                            uses.categoryName = "no weapon";
                        } else {
                            if (selector == "item") {
                                uses.name = vkill.item?.name ?? `<missing ${entryID}>`;
                            } else if (selector == "category") {
                                uses.name = vkill.itemCategory?.name ?? `$<missing ${vkill.item?.categoryID ?? -1}>`;
                            }
                            // this info is redundant for the category lists, but whatever lol
                            uses.categoryID = vkill.item?.categoryID ?? -1;
                            uses.categoryName = vkill.itemCategory?.name ?? `<missing ${vkill.item?.categoryID ?? -1}>`;
                        }
                    }

                    ++uses.vehicleUses;

                    map.set(entryID, uses);
                }

                return Array.from(map.values());
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
                                labels: {
                                    color: "#fff",
                                    textAlign: "left",
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
                    let outfit: OutfitKD | null = outfitMap.get(character.outfitID ?? `0-${character.factionID}`) || null;
                    if (outfit == null) {
                        if (character.outfitID == null) {
                            outfit = {
                                outfitID: `0-${character.factionID}`,
                                outfitTag: null,
                                outfitName: `<no outfit ${FactionUtils.getName(character.factionID)}>`,
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

            groupedKillEvents: function(): Map<number, ExpandedKillEvent[]> {
                return this.kills.reduce(
                    (entryMap: Map<number, ExpandedKillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event]),
                    new Map()
                );
            },

            /*

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
            */
        },

        components: {
            ChartTimestamp,
            InfoHover,
            ATable, ACol, ABody, AFilter, AHeader,
            SessionViewKillsItemTable
        }

    });
    export default SessionViewerKills;
</script>