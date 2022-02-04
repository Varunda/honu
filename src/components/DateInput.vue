<template>
    <input type="date" :value="str" @input="handleInput" />
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import DateUtil from "util/Date";

    export const DateInput = Vue.extend({
        props: {
            value: { type: Date as PropType<Date | null>, required: false },
            AllowNull: { type: Boolean, required: false, default: false },
        },

        data: function() {
            return {
                date: new Date() as Date | null,
                str: "" as string | null
            }
        },

        created: function(): void {
            this.updateDate();
        },

        methods: {
            updateDate: function(): void {
                this.date = this.value;
                if (this.date != null) {
                    this.str = DateUtil.getLocalDateOnlyString(DateUtil.zeroParts(this.date, { hours: true, minutes: true, seconds: true }));
                } else {
                    this.str = "";
                }
            },

            handleInput: function(ev: any): void {
                const target: HTMLInputElement = ev.target;
                this.str = target.value;

                if (this.AllowNull == true && (this.str == "" || this.str == undefined)) {
                    this.date = null;
                } else {
                    this.date = new Date(this.str);
                }

                this.$emit("input", this.date);
            }
        },

        watch: {
            value: function(): void {
                this.$nextTick(() => {
                    this.updateDate();
                });
            }
        }
    });
    export default DateInput;

</script>
