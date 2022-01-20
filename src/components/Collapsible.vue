<template>
    <div>
        <h2 class="wt-header" data-toggle="collapse" :data-target="'#' + elementID">
            {{HeaderText}}
        </h2>

        <div :id="elementID" class="collapse" :class="{ show: opened }">
            <slot></slot>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    export const Collapsible = Vue.extend({
        props: {
            HeaderText: { type: String, required: true },
            show: { type: Boolean, required: false, default: true }
        },

        data: function() {
            return {
                id: Math.floor(Math.random() * 1000000),
                opened: this.show as boolean
            }
        },

        methods: {

        },

        computed: {
            elementID: function(): string {
                return `collapsible-${this.id}`;
            }
        }
    });
    export default Collapsible;
</script>