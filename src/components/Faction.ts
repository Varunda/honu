import Vue from "vue";

import "filters/FactionNameFilter";
import ColorUtil from "util/Color";

export const Faction = Vue.extend({
    props: {
        FactionId: { type: Number, required: true }
    },

    computed: {
        style: function() {
            return {
                color: ColorUtil.getFactionColor(this.FactionId)
            }
        }
    },

    template: `
        <span :style=style>{{FactionId | faction}}</span>
    `
});

export default Faction;
