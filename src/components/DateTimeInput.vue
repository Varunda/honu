<template>
    <input type="datetime-local" :value="str" @input="handleInput" />
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import DateUtil from "util/Date";

    export const DateTimeInput = Vue.extend({
        props: {
            value: { type: Date as PropType<Date>, required: false },
            AllowNull: { type: Boolean, required: false, default: false }
        },

        data: function() {
            return {
                date: new Date() as Date,
                str: "" as string
            }
        },

        created: function(): void {
            this.updateDate();
        },

        methods: {
            updateDate: function(): void {
                this.date = this.value;
                this.str = DateUtil.getLocalDateString(this.date);
            },

            handleInput: function(ev: any): void {
                const target: HTMLInputElement = ev.target;
                this.str = target.value;
                this.date = new Date(this.str);

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
    export default DateTimeInput;

</script>