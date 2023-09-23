<template>
    <table class="wt-block table table-sm">
        <thead>
            <tr class="table-secondary th-border-top-0">
                <th>Weapon</th>
                <th>Users</th>
                <th>Kills</th>
                <th>Percent</th>
                <th>HSR</th>
            </tr>
        </thead>

        <tbody>
            <tr v-for="entry in weaponKills.entries" :key="entry.itemID">
                <td :title="entry.name">
                    <a v-if="entry.itemID != 0" :href="'/i/' + entry.itemID">
                        {{entry.itemName}}
                    </a>

                    <span v-else>
                        no weapon
                    </span>
                </td>
                <td>
                    {{entry.users}} 
                </td>
                <td>
                    {{entry.kills}}
                </td>
                <td>
                    {{(entry.kills / total * 100).toFixed(2)}}%
                </td>
                <td>
                    {{(entry.headshotKills / entry.kills * 100).toFixed(2)}}%
                </td>
            </tr>

            <tr class="table-secondary th-border-top-0">
                <td colspan="4">Total</td>
                <td>{{total}}</td>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
    import Vue from "vue";

    export const WeaponKillsView = Vue.extend({
        props: {
            weaponKills: { required: true },
            total: { type: Number, required: true }
        },

        data: function () {
            return {

            }
        },
    });

    export default WeaponKillsView;
</script>