<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Alert create
            </li>
        </honu-menu>

        <div>
            <div>
                Name
                <input v-model="alert.name" class="form-control" placeholder="Name" />
            </div>

            <div>
                Timestamp
                <date-time-input v-model="alert.timestamp" class="form-control"></date-time-input>
            </div>

            <div>
                Duration in seconds
                <input v-model.number="alert.duration" class="form-control" placeholder="Duration in seconds" />

                Quick set
                <select @input="alert.duration = $event.target.value" class="form-control">
                    <option :value="60 * 30">30 minutes</option>
                    <option :value="60 * 45">45 minutes</option>
                    <option :value="60 * 60">1 hour</option>
                </select>
            </div>

            <div>
                Zone
                <select v-model.number="alert.zoneID" class="form-control">
                    <option :value="0">Global</option>
                    <option :value="2">Indar</option>
                    <option :value="4">Hossin</option>
                    <option :value="6">Amerish</option>
                    <option :value="8">Esamir</option>
                    <option :value="344">Oshur</option>
                </select>
            </div>

            <div>
                World
                <select v-model.number="alert.worldID" class="form-control">
                    <option :value="1">Connery</option>
                    <option :value="10">Miller</option>
                    <option :value="13">Cobalt</option>
                    <option :value="17">Emerald</option>
                    <option :value="19">Jaeger</option>
                    <option :value="40">SolTech</option>
                </select>
            </div>

            <div>
                <button @click="create" type="button" class="btn btn-primary">
                    Create
                </button>
            </div>

            <div>
                {{alert.id}}
                <a :href="'/alert/' + alert.id">View</a>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";
    import ToggleButton from "components/ToggleButton";
    import DateTimeInput from "components/DateTimeInput.vue";

    import { AlertApi, PsAlert } from "api/AlertApi";

    export const AlertCreate = Vue.extend({
        props: {

        },

        data: function() {
            return {
                alert: new PsAlert() as PsAlert
            }
        },

        mounted: function(): void {
            this.alert.duration = 60 * 30;
            this.alert.worldID = 1;
        },

        methods: {
            create: async function(): Promise<void> {
                const response: Loading<number> = await AlertApi.insert(this.alert);
                if (response.state == "loaded") {
                    this.alert.id = response.data;
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover, Busy, Collapsible, ToggleButton,
            DateTimeInput
        }
    });
    export default AlertCreate;
</script>