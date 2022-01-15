<template>
    <div>
        <h2 class="wt-header" data-toggle="collapseeeeeeeeeee" data-target="#report-header">
            Outfit report
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
                    navigator.clipboard.writeText(this.reportUrl);
                } catch (err: any) {
                    console.error(err);
                }
            }
        },

        computed: {
            reportUrl: function(): string {
                return `/report/${this.generator64}`;
            },

            generator64: function(): string {
                return btoa(`#${this.report.id};`)
            }
        }
    });

    export default ReportHeader;
</script>