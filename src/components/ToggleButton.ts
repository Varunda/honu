import Vue from "vue";

export const ToggleButton = Vue.extend({
    props: {
        value: { type: Boolean, required: true },
        TrueColor: { type: String, default: "btn-success" },
    },

    template: `
        <button type="button" :class="cssClasses" :style="cssStyles" @click.stop="toggle($event)">
            <slot></slot>
        </button>
    `,

    methods: {
        toggle: function(ev: Event): void {
            this.$emit("input", !this.value);
        }
    },

    computed: {
        cssClasses: function(): string {
            let css: string = "btn border";
            if (this.value == true && this.TrueColor.startsWith("#") == false) {
                css += ` ${this.TrueColor}`;
            }
            return css;
        },

        cssStyles: function(): string {
            let css: string = "";
            if (this.value == true && this.TrueColor.startsWith("#") == true) {
                css += `background-color: ${this.TrueColor}`;
            }

            return css;
        }

    }

});
export default ToggleButton;