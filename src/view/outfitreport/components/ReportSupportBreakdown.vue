<template>
    <collapsible header-text="Support breakdown">

        <div class="row">
            <div class="col-12 col-xl-6">
                <h4>
                    Heals
                    <info-hover text="What outfits were healed by the tracked characters"></info-hover>
                </h4>

                <div class="d-flex">
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-list :data="heals" left-title="Outfits" right-title="Healed"></chart-block-list>
                    </div>

                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-pie-chart :data="heals"
                            :show-total="true" :show-percent="true">
                        </chart-block-pie-chart>
                    </div>
                </div>
            </div>

            <div class="col-12 col-xl-6">
                <h4>
                    Revives
                    <info-hover text="What outfits were revived by the tracked characters. The total is how many total revives the tracked players got"></info-hover>
                </h4>

                <div class="d-flex">
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-list :data="revives" left-title="Outfits" right-title="Revives"></chart-block-list>
                    </div>

                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-pie-chart :data="revives"
                            :show-total="true" :show-percent="true">
                        </chart-block-pie-chart>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-12 col-xl-6">
                <h4>
                    Resupplies
                    <info-hover text="What outfits were resupplied by the tracked characters"></info-hover>
                </h4>

                <div class="d-flex">
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-list :data="resupplies" left-title="Outfits" right-title="Resupplied"></chart-block-list>
                    </div>

                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-pie-chart :data="resupplies"
                            :show-total="true" :show-percent="true">
                        </chart-block-pie-chart>
                    </div>
                </div>
            </div>

            <div class="col-12 col-xl-6">
                <h4>
                    MAX repairs
                    <info-hover text="What outfits were repaired by the tracked characters"></info-hover>
                </h4>

                <div class="d-flex">
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-list :data="repairs" left-title="Outfits" right-title="MAX repairs"></chart-block-list>
                    </div>

                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-pie-chart :data="repairs"
                            :show-total="true" :show-percent="true">
                        </chart-block-pie-chart>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-12 col-xl-6">
                <h4>
                    Vehicle repairs
                    <info-hover text="What vehicles were repaired by the tracked characters"></info-hover>
                </h4>

                <div class="d-flex">
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-list :data="vehicleRepairs" left-title="Vehicle" right-title="Repair ticks"></chart-block-list>
                    </div>
                    <div class="flex-grow-1 flex-basis-0">
                        <chart-block-pie-chart :data="vehicleRepairs" :show-total="true" :show-percent="true"></chart-block-pie-chart>
                    </div>
                </div>
            </div>
        </div>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata, ReportParameters } from "../Report";

    import { PsCharacter } from "api/CharacterApi";
    import { PsOutfit } from "api/OutfitApi";
    import { Experience } from "api/ExpStatApi";

    import InfoHover from "components/InfoHover.vue";

    import { Block, BlockEntry } from "./charts/common";
    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";
    import ChartBlockList from "./charts/ChartBlockList.vue";
    import Collapsible from "components/Collapsible.vue";

    export const ReportSupportBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                heals: new Block() as Block,
                revives: new Block() as Block,

                resupplies: new Block() as Block,
                repairs: new Block() as Block,

                assists: new Block() as Block,
                vehicleRepairs: new Block() as Block
            }
        },

        mounted: function(): void {
            this.makeAll();
        },

        methods: {

            makeAll: function(): void {
                this.heals = this.makeBlock([Experience.HEAL, Experience.SQUAD_HEAL]);
                this.revives = this.makeBlock([Experience.REVIVE, Experience.SQUAD_REVIVE]);
                this.resupplies = this.makeBlock([Experience.RESUPPLY, Experience.SQUAD_RESUPPLY]);
                this.repairs = this.makeBlock([Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR]);
                this.assists = this.makeBlock([Experience.ASSIST]);

                this.makeVehicleRepair();
            },

            makeVehicleRepair: function(): void {
                this.vehicleRepairs = new Block();

                const map: Map<string, BlockEntry> = new Map();

                for (const exp of this.report.experience) {
                    let vehicleName: string = "";

                    switch (exp.experienceID) {
                        case Experience.REPAIR_FLASH: case Experience.SQUAD_REPAIR_FLASH: vehicleName = "Flash"; break;
                        case Experience.REPAIR_GALAXY: case Experience.SQUAD_REPAIR_GALAXY: vehicleName = "Galaxy"; break;
                        case Experience.REPAIR_LIBERATOR: case Experience.SQUAD_REPAIR_LIBERATOR: vehicleName = "Liberator"; break;
                        case Experience.REPAIR_LIGHTNING: case Experience.SQUAD_REPAIR_LIGHTNING: vehicleName = "Lightning"; break;
                        case Experience.REPAIR_MAGRIDER: case Experience.SQUAD_REPAIR_MAGRIDER: vehicleName = "Magrider"; break;
                        case Experience.REPAIR_MOSQUITO: case Experience.SQUAD_REPAIR_MOSQUITO: vehicleName = "Mosquito"; break;
                        case Experience.REPAIR_PROWLER: case Experience.SQUAD_REPAIR_PROWLER: vehicleName = "Prowler"; break;
                        case Experience.REPAIR_REAVER: case Experience.SQUAD_REPAIR_REAVER: vehicleName = "Reaver"; break;
                        case Experience.REPAIR_SCYTHE: case Experience.SQUAD_REPAIR_SCYTHE: vehicleName = "Scythe"; break;
                        case Experience.REPAIR_SUNDERER: case Experience.SQUAD_REPAIR_SUNDERER: vehicleName = "Sunderer"; break;
                        case Experience.REPAIR_VANGUARD: case Experience.SQUAD_REPAIR_VANGUARD: vehicleName = "Vanguard"; break;
                        case Experience.REPAIR_HARASSER: case Experience.SQUAD_REPAIR_HARASSER: vehicleName = "Harasser"; break;
                        case Experience.REPAIR_VALKYRIE: case Experience.SQUAD_REPAIR_VALKYRIE: vehicleName = "Valkyrie"; break;
                        case Experience.REPAIR_ANT: case Experience.SQUAD_REPAIR_ANT: vehicleName = "ANT"; break;
                        case Experience.REPAIR_COLOSSUS: case Experience.SQUAD_REPAIR_COLOSSUS: vehicleName = "Colossus"; break;
                        case Experience.REPAIR_JAVELIN: case Experience.SQUAD_REPAIR_JAVELIN: vehicleName = "Javelin"; break;
                        case Experience.REPAIR_CHIMERA: case Experience.SQUAD_REPAIR_CHIMERA: vehicleName = "Chimera"; break;
                        case Experience.REPAIR_DERVISH: case Experience.SQUAD_REPAIR_DERVISH: vehicleName = "Dervish"; break;
                        default:
                            continue
                    }

                    if (vehicleName == "") {
                        continue;
                    }

                    if (map.has(vehicleName) == false) {
                        const entry: BlockEntry = new BlockEntry();
                        entry.name = vehicleName;
                        map.set(vehicleName, entry);
                    }

                    const entry: BlockEntry = map.get(vehicleName)!;
                    ++entry.count;

                    map.set(vehicleName, entry);
                }

                this.vehicleRepairs.entries = Array.from(map.values()).sort((a, b) => b.count - a.count || a.name.localeCompare(b.name));
                this.vehicleRepairs.total = this.vehicleRepairs.entries.reduce((acc, iter) => acc += iter.count, 0);
            },

            makeBlock: function(expIDs: number[]): Block {
                const block: Block = new Block();

                const map: Map<string, BlockEntry> = new Map();

                const noOutfit: BlockEntry = new BlockEntry();
                noOutfit.name = "<no outfit>";

                map.set("0", noOutfit);

                for (const exp of this.report.experience) {
                    if (expIDs.indexOf(exp.experienceID) == -1) {
                        continue;
                    }

                    const char: PsCharacter | null = this.report.characters.get(exp.otherID) || null;
                    if (char == null) {
                        console.warn(`report-support-breakdown> Failed to find ${exp.otherID}`);
                        continue;
                    }

                    const outfitID: string = char.outfitID ?? "0";

                    if (map.has(outfitID) == false) {
                        const entry: BlockEntry = new BlockEntry();
                        const outfit: PsOutfit | null = this.report.outfits.get(outfitID) || null;

                        if (outfit != null) {
                            entry.name = `${outfit.tag ? `[${outfit.tag}]` : ""} ${outfit.name}`;
                        } else {
                            entry.name = `<missing ${outfitID}`;
                        }

                        entry.link = `/o/${outfitID}`;

                        map.set(outfitID, entry);
                    }

                    const entry: BlockEntry = map.get(outfitID)!;
                    ++entry.count;

                    map.set(outfitID, entry);
                }

                block.entries = Array.from(map.values()).sort((a, b) => b.count - a.count || a.name.localeCompare(b.name));
                block.total = block.entries.reduce((acc, iter) => acc += iter.count, 0);

                return block;
            }
        },

        components: {
            ChartBlockPieChart,
            ChartBlockList,
            InfoHover,
            Collapsible
        }
    });

    export default ReportSupportBreakdown;
</script>