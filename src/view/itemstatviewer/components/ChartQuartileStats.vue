<template>
    <table v-if="quartile != null" class="table table-sm">
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
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Quartile } from "util/Quartile";

    import { Bucket } from "api/ItemApi";

    export const ChartQuartileStats = Vue.extend({
        props: {
            data: { type: Array as PropType<Bucket[] | null>, required: false },
            ShowPercent: { type: Boolean, required: false, default: false }
        },

        data: function() {
            return {
                quartile: null as Quartile | null,
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
            }
        }
    });
    export default ChartQuartileStats;
</script>