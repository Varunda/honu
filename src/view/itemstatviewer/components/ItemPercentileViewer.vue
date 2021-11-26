<template>
    <div>
        <div v-if="all.state == 'idle'"></div>
        <div v-else-if="all.state == 'loading'">
            Loading...
        </div>

        <div v-else-if="all.state == 'loaded'">
            <h3>KD</h3>
            <div class="row" v-if="all.data.kd != null">
                <div class="col-2">
                    <chart-quartile-stats :data="all.data.kd"></chart-quartile-stats>
                </div>
                <div class="col-5">
                    <chart-item-percentile-stats :stats="all.data.kd" name="KD"></chart-item-percentile-stats>
                </div>
                <div class="col-5">
                    <chart-item-total-stats :stats="all.data.kd" name="KD"></chart-item-total-stats>
                </div>
            </div>

            <h3>KPM</h3>
            <div class="row" v-if="all.data.kpm != null">
                <div class="col-2">
                    <chart-quartile-stats :data="all.data.kpm"></chart-quartile-stats>
                </div>
                <div class="col-5">
                    <chart-item-percentile-stats :stats="all.data.kpm" name="KPM"></chart-item-percentile-stats>
                </div>
                <div class="col-5">
                    <chart-item-total-stats :stats="all.data.kpm" name="KPM"></chart-item-total-stats>
                </div>
            </div>

            <h3>Accuracy</h3>
            <div class="row" v-if="all.data.accuracy != null">
                <div class="col-2">
                    <chart-quartile-stats :data="all.data.accuracy" :show-percent="true"></chart-quartile-stats>
                </div>
                <div class="col-5">
                    <chart-item-percentile-stats :stats="all.data.accuracy" name="Acc%"></chart-item-percentile-stats>
                </div>
                <div class="col-5">
                    <chart-item-total-stats :stats="all.data.accuracy" name="Acc%"></chart-item-total-stats>
                </div>
            </div>

            <h3>Headshot Ratio</h3>
            <div class="row" v-if="all.data.headshotRatio != null">
                <div class="col-2">
                    <chart-quartile-stats :data="all.data.headshotRatio" :show-percent="true"></chart-quartile-stats>
                </div>
                <div class="col-5">
                    <chart-item-percentile-stats :stats="all.data.headshotRatio" name="HSR%" v-if="all.data.headshotRatio != null"></chart-item-percentile-stats>
                </div>
                <div class="col-5">
                    <chart-item-total-stats :stats="all.data.headshotRatio" name="HSR%" v-if="all.data.headshotRatio != null"></chart-item-total-stats>
                </div>
            </div>
        </div>

        <div v-else-if="all.state == 'error'" class="text-danger">
            Error loading stats: {{all.message}}
        </div>

        <div v-else class="text-danger">
            Unchecked state of all: '{{all.state}}'
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";

    import { Quartile } from "util/Quartile";

    import { ItemPercentileStats, ItemPercentileAll, ItemApi } from "api/ItemApi";

    import ChartItemPercentileStats from "./ChartItemPercentileStats.vue";
    import ChartItemTotalStats from "./ChartItemTotalStats.vue";
    import ChartQuartileStats from "./ChartQuartileStats.vue";

    export const ItemPercentileViewer = Vue.extend({
        props: {
            ItemId: { type: String, required: true }
        },

        data: function() {
            return {
                all: Loadable.idle() as Loading<ItemPercentileAll | null>,

                kpm: null as Quartile | null,
                kd: null as Quartile | null,
                acc: null as Quartile | null,
                hsr: null as Quartile | null,
            }
        },

        mounted: function(): void {
            this.bindAll();
        },

        methods: {
            bindAll: async function(): Promise<void> {
                this.all = Loadable.loading();
                this.all = await Loadable.promise(ItemApi.getStatsByID(this.ItemId));

                if (this.all.state == "loaded" && this.all.data != null) {
                    if (this.all.data.kpm != null) {
                        this.kpm = Quartile.get(this.all.data.kpm.map(iter => iter.start));
                    }
                    if (this.all.data.kd != null) {
                        this.kd = Quartile.get(this.all.data.kd.map(iter => iter.start));
                    }
                    if (this.all.data.accuracy != null) {
                        this.acc = Quartile.get(this.all.data.accuracy.map(iter => iter.start));
                    }
                    if (this.all.data.headshotRatio != null) {
                        this.hsr = Quartile.get(this.all.data.headshotRatio.map(iter => iter.start));
                    }
                }

            }
        },

        components: {
            ChartItemPercentileStats,
            ChartItemTotalStats,
            ChartQuartileStats
        }
    });
    export default ItemPercentileViewer;
</script>