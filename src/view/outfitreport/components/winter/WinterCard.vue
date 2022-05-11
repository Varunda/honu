<template>
    <div class="card mr-3 mb-3 bg-transparent border" style="max-width: min(20%, 390px)">
        <div class="card-header">
            <h5>
                {{ShowFunName ? card.funName : card.name}}

                <info-hover v-if="card.availableAfter != null" :text="'Data not tracked before ' + card.availableAfter"></info-hover>
            </h5>
            <h6 class="mb-0">
                {{card.description}}
            </h6>
        </div>

        <div class="list-group list-group-flush flex-grow-1">
            <div v-for="entry in card.entries.slice(0, size)" class="list-group-item border-top bg-transparent">
                <a :href="'/c/' + entry.characterID">
                    {{entry.name}}
                </a>
                - {{entry.display != null ? entry.display : entry.value}}
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import InfoHover from "components/InfoHover.vue";

    export const WinterCard = Vue.extend({
        props: {
            card: { type: Object, required: true },
            ShowFunName: { type: Boolean, required: false, default: false },
            size: { type: Number, required: false, default: 5 }
        },

        components: {
            InfoHover
        }
    });
    export default WinterCard;
</script>