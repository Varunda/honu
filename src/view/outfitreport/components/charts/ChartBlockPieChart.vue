<template>
    <canvas :id="'chart-block-pie-chart-' + id" style="height: 300px; max-height: 300px;" class="mb-2"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Chart, { LegendItem } from "chart.js/auto/auto.esm";

    import { randomRGB, rgbToString, randomColors, randomColor } from "util/Color";
    import { Block, BlockEntry } from "./common";

    export const ChartBlockPieChart = Vue.extend({
        props: {
            data: { type: Object as PropType<Block>, required: true },
            ShowAll: { type: Boolean, required: false, default: false },
            ClippedAmount: { type: Number, required: false, default: 8 },
            ShowPercent: { type: Boolean, required: false, default: false },
            PercentPrecision: { type: Number, required: false, default: 0 },
            ShowTotal: { type: Boolean, required: false, default: false },
        },

        data: function() {
            return {
                chart: null as Chart | null,

                id: Math.floor(Math.random() * 100000) as number,

                labels: [] as string[],
                numbers: [] as number[],
                colors: [] as string[]
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.setup();
                this.makeChart();
            });
        },

        methods: {

            setup: function(): void {
                const shown: BlockEntry[] = (this.ShowAll == true)
                    ? this.data.entries
                    : this.data.entries.slice(0, this.ClippedAmount);

                const colorHue: number = Math.random();
                let colorLen: number = Math.min(this.data.entries.length, this.ClippedAmount);
                if (this.ShowAll == false && this.data.entries.length > this.ClippedAmount) {
                    ++colorLen;
                }

                this.labels = shown.map((iter: BlockEntry) => iter.name);
                this.numbers = shown.map((iter: BlockEntry) => iter.count);
                this.colors = randomColors(colorHue, colorLen);

                if (this.ShowAll == false && this.data.entries.length > this.ClippedAmount) {
                    const hidden = this.data.entries.slice(this.ClippedAmount);

                    this.labels.push("Other");
                    this.numbers.push(hidden.reduce((acc, val) => acc += val.count, 0));
                    this.colors.push(randomColor(colorHue, colorLen, colorLen - 1));
                }
            },

            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const elem = document.getElementById(`chart-block-pie-chart-${this.id}`);
                if (elem == null) {
                    throw `Failed to get canvas #chart-block-pie-chart-${this.id}`;
                }

                try {
                    const ctx = (elem as any).getContext("2d");
                    this.chart = new Chart(ctx, {
                        type: "pie",
                        data: {
                            labels: this.labels,
                            datasets: [{
                                data: this.numbers,
                                backgroundColor: this.colors
                            }]
                        },
                        options: {
                            plugins: {
                                legend: {
                                    display: true,
                                    position: "right",
                                    align: "center",
                                    labels: {
                                        generateLabels: (chart: Chart) => {
                                            const dataset = chart.data.datasets![0];
                                            let sum: number = 0;
                                            for (const datum of dataset.data!) {
                                                if (typeof (datum) == "number") {
                                                    sum += datum;
                                                }
                                            }

                                            return chart.data.labels?.map((label, index) => {
                                                const datum = dataset.data![index];
                                                if (typeof (datum) == "number") {
                                                    return {
                                                        text: `${(label as any).toString()} - ${datum} ${this.ShowPercent == true ? `(${(datum / sum * 100).toFixed(this.PercentPrecision)}%)` : ""}`,
                                                        fillStyle: this.colors[index],
                                                        datasetIndex: index,
                                                        fontColor: "#fff"
                                                    };
                                                }
                                                throw `Invalid type of data '${typeof (datum)}': ${datum}`;
                                            }) ?? [];
                                        }
                                    }
                                }
                            },
                            layout: {
                                padding: {
                                    right: 40
                                }
                            },
                            responsive: false,
                            maintainAspectRatio: false
                        }
                    });
                } catch (err: any) {
                    console.error(`Failed to created chart: ${err}`);
                }
            }

        }

    });

    export default ChartBlockPieChart;

</script>