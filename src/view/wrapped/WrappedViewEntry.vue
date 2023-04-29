<template>
    <div>

        <div v-if="status == ''">
            starting connection...
        </div>

        <div v-else-if="status == 'started'">
            connected! started...
        </div>

        <div v-else-if="status == 'loading_static'">
            Loading static data...
        </div>

        <div v-else-if="status == 'loading_events'">
            Loading events...
        </div>

        <div v-else-if="status == 'done'">
            Loaded!

            <wrapped-view-header :wrapped="wrapped" class="mb-2"></wrapped-view-header>

            <wrapped-view-general :wrapped="wrapped" class="mb-2"></wrapped-view-general>

            <wrapped-view-character-interactions :wrapped="wrapped" class="mb-2"></wrapped-view-character-interactions>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";
    import { Loading, Loadable } from "Loading";

    import { WrappedApi, WrappedEntry } from "api/WrappedApi";
    import { KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExperienceType, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { FacilityControlEvent, FacilityControlEventApi } from "api/FacilityControlEventApi";
    import { ItemAddedEvent, ItemAddedEventApi } from "api/ItemAddedEventApi";
    import { VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";
    import { PsCharacter } from "api/CharacterApi";
    import { PsItem } from "api/ItemApi";
    import { AchievementEarned, AchievementEarnedApi } from "api/AchievementEarnedApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { Achievement } from "api/AchievementApi";
    import { PsFacility } from "api/FacilityApi";

    import WrappedViewHeader from "./components/WrappedViewHeader.vue";
    import WrappedViewGeneral from "./components/WrappedViewGeneral.vue";
    import WrappedViewCharacterInteractions from "./components/WrappedViewCharacterInteractions.vue";
import { FireGroupToFireMode } from "../../api/FireGroupToFireModeApi";
import { PsOutfit } from "../../api/OutfitApi";

    export const WrappedViewEntry = Vue.extend({
        props: {
            WrappedId: { type: String, required: true }
        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,

                status: "" as string,

                wrapped: new WrappedEntry() as WrappedEntry
            }
        },

        created: function(): void {
            this.connect();
            this.wrapped.id = this.WrappedId;
        },

        methods: {
            connect: async function(): Promise<void> {
                if (this.connection != null) {
                    this.connection.stop();
                    this.connection = null;
                }

                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/wrapped")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .configureLogging(sR.LogLevel.Information)
                    .build();

                this.connection.on("SendWrappedEntry", this.onSendWrappedEntry);
                this.connection.on("SendMessage", this.onSendMessage);
                this.connection.on("UpdateStatus", this.onUpdateStatus);

                this.connection.on("SendSessions", this.onSendSessions);
                this.connection.on("SendKills", this.onSendKills);
                this.connection.on("SendDeaths", this.onSendDeaths);
                this.connection.on("SendExp", this.onSendExp);
                this.connection.on("SendFacilityControl", this.onSendFacilityControl);
                this.connection.on("SendItemAdded", this.onSendItemAdded);
                this.connection.on("SendAchievementEarned", this.onSendAchievementEarned);
                this.connection.on("SendVehicleDestroy", this.onSendVehicleDestroy);

                this.connection.on("UpdateCharacters", this.onUpdateCharacters);
                this.connection.on("UpdateOutfits", this.onUpdateOutfits);
                this.connection.on("UpdateItems", this.onUpdateItems);
                this.connection.on("UpdateFacilities", this.onUpdateFacilities);
                this.connection.on("UpdateAchievements", this.onUpdateAchievements);
                this.connection.on("UpdateExpTypes", this.onUpdateExpTypes);
                this.connection.on("UpdateFireGroupToFireMode", this.onUpdateFireGroupXrefs);

                try {
                    await this.connection.start();
                } catch (err) {
                    console.error(`error on start: ${err}`);
                }

                this.connection.invoke("JoinGroup", this.WrappedId);
            },

            onSendWrappedEntry: function(entry: WrappedEntry): void {
                console.log(`wrapped parameters: ${JSON.stringify(entry)}`);
                this.wrapped.inputCharacterIDs = entry.inputCharacterIDs;
            },

            onSendMessage: function(msg: string): void {

            },

            onUpdateStatus: function(status: string): void {
                console.log(`new status: ${status}`);
                this.status = status;
            },

            onSendSessions: function(sessions: Session[]): void {
                for (const s of sessions) {
                    const session: Session = SessionApi.parse(s);
                    this.wrapped.sessions.push(session);
                }
            },

            onSendKills: function(events: KillEvent[]): void {
                for (const ev of events) {
                    const event: KillEvent = KillStatApi.parseKillEvent(ev);
                    this.wrapped.kills.push(event);
                }
            },

            onSendDeaths: function(events: KillEvent[]): void {
                for (const ev of events) {
                    const event: KillEvent = KillStatApi.parseKillEvent(ev);
                    this.wrapped.deaths.push(event);
                }
            },

            onSendExp: function(events: ExpEvent[]): void {
                for (const ev of events) {
                    const event: ExpEvent = ExpStatApi.parseExpEvent(ev);
                    this.wrapped.exp.push(event);
                }
            },

            onSendAchievementEarned: function(events: AchievementEarned[]): void {
                for (const ev of events) {
                    const event: AchievementEarned = AchievementEarnedApi.parse(ev);
                    this.wrapped.achievementEarned.push(event);
                }
            },

            onSendFacilityControl: function(events: FacilityControlEvent[]): void {
                for (const ev of events) {
                    const event: FacilityControlEvent = FacilityControlEventApi.parse(ev);
                    this.wrapped.controlEvents.push(event);
                }
            },

            onSendItemAdded: function(events: ItemAddedEvent[]): void {
                for (const ev of events) {
                    const event: ItemAddedEvent = ItemAddedEventApi.parse(ev);
                    this.wrapped.itemAdded.push(event);
                }
            },

            onSendVehicleDestroy: function(events: VehicleDestroyEvent[]): void {
                for (const ev of events) {
                    const event: VehicleDestroyEvent = VehicleDestroyEventApi.parse(ev);
                    this.wrapped.vehicleDestroy.push(event);
                }
            },

            onUpdateCharacters: function(chars: PsCharacter[]): void {
                for (const c of chars) {
                    this.wrapped.characters.set(c.id, c);
                }
            },

            onUpdateOutfits: function(outfits: PsOutfit[]): void {
                for (const o of outfits) {
                    this.wrapped.outfits.set(o.id, o);
                }
            },

            onUpdateItems: function(items: PsItem[]): void {
                for (const item of items) {
                    this.wrapped.items.set(item.id, item);
                }
            },

            onUpdateAchievements: function(achs: Achievement[]): void {

            },

            onUpdateExpTypes: function(expTypes: ExperienceType[]): void {

            },

            onUpdateFacilities: function(facs: PsFacility[]): void {

            },

            onUpdateFireGroupXrefs: function(refs: FireGroupToFireMode[]): void {
                for (const entry of refs) {
                    if (this.wrapped.fireModeXrefs.has(entry.fireModeID) == false) {
                        this.wrapped.fireModeXrefs.set(entry.fireModeID, []);
                    }

                    // force is safe, created above
                    this.wrapped.fireModeXrefs.get(entry.fireModeID)!.push(entry);
                }

                this.wrapped.fireModeXrefs.forEach((fireGroups: FireGroupToFireMode[], fireModeID: number) => {
                    if (fireGroups.length == 0) {
                        return;
                    }

                    const fireModeIndex: number = fireGroups[0].fireModeIndex;
                    for (const group of fireGroups) {
                        if (group.fireModeIndex != fireModeIndex) {
                            console.warn(`inconsistent fire mode index found for fire mode ${fireModeID}! group: ${JSON.stringify(group)}, base group: ${JSON.stringify(fireGroups[0])}`);
                        }
                    }
                });

                console.log(`got ${refs.length} thingies`);
            }

        },

        components: {
            WrappedViewHeader, WrappedViewGeneral, WrappedViewCharacterInteractions
        }
    });
    export default WrappedViewEntry;
</script>