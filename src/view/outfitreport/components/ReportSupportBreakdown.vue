<template>
    <div>
        <h2 class="wt-header" data-toggle="collapse" data-target="#report-support-breakdown">
            Support breakdown
        </h2>

        <div id="report-support-breakdown" class="collapse show">
            <div class="d-flex">
                <div class="flex-grow-1 flex-basis-0">
                    <h4>Heals</h4>
                </div>
                <div class="flex-grow-1 flex-basis-0">
                    <h4>Revives</h4>
                </div>
            </div>
            <div class="d-flex">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-list :data="heals" left-title="Outfits" right-title="Healed"></chart-block-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-pie-chart :data="heals"
                        :show-total="true" :show-percent="true">
                    </chart-block-pie-chart>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-list :data="revives" left-title="Outfits" right-title="Revives"></chart-block-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-pie-chart :data="revives"
                        :show-total="true" :show-percent="true">
                    </chart-block-pie-chart>
                </div>
            </div>

            <div class="d-flex">
                <div class="flex-grow-1 flex-basis-0">
                    <h4>Resupplies</h4>
                </div>
                <div class="flex-grow-1 flex-basis-0">
                    <h4>Repairs</h4>
                </div>
            </div>
            <div class="d-flex">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-list :data="resupplies" left-title="Outfits" right-title="Resupplied"></chart-block-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-block-pie-chart :data="resupplies"
                        :show-total="true" :show-percent="true">
                    </chart-block-pie-chart>
                </div>

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
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import { PsCharacter } from "api/CharacterApi";
    import { PsOutfit } from "api/OutfitApi";
    import { Experience } from "api/ExpStatApi";

    import { Block, BlockEntry } from "./charts/common";
    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";
    import ChartBlockList from "./charts/ChartBlockList.vue";

    export const ReportSupportBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                heals: new Block() as Block,
                revives: new Block() as Block,

                resupplies: new Block() as Block,
                repairs: new Block() as Block,

                assists: new Block() as Block
            }
        },

        mounted: function(): void {
            this.makeAll();
        },

        methods: {

            makeAll: function(): void {
                this.heals = this.makeBlock([ Experience.HEAL, Experience.SQUAD_HEAL ]);
                this.revives = this.makeBlock([Experience.REVIVE, Experience.SQUAD_REVIVE]);
                this.resupplies = this.makeBlock([Experience.RESUPPLY, Experience.SQUAD_RESUPPLY]);
                this.repairs = this.makeBlock([Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR]);
                this.assists = this.makeBlock([Experience.ASSIST]);
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
            ChartBlockList
        }
    });

    export default ReportSupportBreakdown;
</script>