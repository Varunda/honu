<template>
    <div>
        <div>
            <div v-if="activity.state == 'idle'"></div>
            <busy v-else-if="activity.state == 'loading'" class="honu-busy-lg"></busy>

            <div v-else-if="activity.state == 'error'">
                Error loading outfit activity:
                <br />
                <api-error :error="activity.problem"></api-error>
            </div>

            <canvas v-else-if="activity.state == 'loaded'" :id="'outfit-activity-graph-' + ID"
                style="height: 240px; max-height: 40vh;" class="mb-2">
            </canvas>

            <div v-else>
                Unchecked state of 'activity': '{{activity.state}}'
            </div>
        </div>

        <div>
            <div class="input-group d-inline-flex" style="width: unset;">
                <date-time-input v-model="activityPeriod.finish" class="form-control d-inline-block" style="width: unset;"></date-time-input>

                <span class="input-group-text input-group-addon">
                    -
                </span>

                <date-time-input v-model="activityPeriod.start" class="form-control d-inline-block" style="width: unset;"></date-time-input>

                <button type="button" class="btn btn-primary input-group-append" @click="loadBoth">
                    Load
                </button>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import ApiError from "components/ApiError";

    import { Loading, Loadable } from "Loading";
    import { OutfitActivity, OutfitApi } from "api/OutfitApi";

    import Chart from "chart.js/auto/auto.esm";

    import Busy from "components/Busy.vue";
    import DateTimeInput from "components/DateTimeInput.vue";

    import TimeUtils from "util/Time";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";
    import "filters/LocaleFilter";

    export const OutfitActivityGraph = Vue.extend({
        props: {
            OutfitId: { type: String, required: true }
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000) as number,
                activity: Loadable.idle() as Loading<OutfitActivity[]>,
                chart: null as Chart | null,

                activityPeriod: {
                    start: new Date() as Date,
                    finish: new Date() as Date
                }
            }
        },

        created: function(): void {
            this.activityPeriod.finish.setDate(this.activityPeriod.finish.getDate() - 7);
        },

        mounted: function(): void {
            this.loadActivity().then(() => {
                this.makeGraph();
            });
        },

        methods: {
            loadBoth: async function(): Promise<void> {
                await this.loadActivity();
                this.makeGraph();
            },

            loadActivity: async function(): Promise<void> {
                this.activity = Loadable.loading();
                this.activity = await OutfitApi.getActivity(this.OutfitId, this.activityPeriod.finish, this.activityPeriod.start);
            },

            makeGraph: function(): void {
                if (this.activity.state != "loaded") {
                    return console.error(`cannot makeGraph: activity is not loaded, currently ${this.activity.state}`);
                }

                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const elem = document.getElementById(`outfit-activity-graph-${this.ID}`);
                if (elem == null) {
                    throw `Failed to get canvas #outfit-activity-graph-${this.ID}`;
                }

                const ctx = (elem as any).getContext("2d");

                console.log(`Goes from ${this.activity.data[0].timestamp} to ${this.activity.data[this.activity.data.length - 1].timestamp}`);

                this.chart = new Chart(ctx, {
                    type: "bar",
                    data: {
                        labels: this.activity.data.map(iter => iter.timestamp),
                        datasets: [
                            {
                                data: this.activity.data.map(iter => iter.count),
                                backgroundColor: "#fff",
                                barPercentage: 1, // These 3 are what make the bars continious without any gaps between continous x values
                                categoryPercentage: 1,
                                barThickness: "flex",
                                label: "Online",
                                hoverBackgroundColor: "#00bc8c",
                            }
                        ]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: {
                                labels: {
                                    color: "#fff",
                                }
                            },
                            tooltip: {
                                callbacks: {
                                    title: function(context) {
                                        if (context[0].chart.data.labels == undefined) {
                                            throw ``;
                                        }
                                        const datum = context[0].chart.data.labels[context[0].dataIndex] as Date;
                                        return TimeUtils.format(datum);
                                    }
                                }
                            }
                        },
                        interaction: {
                            intersect: false,
                            mode: "index"
                        },
                        scales: {
                            x: {
                                grid: {
                                    color: "#fff",
                                },
                                ticks: {
                                    color: "#fff",
                                    callback: function(value, index, ticks): string | null {
                                        if (this.chart.data.labels == undefined) {
                                            console.warn(`no labels?`);
                                            return null;
                                        }
                                        const label: Date = this.chart.data.labels[index] as Date;
                                        if (label.getHours() != 12 && index != ticks.length - 1) {
                                            return null;
                                        }
                                        return TimeUtils.format(label, "YYYY-MM-DD");
                                    },
                                },
                            },
                            y: {
                                ticks: {
                                    color: "#fff",
                                    precision: 0
                                },
                                beginAtZero: true,
                            }
                        }
                    }

                });
            }
        },

        components: {
            Busy, ApiError,
            DateTimeInput
        }
    });

    export default OutfitActivityGraph;
</script>