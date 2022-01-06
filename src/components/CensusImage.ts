
import Vue from "vue";

export const CensusImage = Vue.extend({
    props: {
        ImageId: { type: Number, required: true }
    },

    template: `
        <img :src="'https://census.daybreakgames.com/files/ps2/images/static/' + ImageId + '.png'" loading="lazy" />
    `

});

export default CensusImage;