<template>
    <div>
        <collapsible header-text="Input characters">
            <div>
                This wrapped was generated with {{wrapped.inputCharacterIDs.length}} characters
            </div>

            <div class="mb-2">
                <h4>Characters</h4>

                <template v-for="charID of wrapped.inputCharacterIDs">
                    <wrapped-character-button :character-id="charID" :character="wrapped.characters.get(charID)" class="mr-2"
                        @update-filter-character-add="addCharacter" @update-filter-character-remove="removeCharacter">
                    </wrapped-character-button>
                </template>
            </div>

            <!--
                
            <div class="mb-2">
                <h4>Class to include</h4>

                <div class="btn-group">
                    <toggle-button v-model="filters.class.infil">
                        <img src="/img/classes/icon_infil.png" height="16" />
                        Infiltrator
                    </toggle-button>
                    <toggle-button v-model="filters.class.lightAssault">
                        <img src="/img/classes/icon_light.png" height="16" />
                        Light Assault
                    </toggle-button>
                    <toggle-button v-model="filters.class.medic">
                        <img src="/img/classes/icon_medic.png" height="16" />
                        Combat Medic
                    </toggle-button>
                    <toggle-button v-model="filters.class.engi">
                        <img src="/img/classes/icon_engi.png" height="16" />
                        Engineer
                    </toggle-button>
                    <toggle-button v-model="filters.class.heavy">
                        <img src="/img/classes/icon_heavy.png" height="16" />
                        Heavy Assault
                    </toggle-button>
                    <toggle-button v-model="filters.class.max">
                        <img src="/img/classes/icon_max.png" height="16" />
                        MAX
                    </toggle-button>
                </div>

                <div>
                    {{includedClasses}}
                </div>
            </div>

            <div class="mb-2">
                <button class="btn btn-primary" @click="update">
                    Update
                </button>
            </div>
            -->

        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";

    // models
    import { WrappedFilters } from "../common";
    import { PsCharacter } from "api/CharacterApi";

    // components
    import ToggleButton from "components/ToggleButton";
    import Collapsible from "components/Collapsible.vue";

    const WrappedCharacterButton = Vue.extend({
        props: {
            CharacterId: { type: String, required: true },
            character: { type: Object as PropType<PsCharacter | null> }
        },

        data: function() {
            return {
                toggled: true as boolean
            }
        },

        template: `
            <button class="btn mr-2" :class="[ toggled ? 'btn-primary' : 'btn-secondary' ]" @click="clickToggle">
                <span v-if="character == null">
                    &lt;missing {{CharacterId}}&gt;
                </span>

                <span v-else>
                    <span v-if="character.outfitID != null">
                        [{{character.outfitTag}}]
                    </span>

                    {{character.name}}
                </span>
            </button>
        `,

        methods: {
            clickToggle: function(): void {
                //this.toggled = !this.toggled;

                /*
                if (this.toggled == true) {
                    this.$emit("update-filter-character-add", this.CharacterId);
                } else {
                    this.$emit("update-filter-character-remove", this.CharacterId);
                }
                */
            },
        },

        components: {
            ToggleButton
        }
    });

    export const WrappedViewHeader = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                filters: new WrappedFilters() as WrappedFilters
            }
        },

        mounted: function(): void {
            for (const charID of this.wrapped.inputCharacterIDs) {
                this.filters.characters.push(charID);
            }
        },

        methods: {
            update: function(): void {
                this.$emit("update-filters", this.filters);
            },

            addCharacter: function(charID: string): void {
                this.filters.characters.push(charID);
                console.log(`added ${charID} to filter list: [${this.filters.characters.join(", ")}]`);
            },

            removeCharacter: function(charID: string): void {
                this.filters.characters = this.filters.characters.filter(iter => iter != charID);
                console.log(`removed ${charID} to input list: [${this.filters.characters.join(", ")}]`);
            }
        },

        computed: {
            includedClasses: function(): string {
                if (this.filters.class.infil == true
                    && this.filters.class.lightAssault == true
                    && this.filters.class.medic == true
                    && this.filters.class.engi == true
                    && this.filters.class.heavy == true
                    && this.filters.class.max == true) {

                    return "Showing events from all classes";
                }

                const showedClasses: string[] = [];
                if (this.filters.class.infil == true) { showedClasses.push("Infiltrator"); }
                if (this.filters.class.lightAssault == true) { showedClasses.push("Light Assault"); }
                if (this.filters.class.medic == true) { showedClasses.push("Combat Medic"); }
                if (this.filters.class.engi == true) { showedClasses.push("Engineer"); }
                if (this.filters.class.heavy == true) { showedClasses.push("Heavy Assault"); }
                if (this.filters.class.max == true) { showedClasses.push("MAX"); }

                return `Showing events from: ${showedClasses.join(", ")}`;
            }

        },

        components: {
            Collapsible,
            WrappedCharacterButton,
            ToggleButton
        }

    });
    export default WrappedViewHeader;
</script>