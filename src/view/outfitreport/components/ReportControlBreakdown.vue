<template>
    <collapsible header-text="Capture/Defenses participated in">
        <table class="table table-sm" style="table-layout: fixed;">
            <tr class="table-secondary th-border-top-0">
                <th width="14%">Facility</th>
                <th width="14%">Timestamp</th>
                <th width="14%">Action</th>
                <th width="14%">Outfit</th>
                <th width="14%">Players</th>
                <th width="30%"></th>
            </tr>

            <tr v-if="entries.length == 0">
                <td colspan="6">
                    No data
                </td>
            </tr>

            <tr v-for="control in entries">
                <td>
                    <div v-if="control.facility != null">
                        <h4>
                            {{control.facility.name}}
                        </h4>
                        {{control.facility.typeName}} on {{control.facility.zoneID | zone}}
                    </div>
                    <div v-else>
                        &lt;missing facility {{control.control.facility_id}}&gt;
                    </div>
                </td>

                <td>
                    {{control.control.timestamp | moment}}
                </td>

                <td>
                    <span v-if="control.control.oldFactionID == control.control.newFactionID">
                        Defended
                    </span>
                    <span v-else>
                        Captured from {{control.control.oldFactionID | faction}}
                    </span>
                </td>

                <td>
                    <span v-if="control.control.oldFactionID != control.control.newFactionID">
                        <span v-if="control.outfit == null">
                            &lt;no outfit&gt;
                        </span>

                        <span v-else>
                            <a :href="'/o/' + control.outfit.id">
                                <span v-if="control.outfit.tag != null">
                                    [{{control.outfit.tag}}]
                                </span>
                                {{control.outfit.name}}
                            </a>
                        </span>
                    </span>

                    <span v-else>
                        --
                    </span>
                </td>

                <td>
                    {{control.players.length}}
                </td>

                <td>
                    <div>
                        <chart-block-pie-chart :data="control.block" :show-percent="true" style="max-height: 200px;"></chart-block-pie-chart>
                    </div>
                </td>
            </tr>
        </table>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import { PlayerControlEvent } from "api/PlayerControlEventApi";
    import { FacilityControlEvent } from "api/FacilityControlEventApi";
    import { PsFacility } from "api/MapApi";
    import { PsOutfit } from "api/OutfitApi";
    import { PsCharacter } from "api/CharacterApi";

    import { Block, BlockEntry } from "./charts/common";
    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";
    import Collapsible from "components/Collapsible.vue";

    import "filters/FactionNameFilter";
    import "filters/ZoneNameFilter";
    import "MomentFilter";

    class ControlEntry {
        public id: number = 0;
        public players: PlayerControlEvent[] = [];
        public control: FacilityControlEvent = new FacilityControlEvent();
        public facility: PsFacility | null = null;
        public block: Block = new Block();
        public outfit: PsOutfit | null = null;
    }

    export const ReportControlBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                entries: [] as ControlEntry[],

                shown: true as boolean,

                ourOutfits: [] as string[],

                showCaptures: true as boolean,
                showDefenses: false as boolean,
                showAllOutfits: false as boolean
            }
        },

        mounted: function(): void {
            this.updateOurOutfits(false);
            this.make();
        },

        methods: {
            updateOurOutfits: function(b: boolean): void {
                this.showAllOutfits = b;

                for (const player of this.report.players) {
                    const character: PsCharacter | null = this.report.characters.get(player) || null;
                    if (character == null) {
                        continue;
                    }

                    if (character.outfitID == null) {
                        continue;
                    }

                    if (this.ourOutfits.indexOf(character.outfitID) > -1) {
                        continue;
                    }

                    this.ourOutfits.push(character.outfitID);
                }
            },

            setShows: function(cap: boolean, def: boolean): void {
                this.showCaptures = cap;
                this.showDefenses = def;
                this.make();
            },

            make: function(): void {
                this.entries = [];

                for (const control of this.report.control) {
                    // Don't show captures
                    if (this.showCaptures == false && control.oldFactionID != control.newFactionID) {
                        continue;
                    }

                    // Don't show captures that aren't from an outfit the report was generated on
                    if (control.oldFactionID != control.newFactionID && this.showAllOutfits == false && this.ourOutfits.indexOf(control.outfitID ?? "") == -1) {
                        continue;
                    }

                    // Don't show captures
                    if (this.showDefenses == false && control.oldFactionID == control.newFactionID) {
                        continue;
                    }

                    const entry: ControlEntry = new ControlEntry();
                    entry.id = control.id;
                    entry.control = control;
                    entry.players = this.report.playerControl.filter(iter => iter.controlID == control.id);
                    entry.facility = this.report.facilities.get(control.facilityID) || null;
                    entry.outfit = (control.outfitID != null) ? this.report.outfits.get(control.outfitID) || null : null;

                    const map: Map<string, BlockEntry> = new Map();
                    const zero: BlockEntry = new BlockEntry();
                    zero.name = "<no outfit>";
                    map.set("0", zero);

                    for (const pev of entry.players) {
                        const outfitID: string = pev.outfitID || "0";

                        if (map.has(outfitID) == false) {
                            const outfit: PsOutfit | null = this.report.outfits.get(outfitID) || null;
                            const entry: BlockEntry = new BlockEntry();
                            entry.name = outfit != null ? `${outfit.tag != null ? `[${outfit.tag}] ` : ""}${outfit.name}` : `<missing ${outfitID}>`;

                            map.set(outfitID, entry);
                        }

                        const entry: BlockEntry = map.get(outfitID)!;
                        ++entry.count;
                        map.set(outfitID, entry);
                    }

                    entry.block = new Block();
                    entry.block.total = entry.players.length;
                    entry.block.entries = Array.from(map.values()).sort((a, b) => b.count - a.count);

                    this.entries.push(entry);
                }

                this.entries.sort((a, b) => a.control.timestamp.getTime() - b.control.timestamp.getTime());
            }

        },

        components: {
            ChartBlockPieChart,
            Collapsible
        }
    });

    export default ReportControlBreakdown;
</script>