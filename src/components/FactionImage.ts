import Vue from "vue";

import "filters/FactionNameFilter";
import ColorUtil from "util/Color";

export const Faction = Vue.extend({
    props: {
        FactionId: { type: Number, required: true }
    },

    computed: {
        fileName: function(): string {
            if (this.FactionId == 1) {
                return "logo_vs";
            }
            if (this.FactionId == 2) {
                return "logo_nc";
            }
            if (this.FactionId == 3) {
                return "logo_tr";
            }
            return "logo_ns";
        }
    },

    template: `
        <img :src="'/img/' + fileName + '.png'" />
    `
});

export default Faction;
