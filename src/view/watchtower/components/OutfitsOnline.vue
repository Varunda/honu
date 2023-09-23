<template>
    <div v-if="data">
        <table class="wt-block table table-sm">
            <thead class="table-secondary">
                <tr class="th-border-top-0">
                    <td style="width: 30ch">Outfit</td>
                    <td>Currently online</td>
                    <td>Percent</td>
                </tr>
            </thead>

            <tbody>
                <tr v-for="entry in data.outfits">
                    <td>
                        <a v-if="entry.outfitID != '0'" :href="'/o/' + entry.outfitID">
                            {{entry.display}}
                        </a>

                        <span v-else>
                            {{entry.display}}
                        </span>
                    </td>
                    <td>{{entry.amountOnline}}</td>
                    <td>{{(entry.amountOnline / data.totalOnline * 100).toFixed(2)}}%</td>
                </tr>

                <tr class="table-secondary th-border-top-0">
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