<template>
    <div>
        <span v-if="value == null">
            --
        </span>

        <span v-else :style="{ color: textColor }" :title="'This value is better than ' + value.toFixed(2) + '% of other players'">
            {{value.toFixed(2)}}%
        </span>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    export const PercentileCell = Vue.extend({
        props: {
            value: { type: Number, required: false, default: null }
        },

        computed: {
            textColor: function(): string {
                if (this.value == null) {
                    return "#fff";
                }

                if (this.value > 95) {
                    return "#FFD700";
                }
                if (this.value > 80) {
                    return "#46C646";
                }

                return "#ADD8E6";
            }
        }
    });
    export default PercentileCell;
</script>