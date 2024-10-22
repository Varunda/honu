<template>
    <span v-if="metadata != null">
        <span v-if="metadata.isOpened == false">
            <span class="ph-bold ph-lock" title="This continent is locked"></span>
        </span>

        <span v-if="metadata.unstableState == 1">
            (single lane)
        </span>

        <span v-else-if="metadata.unstableState == 2">
            (double lane)
        </span>

        <span v-if="metadata.alertEnd != null">
            <span class="ph-bold ph-exclamation-mark" title="Active alert!"></span>

            <span v-if="metadata.alertInfo != null && metadata.alertInfo.typeID == 9">
                Locks in
            </span>
            <span v-else>
                Ends in
            </span>

            {{metadata.alertEnd | til2}}

            <a :href="'/alert/' + metadata.alert.id">
                <span v-if="metadata.alertInfo != null">
                    ({{metadata.alertInfo.name}})
                </span>
                <span v-else>
                    (--)
                </span>
            </a>
        </span>
    </span>
</template>

<script lang="ts">
    import Vue from "vue";

    import "filters/TilFilter";

    export const ContinentMetadata = Vue.extend({
        props: {
            metadata: { required: false }
        },
    });
    export default ContinentMetadata;
</script>