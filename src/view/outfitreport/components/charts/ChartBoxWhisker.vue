<template>
    <div :id="'breakdown-box-' + ID + '-parent'">
        <canvas :id="'breakdown-box-' + ID"></canvas>

        <table class="table table-sm border-top" :id="'breakdown-box-' + ID + '-quartile'">
            <tr>
                <th>Min</th>
                <th>Q1</th>
                <th>Median</th>
                <th>Q3</th>
                <th>Max</th>
            </tr>
            <tr>
                <td>{{quartile.min | locale(2)}}</td>
                <td>{{quartile.q1 | locale(2)}}</td>
                <td>{{quartile.median | locale(2)}}</td>
                <td>{{quartile.q3 | locale(2)}}</td>
                <td>{{quartile.max | locale(2)}}</td>
            </tr>
        </table>
    </div>

</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Chart from "node_modules/chart.js/auto/auto.esm";
    import { BoxPlotController, BoxAndWiskers } from "node_modules/@sgratzl/chartjs-chart-boxplot/build/index.js";
    Chart.register(BoxPlotController, BoxAndWiskers);

    import Quartile from "util/Quartile";

    export const ChartBoxWhisker = Vue.extend({
        props: {
            data: { type: Array as PropType<number[]>, required: true }
        },

        data: function() {
            return {
                ID: Math.round(Math.random() * 100000) as number,

                chart: null as Chart | null,

                quartile: new Quartile() as Quartile,
            }
        },

        created: function(): void {
            this.quartile = Quartile.get(this.data);
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeGraph();
            });
        },

        methods: {
            makeGraph: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const quartileHeight: number = (document.getElementById(`breakdown-box-${this.ID}-quartile`)as HTMLTableElement).clientHeight;

                (document.getElementById(`breakdown-box-${this.ID}-parent`) as HTMLDivElement).style.height = `${1 * 40 + 20 + quartileHeight}px`;
                (document.getElementById(`breakdown-box-${this.ID}`) as HTMLDivElement).style.height = `${1 * 40 + 20}px`;
                (document.getElementById(`breakdown-box-${this.ID}`) as HTMLDivElement).style.maxHeight = `${1 * 40 + 20}px`;

                const ctx = (document.getElementById(`breakdown-box-${this.ID}`) as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "boxplot",
                    data: {
                        labels: [""],
                        datasets: [{
                            label: "",
                            backgroundColor: "#6c6c6c",
                            borderColor: "#8c8c8c",
                            borderWidth: 2,
                            data: [this.data],
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        indexAxis: "y",
                        plugins: {
                            legend: {
                                display: false
                            },
                            title: {
                                display: false
                            },
                        }
                    }
                });
            }
        },

        components: {

        }
    });
    export default ChartBoxWhisker;
</script>