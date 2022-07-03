<template>
    <div>
        <div v-if="badStreams.length > 0" class="alert alert-warning text-center h5">
            <div>
                Honu reconnected to the Planetside 2 API due to a bad realtime event stream during this report, this caused:
            </div>

            <ul class="d-inline-block text-left mb-0">
                <li v-for="stream in badStreams">
                    {{stream.secondsMissed | tduration}} of {{stream.streamType}} events to be missed
                </li>
            </ul>
        </div>

        <h2 class="wt-header" data-toggle="collapse" data-target="#report-header">
            Outfit report
            <a class="btn btn-primary" @click.stop="exportJson">Export json</a>
        </h2>

        <div id="report-header" class="collapse show text-center mb-3">
            <div v-if="report.trackedOutfits.length > 0">
                <h3 class="d-inline-block mr-2">
                    Outfits:
                </h3>

                <h3 v-for="outfit in report.trackedOutfits" class="d-inline-block mr-2">
                    <a :href="'/o/' + outfit.id">
                        <span v-if="outfit.tag != null">
                            [{{outfit.tag}}]
                        </span>

                        {{outfit.name}}
                    </a>
                </h3>

                <h3 class="d-inline-block">
                    ({{report.trackedCharacters.length}} characters) 
                </h3>
            </div>

            <div>
                <h4>
                    <span v-if="report.periodStart.getDate() != report.periodEnd.getDate()">
                        From {{report.periodStart | moment}} to {{report.periodEnd | moment}},
                    </span>

                    <span v-else>
                        On {{report.periodStart | moment("YYYY-MM-DD")}} from {{report.periodStart | moment("hh:mm A")}} to {{report.periodEnd | moment("hh:mm A")}}
                    </span>

                    (over {{(report.periodEnd.getTime() - report.periodStart.getTime()) / 1000 | mduration}})
                </h4>
            </div>

            <hr class="border" />

            <div>
                <h4>
                    View this report at:
                    <a :href="reportUrl">
                        https://wt.honu.pw{{reportUrl}}
                    </a>

                    <button type="button" class="btn btn-primary" @click="copy">
                        Copy
                    </button>
                </h4>
            </div>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import Collapsible from "components/Collapsible.vue";
    import TimeUtils from "util/Time";

    export const ReportHeader = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                outfitsIDs: [] as string[],
                characterIDs: [] as string[]
            }
        },

        methods: {
            bindIDs: function() {
                //this.outfitsIDs = this.report.
            },

            copy: function() {
                try {
                    navigator.clipboard.writeText(location.href);
                } catch (err: any) {
                    console.error(err);
                }
            },

            exportJson: function(): void {
                const json: string = JSON.stringify({
                    ...this.report,
                    characters: Array.from(this.report.characters.values()),
                    outfits: Array.from(this.report.outfits.values()),
                    items: Array.from(this.report.items.values()),
                    itemCategories: Array.from(this.report.itemCategories.values()),
                    facilities: Array.from(this.report.facilities.values()),
                    playerMetadata: Array.from(this.report.playerMetadata.values())
                });

                const name: string = TimeUtils.format(this.report.periodStart, "YYYY-MM-DDThh:mm");

                const anchor = document.createElement("a");
                anchor.setAttribute("href", `data:text/json;charset=utf-8,${encodeURIComponent(json)}`);
                anchor.setAttribute("download", `honu-report-${name}z.json`);
                document.body.appendChild(anchor);

                anchor.click();
                anchor.remove();
            },
        },

        computed: {
            reportUrl: function(): string {
                return `/report/${this.generator64}`;
            },

            generator64: function(): string {
                return btoa(`#${this.report.id};`)
            },

            badStreams: function(): any[] {
                const expCount: number = this.report.reconnects.filter(iter => iter.streamType == "exp").reduce((acc, i) => acc += i.duration, 0);

                const arr: any[] = [];

                const deathCount: number = this.report.reconnects.filter(iter => iter.streamType == "death").reduce((acc, i) => acc += i.duration, 0);
                if (deathCount > 0) {
                    arr.push({ streamType: "death", secondsMissed: deathCount });
                }

                if (expCount > 0) {
                    arr.push({ streamType: "exp", secondsMissed: expCount });
                }

                return arr;
            }
        },

        components: {
            Collapsible
        }
    });

    export default ReportHeader;
</script>