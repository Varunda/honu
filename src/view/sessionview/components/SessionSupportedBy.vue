<template>
    <div>
        <div v-if="heals.length > 0" class="bottom-border">
            <h3>Heals</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerHeals"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerHeals"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitHeals"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitHeals"></chart-entry-list>
                </div>
            </div>
        </div>

        <div v-if="revives.length > 0" class="bottom-border">
            <h3>Revives</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerRevives"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerRevives"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitRevives"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitRevives"></chart-entry-list>
                </div>
            </div>
        </div>

        <div v-if="resupplies.length > 0" class="bottom-border">
            <h3>Resupplies</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerResupplies"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerResupplies"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitResupplies"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitResupplies"></chart-entry-list>
                </div>
            </div>
        </div>

        <div v-if="repairs.length > 0" class="bottom-border">
            <h3>MAX Repairs</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerRepairs"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitRepairs"></chart-entry-list>
                </div>
            </div>
        </div>

        <div v-if="shieldRepairs.length > 0" class="bottom-border">
            <h3>Shield Repairs</h3>

            <div class="d-flex flex-wrap mw-100">
                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="playerShieldRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="playerShieldRepairs"></chart-entry-list>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-pie :data="outfitShieldRepairs"></chart-entry-pie>
                </div>

                <div class="flex-grow-1 flex-basis-0">
                    <chart-entry-list :data="outfitShieldRepairs"></chart-entry-list>
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import ChartEntryPie from "./ChartEntryPie.vue";
    import ChartEntryList from "./ChartEntryList.vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import { Session } from "api/SessionApi";
    import { ExpandedExpEvent, Experience, ExperienceBlock, ExpEvent } from "api/ExpStatApi";
    import { PsCharacter } from "api/CharacterApi";

    interface Entry {
        display: string;
        count: number;
        link: string | null;
    }

    export const SessionSupportedBy = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Object as PropType<ExperienceBlock>, required: true },
            FullExp: { type: Boolean, required: true }
        },

        data: function() {
            return {
                playerHeals: [] as Entry[],
                outfitHeals: [] as Entry[],

                playerRevives: [] as Entry[],
                outfitRevives: [] as Entry[],

                playerResupplies: [] as Entry[],
                outfitResupplies: [] as Entry[],

                playerRepairs: [] as Entry[],
                outfitRepairs: [] as Entry[],

                playerShieldRepairs: [] as Entry[],
                outfitShieldRepairs: [] as Entry[],
            }
        },

        mounted: function(): void {
            this.generateAll();
        },

        methods: {
            generateAll: function(): void {
                this.generateHealEntries();
                this.generateReviveEntries();
                this.generateResupplyEntries();
                this.generateRepairEntries();
                this.generateShieldRepairEntries();
            },

            generateHealEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.heals);
                this.playerHeals = set[0];
                this.outfitHeals = set[1];
            },

            generateReviveEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.revives);
                this.playerRevives = set[0];
                this.outfitRevives = set[1];
            },

            generateResupplyEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.resupplies);
                this.playerResupplies = set[0];
                this.outfitResupplies = set[1];
            },

            generateRepairEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.repairs);
                this.playerRepairs = set[0];
                this.outfitRepairs = set[1];
            },

            generateShieldRepairEntries: function(): void {
                const set = this.generatePlayerAndOutfitExp(this.shieldRepairs);
                this.playerShieldRepairs = set[0];
                this.outfitShieldRepairs = set[1];
            },

            generatePlayerAndOutfitExp(events: ExpEvent[]): [Entry[], Entry[]] {
                const charMap: Map<string, Entry> = new Map();
                const outfitMap: Map<string, Entry> = new Map();

                for (const ev of events) {
                    const c: PsCharacter | null = this.exp.characters.find(iter => iter.id == ev.sourceID) || null;

                    if (charMap.has(ev.sourceID) == false) {
                        charMap.set(ev.sourceID, {
                            display: c == null ? `<missing ${ev.sourceID}>` : `${(c.outfitID ? `[${c.outfitTag}] ` : "")}${c.name}`,
                            count: 0,
                            link: `/c/${ev.sourceID}`
                        });
                    }
                    ++charMap.get(ev.sourceID)!.count;

                    if (c != null) {
                        if (outfitMap.has(c.outfitID ?? "0") == false) {
                            let entry: Entry = {
                                display: "<no outfit>",
                                count: 0,
                                link: (c.outfitID == null) ? null : `/o/${c.outfitID}`
                            };

                            if (c.outfitID != null) {
                                entry.display = `[${c.outfitTag}] ${c.outfitName}`;
                            }
                            outfitMap.set(c.outfitID ?? "0", entry);
                        }

                        ++outfitMap.get(c.outfitID ?? "0")!.count;
                    }
                }

                const player = Array.from(charMap.values()).sort((a, b) => b.count - a.count);
                const outfit = Array.from(outfitMap.values()).sort((a, b) => b.count - a.count);

                return [player, outfit];
            }

        },

        computed: {
            heals: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isHeal(iter.experienceID));
            },

            revives: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isRevive(iter.experienceID));
            },

            resupplies: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isResupply(iter.experienceID));
            },

            repairs: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isMaxRepair(iter.experienceID));
            },

            shieldRepairs: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isShieldRepair(iter.experienceID));
            },

            spawns: function(): ExpEvent[] {
                return this.exp.events.filter(iter => Experience.isSpawn(iter.experienceID));
            }

        },

        components: {
            ChartTimestamp, ChartEntryPie, ChartEntryList,
            ATable, ACol, ABody, AFilter, AHeader,
            ToggleButton
        }
    });
    export default SessionSupportedBy;
</script>
