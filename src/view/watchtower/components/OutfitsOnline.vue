<template>
    <div v-if="data">
        <table class="wt-block table table-sm">
            <thead class="table-secondary">
                <tr>
                    <td style="width: 30ch">Outfit</td>
                    <td>Currently online</td>
                    <td>Percent</td>
                </tr>
            </thead>

            <tbody>
                <tr v-for="entry in data.outfits">
                    <td>{{entry.display}}</td>
                    <td>{{entry.amountOnline}}</td>
                    <td>{{(entry.amountOnline / data.totalOnline * 100).toFixed(2)}}%</td>
                </tr>

                <tr class="table-secondary">
                    <td colspan="2">Total</td>
                    <td>
                        {{data.totalOnline}}	
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    export const OutfitsOnline = Vue.extend({
        props: {
            data: { required: true },
        },
    });

    export default OutfitsOnline;
</script>