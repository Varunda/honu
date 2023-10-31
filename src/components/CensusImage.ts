import Vue from "vue";

export const CensusImage = Vue.extend({
    props: {
        ImageId: { type: Number, required: true }
    },

    template: `
        <img :src="'/image-proxy/get/' + ImageId" loading="lazy" />
    `

});

export default CensusImage;