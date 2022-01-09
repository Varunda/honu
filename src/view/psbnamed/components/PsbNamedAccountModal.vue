<template>
    <div class="modal-dialog modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Title</h5>

                <button type="button" class="close" data-dismiss-modal>
                    &times;
                </button>
            </div>

            <div class="modal-body">
                <div class="mb-2">
                    <h3 class="wt-header">Player info</h3>

                    <table class="table table-sm">
                        <tr>
                            <th>Discord</th>
                            <td></td>
                        </tr>

                        <tr>
                            <th>Discord ID</th>
                            <td></td>
                        </tr>
                    </table>
                </div>

                <div>
                    <h3 class="wt-header">Characters</h3>
                    <div class="d-flex">
                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>VS</h4>
                            <psb-named-character-column :character="account.vsCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>NC</h4>
                            <psb-named-character-column :character="account.ncCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>TR</h4>
                            <psb-named-character-column :character="account.trCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1">
                            <h4>NS / Do not use</h4>
                            <psb-named-character-column :character="account.nsCharacter"></psb-named-character-column>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import PsbNamedCharacterColumn from "./PsbNamedCharacterColumn.vue";

    import { FlatPsbNamedAccount } from "api/PsbNamedAccountApi";

    export const PsbNamedAccountModal = Vue.extend({
        props: {
            account: { type: Object as PropType<FlatPsbNamedAccount>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {
            PsbNamedCharacterColumn
        }
    });
    export default PsbNamedAccountModal;
</script>