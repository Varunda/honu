<template>
    <table class="table table-sm">
        <tr class="table-secondary th-border-top-0">
            <td>{{LeftTitle}}</td>
            <td>{{RightTitle}}</td>
        </tr>

        <tr v-for="datum in entries">
            <td>
                <a v-if="datum.link" :href="datum.link">
                    {{datum.name}}
                </a>
                <span v-else>
                    {{datum.name}}
                </span>
            </td>
            <td>
                {{datum.count | locale}}
                <span v-if="ShowPercent == true">
                    ({{datum.count / Math.max(1, data.total) * 100 | locale}}%)
                </span>
            </td>
        </tr>

        <tr v-if="ShowTotal == true" class="table-dark">
            <td>Total</td>
            <td>{{data.total | locale}}</td>
        </tr>
    </table>

</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Block, BlockEntry } from "./common";

    import "filters/LocaleFilter";

    export const ChartBlockList = Vue.extend({
        props: {
            data: { type: Object as PropType<Block>, required: true },

            LeftTitle: { type: String, required: false, default: "" },
            RightTitle: { type: String, required: false, default: "" },

            ShowAll: { type: Boolean, required: false, default: false },
            ClippedAmount: { type: Number, required: false, default: 8 },
            ShowPercent: { type: Boolean, required: false, default: true },
            ShowTotal: { type: Boolean, required: false, default: true },
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            entries: function(): BlockEntry[] {
                let data: BlockEntry[] = this.data.entries;
                if (this.ShowAll == false) {
                    data = this.data.entries.slice(0, this.ClippedAmount);
                }

                return data;
            }
        }

    });
    export default ChartBlockList;
</script>