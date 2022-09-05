<template>
    <div style="display: grid; grid-template-columns: 1fr min-content; white-space: nowrap;">
        <div style="padding: 0.25rem;">
            {{LeftTitle}}
        </div>
        <div style="padding: 0.25rem;">
            {{RightTitle}}
        </div>

        <template v-for="datum in entries">
            <div style="text-overflow: ellipsis; overflow: hidden; padding: 0.25rem;" class="border-top">
                {{datum.name}}
            </div>
            <div style="padding: 0.25rem;" class="border-top">
                {{datum.count | locale}}
                <span v-if="ShowPercent == true">
                    ({{datum.count / Math.max(1, data.total) * 100 | locale}}%)
                </span>
            </div>
        </template>

        <template v-if="ShowTotal == true">
            <div class="border-top" style="padding: 0.25rem;">
                Total
            </div>
            <div class="border-top" style="padding: 0.25rem;">
                {{data.total | locale}}
            </div>
        </template>
    </div>
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