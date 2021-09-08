<template>
    <div class="input-group" :id="'flatpickr' + id">
        <input type="text" class="form-control flatpickr-input" :placeholder="placeholder" data-input :disabled="readonly" />
        <div class="input-group-append d-print-none" data-toggle>
            <span class="input-group-text"><span class="far fa-calendar-alt"></span></span>

            <slot></slot>
        </div>
    </div>
</template>

<!--
<style>
    .flatpickr-input[readonly] {
        background-color: unset;
        opacity: unset;
    }
</style>
-->

<script lang="ts">
    import Vue from "vue";
    import flatpickr from "flatpickr";
    import * as moment from "moment";

    // Interface holding the properties set to the global window property
    interface FlatpickrGlobalInterface {
        state: "mounted" | "mounting" | "unmounted";
        count: number;
    };

    export const DateTimePicker = Vue.extend({
        props: {
            // Input element value that is used to display the modeled value
            value: { type: [String, Date], required: false },

            // Are null values be allowed?
            AllowNull: { type: Boolean, required: false, default: false },

            // Is selecting the time also be enabled?
            EnableTime: { type: Boolean, required: false, default: false },

            // Format to display the selected date as
            format: { type: String, required: false, default: "Y-m-d H:i" },

            // How will the selected date be emitted as? Works with C#
            EmitFormat: { type: String, required: false, default: "YYYY-MM-DDThh:mm:ss.SSS" },

            // Emit as a date instead of as a string?
            EmitDate: { type: Boolean, required: false, default: false },

            // Placeholder when a value is unselected
            placeholder: { type: String, required: false, default: "" },

            // Can manual input be used?
            AllowInput: { type: Boolean, required: false, default: false },

            // Is this value readonly? TODO: Implement this
            readonly: { type: Boolean, required: false, default: false }
        },

        data: function() {
            return {
                instance: null as (flatpickr.Instance | flatpickr.Instance[] | null),
                id: 0 as number
            }
        },

        mounted: function() {
            if (document.head == null) { throw `document.head is null, cannot mount flatpickr`; }

            let cfg: FlatpickrGlobalInterface | undefined = (window as any).Flatpickr;
            if (cfg == undefined) {
                cfg = { state: "unmounted", count: 0 };
            }
            this.id = cfg.count++;

            if (cfg.state == "unmounted") {
                let loadedJS: boolean = false;
                let loadedCSS: boolean = false;

                cfg.state = "mounting";

                // Mount the Javascript needed
                const script = document.createElement("script");
                script.setAttribute("src", "/lib/flatpickr/dist/flatpickr.js");
                script.setAttribute("async", "true");
                script.onload = ((event: Event) => {
                    console.log(`DateTimePicker> flatpickr js loaded`);
                    loadedJS = true;
                    if (loadedCSS == true) { ((window as any).Flatpickr as FlatpickrGlobalInterface).state = "mounted"; }
                });
                document.head.appendChild(script);
                console.log(`DateTimePicker> mounted flatpickr js`);

                // Mount the CSS needed for proper styling
                const css = document.createElement("link");
                css.setAttribute("href", "/lib/flatpickr/dist/flatpickr.css");
                css.setAttribute("rel", "stylesheet");
                css.setAttribute("async", "true");
                css.onload = ((event: Event) => {
                    console.log(`DateTimePicker> flatpickr css loaded`);
                    loadedCSS = true;
                    if (loadedJS == true) { ((window as any).Flatpickr as FlatpickrGlobalInterface).state = "mounted"; }
                });
                document.head.appendChild(css);
                console.log(`DateTimePicker> mounted flatpickr css`);
            }

            console.log(`DateTimePicker> instance ${this.id} loaded`);

            (window as any).Flatpickr = cfg;
            this.$nextTick(() => {
                this.init();
            });
        },

        created: function(): void {
            if (this.AllowNull && this.value == null) {
                return;
            }
            if (!moment(this.value).isValid()) {
                console.error("DateTimePicker> Invalid date string", this.value);
            }
        },

        beforeDestroy: function(): void {
            if (this.instance != null) {
                if (Array.isArray(this.instance)) {
                    for (const inst of this.instance) {
                        inst.destroy();
                    }
                } else {
                    this.instance.destroy();
                }
                console.log(`DateTimePicker> Destroyed instance ${this.id}`);
            }
        },

        methods: {
            init: function(): void {
                const cfg: FlatpickrGlobalInterface | undefined = (window as any).Flatpickr;
                if (cfg == undefined) {
                    throw `Flatpickr window interface was undefined in init(). Interface must also be set`;
                }

                if (cfg.state != "mounted") {
                    console.log(`DateTimePicker> flatpickr scripts not yet mounted, trying again in ~500ms`);
                    // Spread the retries out a bit, anywhere from 400ms to 600ms later
                    setTimeout(this.init, 400 + Math.random() * 200);
                    return;
                }

                if (this.readonly == true) {
                    let dateValue = "";
                    if (this.value == null && this.AllowNull) {
                        dateValue = "";
                    } else if (this.value != null && this.EnableTime == true) {
                        dateValue = moment(this.value).format("YYYY-MM-DD hh:mmA");
                    } else if (this.value != null) {
                        dateValue = moment(this.value).format("YYYY-MM-DD");
                    } else {
                        console.error(`DateTimePicker> Unchecked way to format the date-time-picker`);
                        debugger;
                    }
                    $("#flatpickr" + this.id).replaceWith(`<input class='form-control' type='text' disabled value='${dateValue}' />`);
                } else {
                    this.instance = flatpickr("#flatpickr" + this.id, {
                        enableTime: this.EnableTime,
                        altFormat: this.format,
                        defaultDate: this.value,
                        allowInput: this.AllowInput,
                        wrap: true, // Wrap any input-group buttons using the data-input and data-toggle dataset properties

                        onClose: (val: Date[], str: string, inst: any) => {
                            if (val.length > 1) {
                                console.warn(`DateTimePicker> Multiple date values are not currently supported. Using 0th`);
                            }
                            if (val.length > 0) {
                                // Can't get Vue to actually use the Date class for prop members, so instead a string formatted
                                // how Vue expects it is emited. The default from this.EmitFormat is what C# expects,
                                // but just in case it isn't good, the option is available

                                if (this.EmitDate == true) {
                                    this.$emit("input", val[0]);
                                } else {
                                    this.$emit("input", moment(val[0]).format(this.EmitFormat));
                                }
                            } else if (val.length == 0 && this.AllowNull == true) {
                                this.$emit("input", null);
                            }
                        },
                    });
                    if (this.instance == null || (Array.isArray(this.instance) && this.instance.length == 0)) {
                        throw `Failed to start ${'#flatpickr' + this.id}. No instance returned`;
                    }
                }
            }
        }

    });
    export default DateTimePicker;

</script>
