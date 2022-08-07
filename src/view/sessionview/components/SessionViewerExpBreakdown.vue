<template>
    <div>
        <!--
        <collapsible header-text="Full breakdown" @click.stop>
        </collapsible>
        -->

        <div class="row">
            <div class="col-12 col-lg-6">
                <h3>Experience ticks earned</h3>
                <div class="row">
                    <div class="col-12 col-lg-6">
                        <chart-entry-pie :data="typeCount"></chart-entry-pie>
                    </div>
                    <div class="col-12 col-lg-6">
                        <chart-entry-list :data="typeCount" left-column-title="Exp type" middle-column-title="# earned"></chart-entry-list>
                    </div>
                </div>
            </div>

            <div class="col-12 col-lg-6">
                <h3>Exp gained</h3>
                <div class="row">
                    <div class="col-12 col-lg-6">
                        <chart-entry-pie :data="typeAmount"></chart-entry-pie>
                    </div>
                    <div class="col-12 col-lg-6">
                        <chart-entry-list :data="typeAmount" left-column-title="Exp type" middle-column-title="XP earned"></chart-entry-list>
                    </div>
                </div>
            </div>
        </div>

        <div>
            <a @click="showAllEvents = !showAllEvents" href="javascript:void(0);">
                (debug) show all events
            </a>

            <table v-if="showAllEvents" class="table table-sm">
                <thead>
                    <tr class="table-secondary">
                        <td>timestamp</td>
                        <th>source</th>
                        <th>other</th>
                        <th>exp type</th>
                        <th>exp id</th>
                        <th>amount</th>
                        <th>base amt</th>
                        <th>diff</th>
                    </tr>
                </thead>

                <tbody>
                    <tr v-for="ev in full">
                        <td>
                            {{ev.event.timestamp | moment}}
                        </td>

                        <td>
                            <a :href="'/c/' + ev.event.sourceID">
                                <span v-if="ev.source != null">
                                    {{ev.source | characterName}}
                                </span>
                                <span v-else>
                                    missing {{ev.event.sourceID}}
                                </span>
                            </a>
                        </td>

                        <td>
                            <a v-if="ev.event.otherID.length == 19" :href="'/c/' + ev.event.otherID">
                                <span v-if="ev.other != null">
                                    {{ev.other | characterName}}
                                </span>
                                <span v-else>
                                    missing {{ev.event.otherID}}
                                </span>
                            </a>
                            <span v-else :class="{ 'text-info': ev.event.otherID == selectedNpcId }" @click="selectedNpcId = ev.event.otherID;">
                                {{ev.event.otherID}} (NPC ID)
                            </span>
                        </td>

                        <td>
                            <span v-if="ev.type != null">
                                {{ev.type.name}}
                            </span>
                            <span v-else class="text-danger">
                                missing {{ev.event.experienceID}}
                            </span>
                        </td>

                        <td>{{ev.event.experienceID}}</td>

                        <td>
                            {{ev.event.amount}}
                        </td>

                        <td>
                            <span v-if="ev.type != null">
                                {{ev.type.amount}}
                            </span>
                            <span v-else>
                                --
                            </span>
                        </td>

                        <td>
                            <span v-if="ev.type != null">
                                {{ev.event.amount / Math.max(1, ev.type.amount) | locale(2)}}
                            </span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import "MomentFilter";
    import "filters/CharacterName";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import ChartEntryPie from "./ChartEntryPie.vue";
    import ChartEntryList from "./ChartEntryList.vue";

    import Collapsible from "components/Collapsible.vue";

    import { Session } from "api/SessionApi";
    import { ExpandedExpEvent, Experience, ExperienceBlock, ExperienceType, ExpEvent } from "api/ExpStatApi";
    import { PsCharacter } from "api/CharacterApi";

    type FullEvent = {
        event: ExpEvent,
        source: PsCharacter | null,
        other: PsCharacter | null,
        type: ExperienceType | null
    };

    interface Entry {
        display: string;
        count: number;
        link: string | null;
    }

    export const SessionViewerExpBreakdown = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Object as PropType<ExperienceBlock>, required: true },
            FullExp: { type: Boolean, required: true }
        },

        data: function() {
            return {
                full: [] as FullEvent[],

                showAllEvents: false as boolean,

                selectedNpcId: null as number | null,

                typeCount: [] as Entry[],
                typeAmount: [] as Entry[],
            }
        },

        mounted: function(): void {
            this.makeFull();
            this.makeTypeBreakdown();
        },

        methods: {
            makeFull: function(): void {
                for (const ev of this.exp.events) {
                    const f: FullEvent = {
                        event: ev,
                        type: this.exp.experienceTypes.find(iter => iter.id == ev.experienceID) || null,
                        source: this.exp.characters.find(iter => iter.id == ev.sourceID) || null,
                        other: ev.otherID.length == 19 ? this.exp.characters.find(iter => iter.id == ev.otherID) || null : null
                    };

                    this.full.push(f);
                }
            },

            makeTypeBreakdown: function(): void {
                this.typeCount = [];
                this.typeAmount = [];

                const count: Map<number, number> = new Map();
                const amount: Map<number, number> = new Map();

                for (const ev of this.exp.events) {
                    count.set(ev.experienceID, (count.get(ev.experienceID) || 0) + 1);
                    amount.set(ev.experienceID, (amount.get(ev.experienceID) || 0) + ev.amount);
                }

                for (const kvp of count) {
                    this.typeCount.push({
                        display: this.exp.experienceTypes.find(iter => iter.id == kvp[0])?.name ?? `unknown type ${kvp[0]}`,
                        count: kvp[1],
                        link: null
                    });
                }
                this.typeCount.sort((a, b) => b.count - a.count);

                for (const kvp of amount) {
                    this.typeAmount.push({
                        display: this.exp.experienceTypes.find(iter => iter.id == kvp[0])?.name ?? `unknown type ${kvp[0]}`,
                        count: kvp[1],
                        link: null
                    });
                }
                this.typeAmount.sort((a, b) => b.count - a.count);
            }
        },

        computed: {

        },

        components: {
            ChartTimestamp,
            ChartEntryPie,
            ChartEntryList,
            Collapsible
        }
    });
    export default SessionViewerExpBreakdown;

</script>