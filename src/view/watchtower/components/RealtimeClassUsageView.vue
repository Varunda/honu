
<template>
    <div v-if="data != undefined" class="focus-parent">
        <span v-if="data.infil > 0" :style="{ 'flex-grow': infilWidth, 'background-color': 'var(--color-class-infil)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_infil.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.infil}}
            </span>
            <span v-else>
                {{infilWidth}}%
            </span>
        </span>

        <span v-if="data.lightAssault > 0" :style="{ 'flex-grow': lightWidth, 'background-color': 'var(--color-class-light-assault)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_light.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.lightAssault}}
            </span>
            <span v-else>
                {{lightWidth}}%
            </span>
        </span>

        <span v-if="data.combatMedic > 0" :style="{ 'flex-grow': medicWidth, 'background-color': 'var(--color-class-medic)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_medic.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.combatMedic}}
            </span>
            <span v-else>
                {{medicWidth}}%
            </span>
        </span>

        <span v-if="data.engineer > 0" :style="{ 'flex-grow': engiWidth, 'background-color': 'var(--color-class-engi)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_engi.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.engineer}}
            </span>
            <span v-else>
                {{engiWidth}}%
            </span>
        </span>

        <span v-if="data.heavyAssault > 0" :style="{ 'flex-grow': heavyWidth, 'background-color': 'var(--color-class-heavy)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_heavy.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.heavyAssault}}
            </span>
            <span v-else>
                {{heavyWidth}}%
            </span>
        </span>

        <span v-if="data.max > 0" :style="{ 'flex-grow': maxWidth, 'background-color': 'var(--color-class-max)' }" class="focus-entry">
            <img v-if="ShowIcons" src="/img/classes/icon_max.png" class="class-usage-icon" />

            <span v-if="ShowCount">
                {{data.max}}
            </span>
            <span v-else>
                {{maxWidth}}%
            </span>
        </span>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { RealtimeClassUsage } from "../WorldData";

    export const RealtimeClassUsageView = Vue.extend({
        props: {
            data: { type: Object as PropType<RealtimeClassUsage>, required: false },
            ShowCount: { type: Boolean, required: false, default: false },
            ShowIcons: { type: Boolean, required: false, default: true }
        }, 

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {

            infilWidth: function(): string {
                return `${(this.data.infil / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },

            lightWidth: function(): string {
                return `${(this.data.lightAssault / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },

            medicWidth: function(): string {
                return `${(this.data.combatMedic / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },

            engiWidth: function(): string {
                return `${(this.data.engineer / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },

            heavyWidth: function(): string {
                return `${(this.data.heavyAssault / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },

            maxWidth: function(): string {
                return `${(this.data.max / Math.max(1, this.data.total) * 100).toFixed(2)}`
            },
        },

        components: {

        }
    });
    export default RealtimeClassUsageView;
</script>