﻿<template>
    <div class="progress mt-3" style="height: 3rem;">
        <div :class="'progress-bar bg-' + color" :style="{ width: width }" style="height: 3rem;">
            <span style="position: absolute; left: 50%; transform: translateX(-50%); font-size: 2.5rem;">
                <slot></slot>
                {{progress}}/{{total}}
            </span>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import Busy from "components/Busy.vue";

    export const ProgressBar = Vue.extend({
        props: {
            total: { type: Number, required: true },
            progress: { type: Number, required: true },
            color: { type: String, required: false, default: "primary" }
        },

        computed: {
            width: function(): string {
                return `${(this.progress / this.total) * 100}%`;
            }
        },

        components: {
            Busy
        }
    });
    export default ProgressBar;
</script>

