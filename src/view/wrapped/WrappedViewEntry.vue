<template>
    <div>

        <div v-if="status == ''">
            starting connection...
        </div>

        <div v-else-if="status == 'started'">
            <busy class="honu-busy honu-busy-lg"></busy>
            Processing characters...
        </div>

        <div v-else-if="status == 'pending_creation'">
            This wrapped is in queue to be processed
            <busy class="honu-busy honu-busy-sm"></busy>
            <p>
                {{queue.position + 1}} / {{queue.total}}
            </p>
        </div>

        <div v-else-if="status == 'loading_input_characters'">
            Loading input characters...
            <busy class="honu-busy honu-busy-sm"></busy>
        </div>

        <div v-else-if="status == 'loading_events'">
            <busy class="honu-busy honu-busy-lg"></busy>
            Loading events...

            <div>
                Kills: 
                <span v-if="steps.kills == true">
                    Loaded {{wrapped.kills.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Deaths: 
                <span v-if="steps.deaths == true">
                    Loaded {{wrapped.deaths.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Exp: 
                <span v-if="steps.exp == true">
                    Loaded {{wrapped.exp.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Vehicle kills: 
                <span v-if="steps.vehicleDestroy == true">
                    Loaded {{wrapped.vehicleKill.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Vehicle deaths:
                <span v-if="steps.vehicleDestroy == true">
                    Loaded {{wrapped.vehicleDeath.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Facility control: 
                <span v-if="steps.deaths == true">
                    Loaded {{wrapped.deaths.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Achievement earned: 
                <span v-if="steps.achievementEarned == true">
                    Loaded {{wrapped.achievementEarned.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>
            <div>
                Item added: 
                <span v-if="steps.itemAdded == true">
                    Loaded {{wrapped.itemAdded.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Sessions: 
                <span v-if="steps.sessions == true">
                    Loaded {{wrapped.sessions.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

        </div>

        <div v-else-if="status == 'loading_static'">
            <busy class="honu-busy honu-busy-lg"></busy>
            Loading static data...

            <div>
                Characters: 
                <span v-if="steps.characters == true">
                    Loaded {{wrapped.characters.size}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Outfits: 
                <span v-if="steps.outfits == true">
                    Loaded {{wrapped.outfits.size}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Items: 
                <span v-if="steps.items == true">
                    Loaded {{wrapped.items.size}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Facilities: 
                <span v-if="steps.facilities == true">
                    Loaded {{wrapped.facilities.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>

            <div>
                Experience types: 
                <span v-if="steps.expTypes == true">
                    Loaded {{wrapped.expTypes.length}}
                </span>
                <span v-else>
                    <busy class="honu-busy honu-busy-sm"></busy>
                </span>
            </div>
        </div>

        <div v-else-if="status == 'building_data'">
            <busy class="honu-busy honu-busy-lg"></busy>
            Generating data...
        </div>

        <div v-else-if="status == 'done'">
            <div class="mb-2 w-100">
                <button v-if="showFull == false" @click="showFull = true" class="btn btn-primary w-100">
                    Show all (laggy and resource intensive!)
                </button>
                <button v-else @click="showFull = false" class="btn btn-secondary w-100">
                    Show highlights
                </button>
            </div>

            <div v-if="showFull == false" class="d-flex" style="gap: 1rem;">

                <wrapped-simple-card title="Playtime" :data="simple.playtime" header-right="Playtime"></wrapped-simple-card>

                <wrapped-simple-card title="Kills" :data="simple.kills" header-right="Kills"></wrapped-simple-card>

                <!--
                    <wrapped-view-highlight :wrapped="filteredWrapped"></wrapped-view-highlight>
                -->
            </div>

            <div v-else-if="showFull == true">
                <wrapped-view-header :wrapped="filteredWrapped" @update-filters="updateFilters" class="mb-2"></wrapped-view-header>

                <wrapped-view-general :wrapped="filteredWrapped" class="mb-2"></wrapped-view-general>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--red), var(--yellow))"></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-character-interactions :wrapped="filteredWrapped"></wrapped-view-character-interactions>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--blue), var(--purple))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-classes :wrapped="filteredWrapped"></wrapped-view-classes>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--green), var(--yellow))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-weapons :wrapped="filteredWrapped"></wrapped-view-weapons>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--orange), var(--blue))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-exp :wrapped="filteredWrapped"></wrapped-view-exp>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--purple), var(--red))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-vehicle :wrapped="filteredWrapped"></wrapped-view-vehicle>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--purple), var(--red))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-facility :wrapped="filteredWrapped"></wrapped-view-facility>
                    </div>
                </div>

                <div class="d-flex flex-row w-100 mb-3" style="">
                    <div class="mb-1" style="width: 1rem; flex-grow: 0; background-image: linear-gradient(to bottom, var(--yellow), var(--purple))" ></div>

                    <div style="flex-grow: 1;" class="ml-2">
                        <wrapped-view-sessions :wrapped="filteredWrapped"></wrapped-view-sessions>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    // general
    import Vue from "vue";
    import * as sR from "signalR";
    import { Loading, Loadable } from "Loading";

    // models
    import { WrappedExtraData, WrappedFilters } from "./common";
    import { WrappedSimpleData, WrappedSimpleEntry } from "./simple";

    import { WrappedApi, WrappedEntry } from "api/WrappedApi";
    import { KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExperienceType, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { FacilityControlEvent, FacilityControlEventApi } from "api/FacilityControlEventApi";
    import { ItemAddedEvent, ItemAddedEventApi } from "api/ItemAddedEventApi";
    import { VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";
    import { CharacterApi, PsCharacter } from "api/CharacterApi";
    import { PsItem } from "api/ItemApi";
    import { AchievementEarned, AchievementEarnedApi } from "api/AchievementEarnedApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { Achievement } from "api/AchievementApi";
    import { PsFacility } from "api/MapApi";
    import { FireGroupToFireMode } from "api/FireGroupToFireModeApi";
    import { PsOutfit } from "api/OutfitApi";
    import { PsVehicle } from "api/VehicleApi";

    import TimeUtils from "util/Time";
    import LocaleUtil from "util/Locale";
    import CharacterUtils from "util/Character";

    // components
    import Busy from "components/Busy.vue";

    // wrapped specific components
    import WrappedSimpleCard from "./WrappedSimpleCard.vue";
    import WrappedViewHeader from "./components/WrappedViewHeader.vue";
    import WrappedViewGeneral from "./components/WrappedViewGeneral.vue";
    import WrappedViewCharacterInteractions from "./components/WrappedViewCharacterInteractions.vue";
    import WrappedViewSessions from "./components/WrappedViewSessions.vue";
    import WrappedViewClasses from "./components/WrappedViewClasses.vue";
    import WrappedViewWeapons from "./components/WrappedViewWeapons.vue";
    import WrappedViewVehicle from "./components/WrappedViewVehicle.vue";
    import WrappedViewExp from "./components/WrappedViewExp.vue";
    import WrappedViewFacility from "./components/WrappedViewFacility.vue";
    import WrappedViewHighlight from "./components/WrappedViewHighlight.vue";

    const WRAPPED: WrappedEntry = new WrappedEntry();

    export const WrappedViewEntry = Vue.extend({
        props: {
            WrappedId: { type: String, required: true }
        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,

                status: "" as string,

                showFull: false as boolean,

                steps: {
                    kills: false as boolean,
                    deaths: false as boolean,
                    exp: false as boolean,
                    vehicleDestroy: false as boolean,
                    achievementEarned: false as boolean,
                    itemAdded: false as boolean,
                    sessions: false as boolean,

                    characters: false as boolean,
                    outfits: false as boolean,
                    items: false as boolean,
                    facilities: false as boolean,
                    expTypes: false as boolean,
                },

                simple: {
                    playtime: new WrappedSimpleData("Playtime", "character") as WrappedSimpleData,
                    kills: new WrappedSimpleData("Kills", "character") as WrappedSimpleData,

                    all: [] as WrappedSimpleData[]
                },

                queue: {
                    position: 0 as number,
                    total: 0 as number
                },

                wrapped: new WrappedEntry() as WrappedEntry,
                filters: new WrappedFilters() as WrappedFilters,
            }
        },

        created: function(): void {
            this.connect();
            WRAPPED.id = this.WrappedId;
        },

        methods: {
            updateFilters: function(filters: WrappedFilters): void {
                console.log(`updating filters: ${JSON.stringify(filters)}`);
                this.status = "building_data";
                this.$nextTick(() => {
                    this.filters = filters;
                    this.status = "done";
                });
            },

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
                this.connection.on("SendQueuePosition", this.onQueuePosition);

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
                this.connection.on("UpdateVehicles", this.onUpdateVehicles);

                try {
                    await this.connection.start();
                } catch (err) {
                    console.error(`error on start: ${err}`);
                }

                this.connection.invoke("JoinGroup", this.WrappedId).then(() => {
                    console.log("JoinGroup done");

                    if (this.showFull == false) {
                        this.passTwoSimpleMode();
                    } else {
                        this.wrapped = WRAPPED;
                    }
                });
            },

            onSendWrappedEntry: function(entry: WrappedEntry): void {
                console.log(`wrapped parameters: ${JSON.stringify(entry)}`);
                WRAPPED.inputCharacterIDs = entry.inputCharacterIDs;

                if (entry.status == 1) { // pending
                    console.log(`pending, hopefully get queue status`);
                } else if (entry.status == 2) { // in creation
                    console.log(`in creation!`);
                } else if (entry.status == 3) { // done!
                    console.log(`entry is done!`);
                }
            },

            onSendMessage: function(msg: string): void {
                console.log(`Message from Hub> ${msg}`);
            },

            onUpdateStatus: function(status: string): void {
                console.log(`new status: ${status}`);

                if (status == "done") {
                    console.log(`loaded ${this.WrappedId}`);
                    WRAPPED.extra = WrappedExtraData.build(WRAPPED);
                }

                this.status = status;
            },

            onSendSessions: function(sessions: Session[]): void {
                for (const s of sessions) {
                    const session: Session = SessionApi.parse(s);
                    WRAPPED.sessions.push(session);
                }

                /*
                const playTime: Map<string, number> = new Map();

                for (const s of sessions) {
                    const session: Session = SessionApi.parse(s);
                    if (session.end == null) {
                        continue;
                    }

                    const id: string = session.characterID;

                    const duration: number = (session.end.getTime() - session.start.getTime()) / 1000;

                    playTime.set(id, (playTime.get(id) ?? 0) + duration);
                    this.simple.playtime.total += duration;
                }

                const mostPlayed = Array.from(playTime.entries()).sort((a, b) => {
                    return b[1] - a[1];
                }).slice(0, 5);

                this.simple.playtime.totalDisplay = TimeUtils.duration(this.simple.playtime.total);
                this.simple.playtime.data = mostPlayed.map((iter) => {
                    const charID: string = iter[0];
                    const playtime: number = iter[1];

                    const datum: WrappedSimpleEntry = new WrappedSimpleEntry();
                    datum.id = charID;
                    datum.display = charID;
                    datum.link = `/c/${charID}`;
                    datum.value = TimeUtils.duration(playtime);

                    return datum;
                });
                */

                this.steps.sessions = true;
            },

            onSendKills: function(events: KillEvent[]): void {
                for (const ev of events) {
                    const event: KillEvent = KillStatApi.parseKillEvent(ev);

                    if (event.attackerTeamID == event.killedTeamID) {
                        WRAPPED.teamkills.push(event);
                    } else {
                        WRAPPED.kills.push(event);
                    }
                }

                /*
                    const kills: Map<string, number> = new Map();

                    for (const ev of events) {
                        const event: KillEvent = KillStatApi.parseKillEvent(ev);

                        if (event.attackerTeamID == event.killedTeamID) {

                        } else {
                            if (kills.has(event.killedCharacterID) == false) {
                                kills.set(event.killedCharacterID, 0);
                            }

                            kills.set(event.killedCharacterID, (kills.get(event.killedCharacterID) ?? 0) + 1);
                            this.simple.kills.total += 1;
                        }
                    }

                    const most = Array.from(kills.entries()).sort((a, b) => {
                        return b[1] - a[1];
                    }).slice(0, 5);

                    this.simple.kills.data = most.map((iter) => {
                        const charID: string = iter[0];
                        const kills: number = iter[1];

                        const datum: WrappedSimpleEntry = new WrappedSimpleEntry();
                        datum.id = charID;
                        datum.display = charID;
                        datum.link = `/c/${charID}`;
                        datum.value = LocaleUtil.locale(kills, 0);

                        return datum;
                    });
                */

                this.steps.kills = true;
            },

            onSendDeaths: function(events: KillEvent[]): void {
                for (const ev of events) {
                    const event: KillEvent = KillStatApi.parseKillEvent(ev);

                    if (event.attackerTeamID == event.killedTeamID) {
                        WRAPPED.teamdeaths.push(event);
                    } else {
                        WRAPPED.deaths.push(event);
                    }
                }
                this.steps.deaths = true;
            },

            onSendExp: function(events: ExpEvent[]): void {
                for (const ev of events) {
                    const event: ExpEvent = ExpStatApi.parseExpEvent(ev);
                    WRAPPED.exp.push(event);
                }
                this.steps.exp = true;
            },

            onSendAchievementEarned: function(events: AchievementEarned[]): void {
                for (const ev of events) {
                    const event: AchievementEarned = AchievementEarnedApi.parse(ev);
                    WRAPPED.achievementEarned.push(event);
                }
                this.steps.achievementEarned = true;
            },

            onSendFacilityControl: function(events: FacilityControlEvent[]): void {
                for (const ev of events) {
                    const event: FacilityControlEvent = FacilityControlEventApi.parse(ev);
                    WRAPPED.controlEvents.push(event);
                }
            },

            onSendItemAdded: function(events: ItemAddedEvent[]): void {
                for (const ev of events) {
                    const event: ItemAddedEvent = ItemAddedEventApi.parse(ev);
                    WRAPPED.itemAdded.push(event);
                }
                this.steps.itemAdded = true;
            },

            onSendVehicleDestroy: function(events: VehicleDestroyEvent[]): void {
                for (const ev of events) {
                    const event: VehicleDestroyEvent = VehicleDestroyEventApi.parse(ev);

                    if (WRAPPED.inputCharacterIDs.indexOf(ev.attackerCharacterID) > -1) {
                        WRAPPED.vehicleKill.push(event);
                    } else if (WRAPPED.inputCharacterIDs.indexOf(ev.killedCharacterID) > -1) {
                        WRAPPED.vehicleDeath.push(event);
                    }

                }
                this.steps.vehicleDestroy = true;
            },

            onUpdateCharacters: function(chars: PsCharacter[]): void {
                for (const c of chars) {
                    const character: PsCharacter = CharacterApi.parse(c);
                    WRAPPED.characters.set(c.id, character);
                }
                this.steps.characters = true;
            },

            onUpdateOutfits: function(outfits: PsOutfit[]): void {
                for (const o of outfits) {
                    WRAPPED.outfits.set(o.id, o);
                }
                this.steps.outfits = true;
            },

            onUpdateItems: function(items: PsItem[]): void {
                for (const item of items) {
                    WRAPPED.items.set(item.id, item);
                }
                this.steps.items = true;
            },

            onUpdateAchievements: function(achs: Achievement[]): void {
                for (const ach of achs) {
                    WRAPPED.achivements.set(ach.id, ach);
                }
            },

            onUpdateExpTypes: function(expTypes: ExperienceType[]): void {
                for (const expType of expTypes) {
                    WRAPPED.expTypes.set(expType.id, expType);
                }
            },

            onUpdateFacilities: function(facs: PsFacility[]): void {
                for (const f of facs) {
                    WRAPPED.facilities.set(f.facilityID, f);
                }
            },

            onUpdateFireGroupXrefs: function(refs: FireGroupToFireMode[]): void {
                for (const entry of refs) {
                    if (WRAPPED.fireModeXrefs.has(entry.fireModeID) == false) {
                        WRAPPED.fireModeXrefs.set(entry.fireModeID, []);
                    }

                    // force is safe, created above
                    WRAPPED.fireModeXrefs.get(entry.fireModeID)!.push(entry);
                }

                WRAPPED.fireModeXrefs.forEach((fireGroups: FireGroupToFireMode[], fireModeID: number) => {
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
            },

            onUpdateVehicles: function(vehs: PsVehicle[]): void {
                for (const v of vehs) {
                    WRAPPED.vehicles.set(v.id, v);
                }
            },

            onQueuePosition: function(position: number, total: number): void {
                console.log(`got queue position: ${position} / ${total}`);
                this.queue.position = position;
                this.queue.total = total;
            },

            passTwoSimpleMode: async function(): Promise<void> {
                console.log(`starting pass 2 for simple mode`);

                const simpleEntries: WrappedSimpleData[] = [
                    this.simple.playtime,
                    this.simple.kills,
                    ...this.simple.all
                ];

                console.log(`loading static data for ${simpleEntries.length} entries`);

                const charIDs: Set<string> = new Set();

                for (const simple of simpleEntries) {
                    console.log(`${simple.name} => ${simple.type}`);
                    if (simple.type == "character") {
                        simple.data.map(iter => iter.id).forEach(iter => charIDs.add(iter));
                    } else if (simple.type == "outfit") {

                    } else if (simple.type == "item") {

                    } else if (simple.type == "facility") {

                    } else if (simple.type == "none") {
                        continue;
                    } else {
                        throw `unchecked type ${simple.type}`;
                    }
                }

                console.log(`loading data for ${charIDs.size} characters...`);
                const characters: Loading<PsCharacter[]> = await CharacterApi.getByIDs(Array.from(charIDs));
                if (characters.state != "loaded") {
                    console.error(`failed to load characters in pass2`);
                    return;
                }

                const outfitIDs: Set<string> = new Set(characters.data.filter(iter => iter.outfitID != null).map(iter => iter.outfitID!));
                //const outfits: 

                console.log(`loaded ${characters.data.length} characters`);

                const dict: Map<string, PsCharacter> = new Map();
                characters.data.forEach(iter => dict.set(iter.id, iter));

                for (const simple of simpleEntries) {
                    if (simple.type != "character") {
                        continue;
                    }

                    for (const iter of simple.data) {
                        iter.display = CharacterUtils.display(iter.id, dict.get(iter.id));
                    }
                }

            }

        },

        computed: {
            filteredWrapped: function(): WrappedEntry {

                if (this.filters.class.infil == false
                    || this.filters.class.lightAssault == false
                    || this.filters.class.medic == false
                    || this.filters.class.engi == false
                    || this.filters.class.heavy == false
                    || this.filters.class.max == false) {

                }

                return WRAPPED;
            }
        },

        components: {
            Busy,
            WrappedSimpleCard,
            WrappedViewHeader, WrappedViewGeneral, WrappedViewCharacterInteractions, WrappedViewSessions, WrappedViewClasses, WrappedViewWeapons,
            WrappedViewVehicle, WrappedViewExp, WrappedViewFacility,
            WrappedViewHighlight
        }
    });
    export default WrappedViewEntry;
</script>