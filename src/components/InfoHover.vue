<template>
    <span :id="'info-hover-' + ID"
            class="d-inline-block" data-toggle="popover" data-trigger="hover"
            :data-content="text"
            :data-html="AllowHtml"
            :style="{ 'filter': !icon ? 'invert(1)' : ''}">

        <span v-if="icon != null" :class="'fas fa-' + icon">

        </span>

        <img v-else src="/img/question-circle.svg" />
    </span>
</template>

<script lang="ts">
    import Vue from "vue";

    export const InfoHover = Vue.extend({
        props: {
            text: { type: String, required: true },
            AllowHtml: { type: Boolean, required: false, default: false },
            icon: { type: String, required: false }
        },

        data: function () {
            return {
                ID: 0 as number
            };
        },

        created: function () {
            this.ID = Math.floor(Math.random() * 10000);
        },

        mounted: function () {
            this.$nextTick(() => {
                $(`#info-hover-${this.ID}`).popover();
            });
        },
    });

    export default InfoHover;
</script>