<template>
    <div class="card flex-grow-0 flex-basis-1">
        <div class="card-body p-0">
            <h3 class="card-title bg-secondary text-center py-1">{{data.name}}</h3>

            <h5 class="card-text px-1">
                <span v-if="data.totalDisplay != null">
                    Total: {{data.totalDisplay}}
                </span>
                <span v-else>
                    Total: {{data.total}}
                </span>
            </h5>

            <table class="table table-sm">
                <thead class="table-secondary">
                    <tr class="th-border-top-0 px-2">
                        <th>{{HeaderLeft}}</th>
                        <th>{{HeaderRight}}</th>
                    </tr>
                </thead>

                <tbody>
                    <tr v-for="datum in data.data" class="px-2">
                        <td>
                            <a v-if="datum.link != null" :href="datum.link">
                                {{datum.display}}
                            </a>
                            <span v-else>
                                {{datum.display}}
                            </span>
                        </td>
                        <td>
                            {{datum.value}}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { WrappedSimpleData, WrappedSimpleEntry } from "./simple";

    export const WrappedSimpleCard = Vue.extend({
        props: {
            data: { type: Object as PropType<WrappedSimpleData>, required: true },
            HeaderLeft: { type: String, required: false, default: "Character" },
            HeaderRight: { type: String, required: false, default: "Amount" }
        },

        methods: {

        },

    });
    export default WrappedSimpleCard;
</script>