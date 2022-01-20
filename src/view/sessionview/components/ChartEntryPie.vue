<template>
    <canvas :id="'chart-entry-pie-' + ID" style="max-height: 300px;" class="w-auto mb-2"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Chart from "chart.js/auto/auto.esm";
    //import { randomRGB, rgbToString, randomColors } from "util/Color";
    import ColorUtils from "util/Color";

    interface Entry {
        display: string;
        count: number;
    }

    export const ChartEntryPie = Vue.extend({
        props: {
            data: { type: Array as PropType<Entry[]>, required: true },
            MaxEntries: { type: Number, required: false, default: 8 }
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000) as number,
                chart: null as Chart | null
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeChart();
            });
        },

        methods: {
            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const elem = document.getElementById(`chart-entry-pie-${this.ID}`);
                if (elem == null) {
                    return console.error(`chart-entry-pie-${this.ID} does not exist, cannot make chart`);
                }

                const ctx = (elem as any).getContext("2d");

                const total: number = this.data.reduce((acc, iter) => acc += iter.count, 0);
                const show: Entry[] = this.data.slice(0, this.MaxEntries);

                if (this.data.length > this.MaxEntries) {
                    const rest: Entry = {
                        display: "Other",
                        count: this.data.slice(this.MaxEntries).reduce((acc, iter) => acc += iter.count, 0)
                    };
                    show.push(rest);
                }

                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: show.map(iter => `${iter.display} - ${(iter.count / total * 100).toFixed(2)}%`),
                        datasets: [{
                            data: show.map(iter => iter.count),
                            backgroundColor: ColorUtils.randomColors(Math.random(), show.length)
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: "right",
                                labels: {
                                    color: "#fff"
                                }
                            }
                        },
                    }
                });
            }

        }

    });
    export default ChartEntryPie;
</script>