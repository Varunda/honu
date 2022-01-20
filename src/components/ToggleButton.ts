import Vue from "vue";

export const ToggleButton = Vue.extend({
    props: {
        value: { type: Boolean, required: true },
    },

    template: `
        <button type="button" :class="[ 'btn', 'border', value == true ? 'btn-success' : 'btn-secondary' ]" @click="$emit('input', !value)">
            <slot></slot>
        </button>
    `
});
export default ToggleButton;