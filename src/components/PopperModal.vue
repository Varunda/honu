<template>
    <div id="stat-table" style="display: none; background-color: var(--secondary); color: white; border: 2px var(--light) solid;">
        <div class="d-flex bg-dark" style="align-items: center;">
            <span class="flex-grow-1 px-2">
                {{modalData.title}}
            </span>

            <button type="button" class="btn flex-grow-0" @click="closeStatTooltip">
                &times;
            </button>
        </div>

        <div style="max-height: 30vh; overflow: auto">
            <table class="table table-sm table-striped mb-0 table-sticky-header">
                <thead>
                    <tr>
                        <th v-for="column in modalData.columnNames">
                            {{column}}
                        </th>
                    </tr>
                </thead>

                <tbody v-if="modalData.loading == false">
                    <tr v-for="datum in modalData.data">
                        <td v-for="field in modalData.columnFields">
                            <div v-if="modalData.renderers.has(field)" v-html="modalData.renderers.get(field)(datum)">

                            </div>
                            <div v-else>
                                {{datum[field]}}
                            </div>
                        </td>
                    </tr>
                </tbody>

                <tbody v-else>
                    <tr>
                        <td :colspan="modalData.columnNames.length">
                            Loading...
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { PopperModalData } from "popper/PopperModalData";

    import { createPopper, Instance } from "node_modules/@popperjs/core/lib/popper";

    export const PopperModal = Vue.extend({
        props: {
            value: { type: Object as PropType<PopperModalData>, required: true }
        },

        data: function() {
            return {
                modalData: new PopperModalData() as PopperModalData,
                popperInstance: null as Instance | null,
            }
        },

        methods: {
            setModalData: function (modalData: PopperModalData): void {
                this.modalData = modalData;

                if (this.modalData.root == null) {
                    console.error(`Missing root element`);
                    return;
                }

                const tooltip: HTMLElement | null = document.getElementById("stat-table");
                if (tooltip == null) {
                    console.error(`Missing tooltip element '#stat-table'`);
                    return;
                }

                tooltip.style.display = "block";

                if (this.popperInstance != null) {
                    this.popperInstance.destroy();
                    this.popperInstance = null;
                }

                console.log(`updating popper modal`);

                const popper: Instance = createPopper(this.modalData.root, tooltip, {
                    placement: "auto",
                });
                this.popperInstance = popper;

                // idk why this is needed and i can't be bothered to figure it out
                // the display value kept getting set to none instead of block
                tooltip.style.display = "block";
                this.$nextTick(() => {
                    tooltip.style.display = "block";
                });
            },

            closeStatTooltip: function(): void {
                console.log(`close stat tooltip`);
                if (this.popperInstance != null) {
                    this.popperInstance.destroy();

                    const tooltip: HTMLElement | null = document.getElementById("stat-table");
                    if (tooltip != null) {
                        tooltip.style.display = "none";
                    }
                }
            },
        },

        watch: {
            value: {
                deep: true,
                handler: function(): void {
                    this.setModalData(this.value);
                }
            }
        },

        computed: {

        },

        components: {

        }
    });

    export default PopperModal;
</script>