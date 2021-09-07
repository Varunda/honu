<template>
    <span v-if="metadata != null">
        <span v-if="metadata.isOpened == false">
            <span class="fas fa-lock" title="This continent is locked"></span>
        </span>

        <span v-if="metadata.alertEnd != null">
            <span class="fas fa-exclamation-triangle" title="Active alert!"></span>
            Locks
            {{metadata.alertEnd | til}}
        </span>
    </span>
</template>

<script lang="ts">
    import Vue from "vue";

    export const ContinentMetadata = Vue.extend({
        props: {
            metadata: { required: false }
        }
    });
    export default ContinentMetadata;
</script>