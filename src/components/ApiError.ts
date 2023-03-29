import Vue from "vue";

export const ApiError = Vue.extend({
    props: {
        error: { required: true }
    },

    computed: {
        getErrorType: function(): string {
            if (typeof (this.error) == "string" || this.error instanceof String) {
                return "string";
            }

            if (typeof (this.error) == "object") {
                if (this.error == null) {
                    return "unknown";
                }

                if ("message" in this.error) {
                    return "problems";
                }
            }

            return "unknown";
        }
    },

    template: `
        <div>
            <div v-if="getErrorType == 'string'>
                error: {{error}}
            </div>

            <div v-else-if="getErrorType == 'problems'>

            </div>

            <div v-else-if="getErrorType == 'unknown'>

            </div>

            <div v-else>

            </div>
        </div>
    `
});

export default ApiError;