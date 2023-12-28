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

                if ("title" in this.error) {
                    return "problems";
                }
            }

            return "unknown";
        }
    },

    template: `
        <div>
            <div v-if="getErrorType == 'string'">
                error: {{error}}
            </div>

            <div v-else-if="getErrorType == 'problems'">
                <div>
                    An error occured in this API request: <code>{{error.title}}</code>
                </div>

                <div>
                    Erroring URL: <code>{{error.type}}</code>
                </div>
            </div>

            <div v-else-if="getErrorType == 'unknown'">
                unknown error format: {{error}}
            </div>

            <div v-else>
                unchecked error type: {{getErrorType}}
            </div>
        </div>
    `
});

export default ApiError;