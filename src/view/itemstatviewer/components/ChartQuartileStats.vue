<template>
    <div v-if="quartile != null" class="d-flex">
        <table class="table table-sm mr-2 flex-grow-1">
            <tr class="table-secondary">
                <td colspan="2">
                    Quartiles
                </td>
            </tr>

            <tr>
                <td><b>Max</b></td>
                <td>
                    {{quartile.max | locale}}{{ShowPercent == true ? "%" : ""}}
                </td>
            </tr>

            <tr>
                <td><b>75%</b></td>
                <td>
                    {{quartile.q3 | locale}}{{ShowPercent == true ? "%" : ""}}
                </td>
            </tr>

            <tr>
                <td><b>Avg</b></td>
                <td>
                    {{quartile.median | locale}}{{ShowPercent == true ? "%" : ""}}
                </td>
            </tr>

            <tr>
                <td><b>25%</b></td>
                <td>
                    {{quartile.q1 | locale}}{{ShowPercent == true ? "%" : ""}}
                </td>
            </tr>

            <tr>
                <td><b>Min</b></td>
                <td>
                    {{quartile.min | locale}}{{ShowPercent == true ? "%" : ""}}
                </td>
            </tr>
        </table>

        <table class="table table-sm flex-grow-1">
            <tr class="table-secondary">
                <td colspan="2">
                    Benchmarks
                    <info-hover text="The top% for this stat"></info-hover>
                </td>
            </tr>

            <tr>
                <td>{{benchmark.plus3.value | locale(2)}}{{ShowPercent ? "%" : ""}}</td>
                <td>{{benchmark.plus3.percent | locale}}%</td>
            </tr>

            <tr>
                <td>{{benchmark.plus2.value | locale(2)}}{{ShowPercent ? "%" : ""}}</td>
                <td>{{benchmark.plus2.percent | locale}}%</td>
            </tr>

            <tr>
                <td>{{benchmark.plus1.value | locale(2)}}{{ShowPercent ? "%" : ""}}</td>
                <td>{{benchmark.plus1.percent | locale}}%</td>
            </tr>

            <tr>
                <td>{{benchmark.zero.value | locale(2)}}{{ShowPercent ? "%" : ""}}</td>
                <td>{{benchmark.zero.percent | locale}}%</td>
            </tr>

            <tr>
                <td>{{benchmark.neg1.value | locale(2)}}{{ShowPercent ? "%" : ""}}</td>
                <td>{{benchmark.neg1.percent | locale}}%</td>
            </tr>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Quartile } from "util/Quartile";
    import Percentile from "util/Percentile";

    import { Bucket } from "api/ItemApi";

    import InfoHover from "components/InfoHover.vue";

    export const ChartQuartileStats = Vue.extend({
        props: {
            data: { type: Array as PropType<Bucket[] | null>, required: false },
            ShowPercent: { type: Boolean, required: false, default: false },
            interval: { type: Number, required: false, default: 1 }
        },

        data: function() {
            return {
                quartile: null as Quartile | null,

                benchmark: {
                    neg1: { value: 0 as number, percent: 0 as number },
                    zero: { value: 0 as number, percent: 0 as number },
                    plus1: { value: 0 as number, percent: 0 as number },
                    plus2: { value: 0 as number, percent: 0 as number },
                    plus3: { value: 0 as number, percent: 0 as number }
                },

                percentile: {
                    top1: 0 as number,
                    top5: 0 as number
                },

                array: [] as number[]
            }
        },

        mounted: function(): void {
            this.make();
        },

        methods: {
            make: function(): void {
                this.quartile = null;
                this.array = [];

                if (this.data == null) {
                    return;
                }

                for (const bucket of this.data) {
                    for (let i = 0; i < bucket.count; ++i) {
                        this.array.push(bucket.start);
                    }
                }

                this.quartile = Quartile.get(this.array);

                const avg: number = this.quartile.median;
                const int: number = this.interval;
                console.log(avg);

                const bmAvg: number = (this.ShowPercent == true) ? (Math.ceil(Math.trunc(avg) / 5) * 5) : Math.ceil(avg);

                this.benchmark.neg1.value = bmAvg - int;
                this.benchmark.neg1.percent = (1 - Percentile.rank(this.array, bmAvg - int)) * 100;

                this.benchmark.zero.value = bmAvg;
                this.benchmark.zero.percent = (1 - Percentile.rank(this.array, bmAvg)) * 100;

                this.benchmark.plus1.value = bmAvg + int;
                this.benchmark.plus1.percent = (1 - Percentile.rank(this.array, bmAvg + int)) * 100;

                this.benchmark.plus2.value = bmAvg + int + int;
                this.benchmark.plus2.percent = (1 - Percentile.rank(this.array, bmAvg + int + int)) * 100;

                this.benchmark.plus3.value = bmAvg + (3 * int);
                this.benchmark.plus3.percent = (1 - Percentile.rank(this.array, bmAvg + (3 * int))) * 100;

            }
        },

        components: {
            InfoHover
        }
    });
    export default ChartQuartileStats;
</script>