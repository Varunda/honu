<template>
    <div style="height: 40vh; overflow: auto;">
        <table class="table w-100 table-sticky-header" >
            <thead>
                <tr>
                    <th>Timestamp</th>
                    <th>Facility</th>
                    <th>Action</th>
                    <th>Outfit</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="ev in events">
                    <td>
                        {{ev.event.timestamp | moment}}
                    </td>
                    <td>
                        <span v-if="ev.facility != null">
                            {{ev.facility.name}}
                        </span>
                    </td>
                    <td>
                        <faction :faction-id="ev.event.newFactionID"></faction>
                        <span v-if="ev.event.newFactionID != ev.event.oldFactionID">
                            captured from <faction :faction-id="ev.event.oldFactionID"></faction>
                        </span>
                        <span v-else>
                            defended
                        </span>
                        with

                        <a @click="openOutfits($event, ev.event.id)">
                            {{ev.event.players}}
                        </a>

                        players
                    </td>
                    <td>
                        <span v-if="ev.event.oldFactionID != ev.event.newFactionID">
                            <span v-if="ev.outfit == null">
                                &lt;no outfit&gt;
                            </span>
                            <span v-else>
                                <a :href="'/o/' + ev.event.outfitID">
                                    <span v-if="ev.outfit.tag != null">
                                        [{{ev.outfit.tag}}]
                                    </span>
                                    {{ev.outfit.name}}
                                </a>
                            </span>
                        </span>
                        <span v-else>
                            --
                        </span>
                    </td>
                </tr>
            </tbody>
        </table>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading } from "Loading";

    import { ExpandedFacilityControlEvent } from "api/FacilityControlEventApi";
    import { ExpandedPlayerControlEvent, PlayerControlEventApi } from "api/PlayerControlEventApi";
    import { PsCharacter } from "api/CharacterApi";

    import { PopperModalData } from "popper/PopperModalData";
    import EventBus from "EventBus";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import Faction from "components/Faction";

    import CharacterUtil from "util/Character";

    import "MomentFilter";
    import "filters/FactionNameFilter";

    export const AlertControlEvents = Vue.extend({
        props: {
            control: { type: Array as PropType<ExpandedFacilityControlEvent[]>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            openOutfits: async function(event: any, controlID: number): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Outfits";
                modalData.columnFields = [ "outfitDisplay", "amount" ];
                modalData.columnNames = [ "Outfit", "Players" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const players: Loading<ExpandedPlayerControlEvent[]> = await PlayerControlEventApi.getByEventID(controlID);

                if (players.state == "loaded") {
                    const outfitIDs: string[] = players.data.filter(iter => iter.character != null)
                        .map(iter => iter.character!.outfitID ?? "0") // already checked if null above ^
                        .filter((v, i, a) => a.indexOf(v) == i);

                    modalData.data = outfitIDs.map(outfitID => {
                        const characters: PsCharacter[] = players.data.filter(iter => iter.character != null && (iter.character.outfitID ?? "0") == outfitID)
                            .map(iter => iter.character!);

                        if (characters.length == 0) {
                            throw `why are there no characters for outfit ${outfitID}`;
                        }

                        return {
                            outfitID: outfitID,
                            outfitDisplay: (outfitID == "0") ? "no outfit" : `[${characters[0].outfitTag}] ${characters[0].outfitName}`,
                            amount: characters.length
                        };
                    }).sort((a, b) => b.amount - a.amount);
                }

                modalData.renderers.set("outfitDisplay", (data: any): string => {
                    if (data.outfitID == 0) {
                        return `<span>no outfit</span>`
                    }
                    return `<a href="/o/${data.outfitID}">${data.outfitDisplay}</a>`;
                });

                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            }
        },

        computed: {
            events: function(): ExpandedFacilityControlEvent[] {
                return this.control.filter(iter => iter.event.oldFactionID != iter.event.newFactionID);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            Faction
        }
    });
    export default AlertControlEvents;
</script>