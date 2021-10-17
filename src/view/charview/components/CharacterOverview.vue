<template>
    <div>
        <div>
            Character ID: {{character.id}}

            <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/character?character_id=' + character.id" target="_blank">
                Census
                <span class="fas fa-external-link-alt"></span>
            </a>
        </div>

        <div>
            Name: {{character.name}}
        </div>

        <div>
            Outfit and link
        </div>

        <div>
            Battle rank:
            <span v-if="character.prestige == true" title="A.S.P">1~</span>{{character.battleRank}}
        </div>

        <div>
            Class playtime:
        </div>

        <div>
            Vehicle playtime:
        </div>

        <div>
            Score and SPM
        </div>

        <div>
            heavy WIP 
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsCharacter } from "api/CharacterApi";

    export const CharacterOverview = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        }

    });
    export default CharacterOverview;
</script>
