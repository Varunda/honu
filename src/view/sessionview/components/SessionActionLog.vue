<template>
    <div>
        <div class="mb-3">
            <h5>
                Filters
                <button class="btn btn-sm btn-secondary btn-outline-light" @click="resetShow" title="Reset shown actions">Reset</button>
            </h5>
            <div class="btn-group mb-2">
                <toggle-button v-model="show.kills" class="flex-grow-0">Kills</toggle-button>
                <toggle-button v-model="show.deaths" class="flex-grow-0">Deaths</toggle-button>
                <toggle-button v-model="show.vehicleDestroy" class="flex-grow-0">Vehicle destroy</toggle-button>
                <toggle-button v-model="show.assists" class="flex-grow-0">Assists</toggle-button>
                <toggle-button v-model="show.revives" class="flex-grow-0">Revives</toggle-button>
                <toggle-button v-model="show.heals" class="flex-grow-0">Heals</toggle-button>
                <toggle-button v-model="show.shieldRepairs" class="flex-grow-0">Shield repairs</toggle-button>
                <toggle-button v-model="show.maxRepairs" class="flex-grow-0">MAX repairs</toggle-button>
                <toggle-button v-model="show.resupplies" class="flex-grow-0">Resupplies</toggle-button>
                <toggle-button v-model="show.expOther" class="flex-grow-0">Misc exp events</toggle-button>
                <toggle-button v-model="show.expNpc" class="flex-grow-0">Exp npc events</toggle-button>
                <toggle-button v-model="show.expSelf" class="flex-grow-0">Exp self events</toggle-button>
            </div>

            <h5>Events performed on the session&apos;s character</h5>
            <div class="btn-group">
                <toggle-button v-model="show.revived">Revived</toggle-button>
                <toggle-button v-model="show.healed">Healed</toggle-button>
                <toggle-button v-model="show.shieldRepaired">Shield repaired</toggle-button>
                <toggle-button v-model="show.resupplied">Resupplied</toggle-button>
                <toggle-button v-model="show.maxRepaired">MAX repaired</toggle-button>
            </div>
        </div>

        <div class="mb-3">
            <h5>Options</h5>
            <toggle-button v-model="options.useNpcID">Show NPC ID</toggle-button>
            <toggle-button v-model="options.combineSupport">Combine similar</toggle-button>
            <toggle-button v-model="options.showDebug">Show debug</toggle-button>
        </div>

        <div v-if="assistScoreMult != 1" class="bg-warning text-center">
            Score multipler is {{assistScoreMult.toFixed(2)}}, not 1. This may produce incorrect results for what percent of damage for assists
        </div>

        <a-table
            :entries="tableActions"
            default-sort-column="timestamp" default-sort-order="asc"
            :paginate="false" row-padding="compact" display-type="table" 
            :striped="false" :show-filters="true" :hover="true">

            <a-col>
                <a-header>
                    <b>Type</b>
                </a-header>

                <a-filter method="dropdown" field="type" type="string" :source="tableActionSource"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span v-if="typeNames.has(entry.type)">
                        {{typeNames.get(entry.type)}}
                    </span>

                    <span v-else>
                        {{entry.type}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Action</b>
                    <span class="text-muted">
                        [seconds since previous event]
                    </span>
                </a-header>

                <a-body v-slot="entry">
                    <span v-html="entry.parts.map(iter => iter.html).join(' ')">

                    </span>

                    <span v-if="entry.count && entry.count > 1">
                        ({{entry.count}} times)
                    </span>

                    <span v-if="entry.diff != undefined" class="text-muted">
                        [{{entry.diff}}s]
                    </span>

                    <div v-if="options.showDebug">
                        <span v-if="entry.event == undefined">
                            --
                        </span>
                        <span v-else class="text-monospace" style="overflow-wrap: break-word">
                            {{JSON.stringify(entry.event)}}
                        </span>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="timestamp">
                <a-header>
                    <b>Timestamp</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | moment("YYYY-MM-DD hh:mm:ssA")}}
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { ExpandedExpEvent, Experience, ExperienceBlock, ExperienceType, ExpEvent } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent } from "api/VehicleDestroyEventApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";
    import { PsVehicle, VehicleApi } from "api/VehicleApi";

    import ZoneUtils from "util/Zone";
    import TimeUtils from "util/Time";
    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";
    import UserStorageUtil from "util/UserStorage";

    type LogPart = {
        html: string;
    };

    type ActionLogEntry = {
        type: string;
        timestamp: Date;
        parts: LogPart[];
        count?: number;
        otherID?: string;

        // when filtered and sorted, how many seconds in between actions
        diff?: number;

        event?: object;
    };

    type ZonedEvent = {
        zoneID: number;
        timestamp: Date;
    }

    export const SessionActionLog = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            deaths: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            teamkills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            exp: { type: Object as PropType<ExperienceBlock>, required: true },
            ExpOther: { type: Object as PropType<ExperienceBlock>, required: true },
            VehicleDestroy: { type: Array as PropType<ExpandedVehicleDestroyEvent[]>, required: true },
        },

        data: function() {
            return {
                entries: [] as ActionLogEntry[],

                typeNames: new Map([
                    ["start", "Session start"],
                    ["finish", "Session end"],
                    ["zone_change", "Change continent"],

                    ["kill", "Kill"],
                    ["death", "Death"],
                    ["assist", "Assist"],
                    ["vehicle_destroy", "Vehicle destroy"],

                    ["revive", "Revive"],
                    ["heal", "Heal"],
                    ["shield_repair", "Shield repair"],

                    ["resupply", "Resupply"],
                    ["max_repair", "MAX repair"],

                    // session character is other_id
                    ["revived", "Revived"],
                    ["healed", "Healed"],
                    ["shield_repaired", "Shield repair"],
                    ["resupplied", "Resupplied"],
                    ["max_repaired", "MAX repaired"],

                    ["exp_other", "Experience (other)"],
                    ["exp_npc", "Experience (NPC)"],
                    ["exp_self", "Experience (self)"],

                ]) as Map<string, string>,

                assistScoreMult: 1 as number,

                options: {
                    useNpcID: false as boolean,
                    combineSupport: true as boolean,
                    showDebug: false as boolean
                },

                show: {
                    kills: true as boolean,
                    assists: false as boolean,
                    deaths: true as boolean,
                    vehicleDestroy: true as boolean,

                    revives: true as boolean,
                    heals: false as boolean,
                    shieldRepairs: false as boolean,
                    resupplies: false as boolean,
                    maxRepairs: false as boolean,

                    healed: false as boolean,
                    revived: true as boolean,
                    shieldRepaired: false as boolean,
                    resupplied: false as boolean,
                    maxRepaired: false as boolean,

                    expOther: false as boolean,
                    expNpc: false as boolean,
                    expSelf: false as boolean
                },

                vehicles: Loadable.idle() as Loading<PsVehicle[]>
            }
        },

        created: function(): void {
            this.loadFromStorage();
            this.setAll();
        },

        methods: {
            loadFromStorage: function(): void {
                if (UserStorageUtil.available() == false) {
                    return;
                }

                const actions: any | null = UserStorageUtil.get<object>("ActionLog.Actions");
                console.log("actions from storage", actions);

                if (actions == null) {
                    return;
                }

                for (const key of Object.keys(this.show)) {
                    console.log("setting action view", key, actions[key]);
                    (this.show as any)[key] = actions[key] == true;
                }
            },

            saveToStorage: function(): void {
                if (UserStorageUtil.available() == false) {
                    return;
                }

                console.log(`saving shown actions to storage`, JSON.stringify(this.show));
                UserStorageUtil.set("ActionLog.Actions", this.show);
            },

            resetShow: function(): void {
                this.show = {
                    kills: true as boolean,
                    assists: false as boolean,
                    deaths: true as boolean,
                    vehicleDestroy: true as boolean,

                    revives: true as boolean,
                    heals: false as boolean,
                    shieldRepairs: false as boolean,
                    resupplies: false as boolean,
                    maxRepairs: false as boolean,

                    healed: false as boolean,
                    revived: true as boolean,
                    shieldRepaired: false as boolean,
                    resupplied: false as boolean,
                    maxRepaired: false as boolean,

                    expOther: false as boolean,
                    expNpc: false as boolean,
                    expSelf: false as boolean
                };
            },

            bindVehicles: async function(): Promise<void> {
                this.vehicles = Loadable.loading();
                this.vehicles = await VehicleApi.getAll();
            },

            setAll: async function(): Promise<void> {
                this.entries = await this.makeAll();
            },

            makeAll: async function(): Promise<ActionLogEntry[]> {
                console.time("SessionActionLog> make action entries");
                if (this.vehicles.state == "idle") {
                    await this.bindVehicles();
                }

                const entries: ActionLogEntry[] = [];

                entries.push(...this.makeKills());
                entries.push(...this.makeDeaths());
                entries.push(...this.makeTeamkills());
                entries.push(...this.makeExp());
                entries.push(...this.makeOtherExp());
                entries.push(...this.makeZoneChange());
                entries.push(...this.makeVehicleDestroy());

                // Add the session start and end
                entries.push({
                    type: "start",
                    parts: [{ html: "<b>Session started</b>" }],
                    timestamp: new Date(this.session.start.getTime() - 50) // If the session started because of an event, have the session start before that event
                });

                if (this.session.end != null) {
                    const end: Date = this.session.end ?? new Date();

                    entries.push({
                        type: "finish",
                        parts: [{ html: `<b>Session finished</b> - lasted ${TimeUtils.duration((end.getTime() - this.session.start.getTime()) / 1000)}` }],
                        timestamp: this.session.end ?? new Date()
                    });
                } else {
                    entries.push({
                        type: "finish",
                        parts: [{ html: `<span class="text-warning">Session has not ended</span>` }],
                        timestamp: this.session.end ?? new Date()
                    });
                }

                entries.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                console.timeEnd("SessionActionLog> make action entries");
                return entries;
            },

            makeKills: function(): ActionLogEntry[] {
                return this.makeDeathEventEntries(this.kills, true);
            },

            makeTeamkills: function(): ActionLogEntry[] {
                return this.makeDeathEventEntries(this.teamkills, true);
            },

            makeDeaths: function(): ActionLogEntry[] {
                return this.makeDeathEventEntries(this.deaths, false);
            },

            /**
             * Make action log entries for kills and deaths
             * @param events Events to make the log entries of
             * @param asKill Where these kill/deaths a kill for the character of the session, or deaths?
             */
            makeDeathEventEntries: function(events: ExpandedKillEvent[], asKill: boolean): ActionLogEntry[] {
                if (this.vehicles.state != "loaded") {
                    console.warn(`SessionActionLog> vehicles isn't loaded`);
                }

                let lastKill: number | null = null;

                const entries: ActionLogEntry[] = [];
                for (const iter of events) {
                    const entry: ActionLogEntry = {
                        parts: [
                            this.createLoadoutIcon(iter.event.attackerLoadoutID),
                            this.createCharacterLink(iter.attacker, iter.event.attackerCharacterID),
                            { html: (iter.event.attackerTeamID != iter.event.killedTeamID) ? `killed` : `teamkilled` },
                            this.createLoadoutIcon(iter.event.killedLoadoutID),
                            this.createCharacterLink(iter.killed, iter.event.killedCharacterID),
                            { html: `using a` },
                            { html: (iter.event.weaponID == 0) ? "no weapon" : this.createLink(iter.item?.name ?? `&lt;missing ${iter.event.weaponID}&gt;`, `/i/${iter.event.weaponID}`) },
                        ],
                        timestamp: iter.event.timestamp,
                        type: (asKill == true) ? "kill" : "death",
                        count: 1,
                        otherID: (asKill == true) ? iter.event.killedCharacterID : iter.event.attackerCharacterID,
                        event: iter.event
                    };

                    if (iter.event.attackerVehicleID != 0 && this.vehicles.state == "loaded") {
                        const v: PsVehicle | null = this.vehicles.data.find(i => i.id == iter.event.attackerVehicleID) ?? null;
                        entry.parts.push({
                            html: `in a ${v?.name ?? `unknown vehicle ${iter.event.attackerVehicleID}`}`
                        });
                    }

                    if (iter.event.isHeadshot == true) {
                        entry.parts.push({
                            html: `(headshot)`
                        });
                    }

                    lastKill = iter.event.timestamp.getTime();

                    entries.push(entry);
                }

                return entries;
            },

            /**
             * Make action log entries for experience events
             */
            makeExp: function(): ActionLogEntry[] {
                const entries: ActionLogEntry[] = [];

                let scoreMult: number = 1;

                for (const ev of this.exp.events) {
                    const expID: number = ev.experienceID;

                    let baseAmount: number | null = null;

                    switch (expID) {
                        case Experience.REVIVE: baseAmount = 75; break;
                        case Experience.SQUAD_REVIVE: baseAmount = 100; break;

                        case Experience.RESUPPLY: baseAmount = 10; break;
                        case Experience.SQUAD_RESUPPLY: baseAmount = 15; break;

                        case Experience.SQUAD_SPAWN: baseAmount = 10; break;
                    }

                    if (baseAmount != null) {
                        scoreMult = ev.amount / baseAmount;
                    }
                }

                this.assistScoreMult = scoreMult;

                console.log(`SessionActionLog> using a score mult of ${scoreMult.toFixed(2)}`);

                let prev: ActionLogEntry | null = null;

                const typeMap: Map<number, ExperienceType> = new Map(this.exp.experienceTypes.map(iter => [iter.id, iter]));

                for (let i = 0; i < this.exp.events.length; ++i) {
                    const iter: ExpEvent = this.exp.events[i];

                    const expID: number = iter.experienceID;
                    const expType: ExperienceType | undefined = typeMap.get(expID);
                    const expDesc: string = expType?.name ?? `unknown ${expID}`;

                    let type: string = `Other (${expType?.name ?? `unknown ${expID}`})`;

                    const source: PsCharacter | null = this.exp.characters.find(c => c.id == iter.sourceID) || null;
                    const other: PsCharacter | null = this.exp.characters.find(c => c.id == iter.otherID) || null;

                    const parts: LogPart[] = [
                        this.createLoadoutIcon(iter.loadoutID),
                        this.createCharacterLink(source, iter.sourceID)
                    ];

                    if (Experience.isKill(expID) || expID == Experience.HEADSHOT) {
                        // skip exp for kill events, we have a Kill event for it
                        // same with headshot, already shown in a Kill event
                        continue;
                    } else if (Experience.isMaxRepair(expID)) {
                        type = "max_repair";
                        const amount: number = iter.amount / Math.max(1, scoreMult) * 25;

                        parts.push({ html: `repaired` });
                        parts.push(this.createCharacterLink(other, iter.otherID, { possessive: true }));
                        parts.push(this.createLogText(`MAX suit for ${amount}-${amount + 25} health`));
                    } else if (Experience.isShieldRepair(expID)) {
                        type = "shield_repaired";
                        const amount: number = iter.amount / Math.max(1, scoreMult) * 10;

                        parts.push({ html: `repaired` });
                        parts.push(this.createCharacterLink(other, iter.otherID, { possessive: true }));
                        parts.push(this.createLogText(`shield for ${amount}-${amount + 10} shield`));
                    } else if (Experience.isAssist(expID)) {
                        type = "assist";

                        // For assist events, the amount of score gained is relative to the % of damage dealt (i think)
                        let baseAssistAmount: number = 0;

                        switch (expID) {
                            case Experience.ASSIST: baseAssistAmount = 100; break;
                            case Experience.PRIORITY_ASSIST: baseAssistAmount = 150; break;
                            case Experience.HIGH_PRIORITY_ASSIST: baseAssistAmount = 300; break;
                            case Experience.SPAWN_ASSIST: break;
                        }

                        parts.push(this.createLogText("assisted in killing"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                        parts.push({
                            html: `(${((iter.amount / scoreMult) / Math.max(1, baseAssistAmount) * 100).toFixed(0)}% of damage)`
                        });
                    } else if (Experience.isHeal(expID)) {
                        type = "heal";

                        parts.push(this.createLogText("healed"));
                        parts.push(this.createCharacterLink(other, iter.otherID));

                        const amount: number = iter.amount / Math.max(1, scoreMult) * 10;
                        parts.push(this.createLogText(`for ${amount}-${amount + 10} health`));
                    } else if (Experience.isRevive(expID)) {
                        type = "revive";

                        parts.push(this.createLogText("revived"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                    } else if (Experience.isResupply(expID)) {
                        type = "resupply";

                        parts.push(this.createLogText("resupplied"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                    } else if (iter.otherID.length == 19) {
                        type = "exp_other";

                        parts.push(this.createLogText("interacted with"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                        parts.push(this.createLogText(`to earn ${expDesc}`));
                    } else if (iter.otherID.length > 0 && iter.otherID != "0") {
                        type = "exp_npc";

                        if (this.options.useNpcID == true) {
                            parts.push(this.createLogText(`interacted with NPC ${iter.otherID} to earn ${expDesc}`));
                        } else {
                            parts.push(this.createLogText(`interacted with an NPC to earn ${expDesc}`));
                        }
                    } else {
                        type = "exp_self";

                        parts.push(this.createLogText(`earned ${expDesc}`));
                    }

                    // if the previous action log entry added is the same type and otherID, instead increment the count
                    if (this.options.combineSupport == true && prev != null && prev.type == type && prev.otherID != null && prev.otherID == iter.otherID) {
                        prev.count = (prev.count ?? 1) + 1;
                    } else {
                        const entry: ActionLogEntry = {
                            parts: parts,
                            timestamp: iter.timestamp,
                            type: type,
                            count: 1,
                            otherID: iter.otherID,
                            event: iter
                        };

                        entries.push(entry);
                        prev = entry;
                    }
                }

                return entries;
            },

            makeOtherExp: function(): ActionLogEntry[] {
                const entries: ActionLogEntry[] = [];

                let scoreMult: number = 1;

                // TODO: need this seperate for each character??
                for (const ev of this.ExpOther.events) {
                    const expID: number = ev.experienceID;

                    let baseAmount: number | null = null;

                    switch (expID) {
                        case Experience.REVIVE: baseAmount = 75; break;
                        case Experience.SQUAD_REVIVE: baseAmount = 100; break;

                        case Experience.RESUPPLY: baseAmount = 10; break;
                        case Experience.SQUAD_RESUPPLY: baseAmount = 15; break;

                        case Experience.SQUAD_SPAWN: baseAmount = 10; break;
                    }

                    if (baseAmount != null) {
                        scoreMult = ev.amount / baseAmount;
                    }
                }

                this.assistScoreMult = scoreMult;

                console.log(`SessionActionLog> Using a score mult of ${scoreMult.toFixed(2)}`);

                let prev: ActionLogEntry | null = null;

                const typeMap: Map<number, ExperienceType> = new Map(this.exp.experienceTypes.map(iter => [iter.id, iter]));

                for (let i = 0; i < this.ExpOther.events.length; ++i) {
                    const iter: ExpEvent = this.ExpOther.events[i];

                    const expID: number = iter.experienceID;
                    const expType: ExperienceType | undefined = typeMap.get(expID);

                    let type: string = `Other (${expType?.name ?? `unknown ${expID}`})`;

                    const source: PsCharacter | null = this.ExpOther.characters.find(c => c.id == iter.sourceID) || null;
                    const other: PsCharacter | null = this.ExpOther.characters.find(c => c.id == iter.otherID) || null;

                    const parts: LogPart[] = [
                        this.createLoadoutIcon(iter.loadoutID),
                        this.createCharacterLink(source, iter.sourceID)
                    ];

                    if (Experience.isKill(expID) || expID == Experience.HEADSHOT) {
                        // skip exp for kill events, we have a Kill event for it
                        // same with headshot, already shown in a Kill event
                        continue;
                    } else if (Experience.isMaxRepair(expID)) {
                        type = "max_repaired";
                        const amount: number = iter.amount / Math.max(1, scoreMult) * 25;

                        parts.push({ html: `repaired` });
                        parts.push(this.createCharacterLink(other, iter.otherID, { possessive: true }));
                        parts.push(this.createLogText(`MAX suit for ${amount}-${amount + 25} health`));
                    } else if (Experience.isShieldRepair(expID)) {
                        type = "shield_repaired";
                        const amount: number = iter.amount / Math.max(1, scoreMult) * 10;

                        parts.push({ html: `repaired` });
                        parts.push(this.createCharacterLink(other, iter.otherID, { possessive: true }));
                        parts.push(this.createLogText(`shield for ${amount}-${amount + 10} shield`));
                    } else if (Experience.isHeal(expID)) {
                        type = "healed";
                        const amount: number = iter.amount / Math.max(1, scoreMult) * 10;

                        parts.push(this.createLogText("healed"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                        parts.push(this.createLogText(`for ${amount}-${amount + 10} health`));
                    } else if (Experience.isRevive(expID)) {
                        type = "revived";

                        parts.push(this.createLogText("revived"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                    } else if (Experience.isResupply(expID)) {
                        type = "resupplied";

                        parts.push(this.createLogText("resupplied"));
                        parts.push(this.createCharacterLink(other, iter.otherID));
                    }

                    // if the previous action log entry added is the same type and otherID, instead increment the count
                    if (this.options.combineSupport == true && prev != null && prev.type == type && prev.otherID != null && prev.otherID == iter.otherID) {
                        prev.count = (prev.count ?? 1) + 1;
                    } else {
                        const entry: ActionLogEntry = {
                            parts: parts,
                            timestamp: iter.timestamp,
                            type: type,
                            count: 1,
                            otherID: iter.otherID,
                            event: iter
                        };

                        entries.push(entry);
                        prev = entry;
                    }
                }

                return entries;
            },

            /**
             * Make log entries for vehicle kills/deaths
             */
            makeVehicleDestroy: function(): ActionLogEntry[] {
                const entries: ActionLogEntry[] = this.VehicleDestroy.map(iter => {
                    const killedColor: string = ColorUtils.getFactionColor(iter.killed?.factionID ?? iter.event.killedFactionID);

                    const entry: ActionLogEntry = {
                        parts: [
                            this.createLoadoutIcon(iter.event.attackerLoadoutID),
                            this.createCharacterLink(iter.attacker, iter.event.attackerCharacterID),
                            this.createLogText(`destroyed`),
                            this.createCharacterLink(iter.killed, iter.event.killedCharacterID, { possessive: true }),
                            this.createLogText(`${iter.killedVehicle?.name ?? `&lt;missing vehicle ${iter.event.killedVehicleID}&gt;`}`),
                            this.createLogText(`using the`),
                            this.createLogText((iter.event.attackerVehicleID == "0") ? "": `${iter.attackerVehicle?.name ?? `&lt;missing vehicle ${iter.event.attackerVehicleID}&gt;`}'s`),
                            { html: (iter.event.attackerWeaponID == 0) ? "no weapon" : this.createLink(iter.item?.name ?? `&lt;missing ${iter.event.attackerWeaponID}&gt;`, `/i/${iter.event.attackerWeaponID}`) },
                        ],
                        timestamp: iter.event.timestamp,
                        type: "vehicle_destroy",
                        event: iter.event
                    };

                    return entry;
                });

                return entries;
            },

            /**
             * Make the log entries for continent changes
             */
            makeZoneChange: function(): ActionLogEntry[] {
                const entries: ActionLogEntry[] = [];

                const zonedEvents: ZonedEvent[] = [];

                zonedEvents.push(...this.kills.map((iter) => {
                    return { zoneID: iter.event.zoneID, timestamp: iter.event.timestamp };
                }));

                zonedEvents.push(...this.deaths.map((iter) => {
                    return { zoneID: iter.event.zoneID, timestamp: iter.event.timestamp };
                }));

                zonedEvents.push(...this.exp.events.map((iter) => {
                    return { zoneID: iter.zoneID, timestamp: iter.timestamp };
                }));

                zonedEvents.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                let currentZoneID: number = -1;

                for (const ev of zonedEvents) {
                    if (ev.zoneID != currentZoneID) {
                        entries.push({
                            type: "zone_change",
                            // Since events are created on a zone change, and then the actual event is included in the action list,
                            //      the timestamps would be the same. To ensure a zone change shows up before the event that caused it,
                            //      subtract 50ms off that event (since all other events are in increments of 1 second)
                            timestamp: new Date(ev.timestamp.getTime() - 50),
                            parts: [
                                { html: `Changed continents to ${ZoneUtils.getZoneName(ev.zoneID)}` }
                            ],
                            event: ev
                        });

                        currentZoneID = ev.zoneID;
                    }
                }

                return entries;
            },

            /**
             * Create a text part with color
             * @param text Text to display
             * @param color Color of the text. Leave undefined for default color
             */
            createLogText: function(text: string, color?: string): LogPart {
                return { html: `<span${(color != undefined ? ` style="color: ${color}"` : ``)}>${text}</span>` };
            },

            /**
             * Create a link with color and text
             * @param name What the link will be displayed as
             * @param link What URL the link links to
             * @param color What color to display the link. Leave undefined for default color
             */
            createLink: function(name: string, link: string, color?: string): string {
                return `<a href="${link}" ${(color != undefined ? `style="color:${color};"` : "")}>${name}</a>`;
            },

            /**
             * Create a LogPart that links to a character
             * @param c PsCharacter, used for the faction ID
             * @param id ID of the character
             * @param options Optional options about how to display the link, such as will an 's be included?
             */
            createCharacterLink: function(c: PsCharacter | null, id: string, options?: { possessive?: boolean }): LogPart {
                if (id == "0") {
                    return { html: "no one" + (options?.possessive == true ? "'s" : "") };
                }

                let text: string = this.getCharacterName(c, id);
                if (options && options.possessive) {
                    text += "'s";
                }

                return {
                    html: this.createLink(text, `/c/${id}`, (c != null) ? ColorUtils.getFactionColor(c.factionID) : undefined)
                }
            },

            /**
             * Get the name of a loadout/class
             * @param loadoutID ID of the loadout
             */
            createLoadoutName: function(loadoutID: number): LogPart {
                return this.createLogText(`${LoadoutUtils.getLoadoutName(loadoutID)}`);
            },

            /**
             * Create a 16 pixel icon based on the loadout ID of a class
             * @param loadoutID ID of the loadout
             */
            createLoadoutIcon: function(loadoutID: number): LogPart {
                if (loadoutID == 0) {
                    return { html: "" };
                }

                const className: string = LoadoutUtils.getLoadoutName(loadoutID);
                let iconName: string = "";
                if (className == LoadoutUtils.NAME_INFILTRATOR) {
                    iconName = "icon_infil.png";
                } else if (className == LoadoutUtils.NAME_LIGHT_ASSAULT) {
                    iconName = "icon_light.png";
                } else if (className == LoadoutUtils.NAME_MEDIC) {
                    iconName = "icon_medic.png";
                } else if (className == LoadoutUtils.NAME_ENGINEER) {
                    iconName = "icon_engi.png";
                } else if (className == LoadoutUtils.NAME_HEAVY_ASSAULT) {
                    iconName = "icon_heavy.png";
                } else if (className == LoadoutUtils.NAME_MAX) {
                    iconName = "icon_max.png";
                } else {
                    console.warn(`SessionActionLog.createLoadoutIcon> Unchecked loadoutID ${loadoutID}`);
                    return { html: "" };
                }

                return {
                    html: `<img src="/img/classes/${iconName}" height="16" />`
                };
            },

            getCharacterName: function(c: PsCharacter | null, id: string): string {
                if (c == null) {
                    return `&lt;missing ${id}&gt;`;
                }

                const tag: string = c.outfitID != null ? `[${c.outfitTag}] ` : "";
                return `${tag}${c.name}`;
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            ToggleButton
        },

        computed: {
            tableActions: function(): Loading<ActionLogEntry[]> {
                console.log(`Showing table actions with the following SHOW options: ${JSON.stringify(this.show)}`);

                const actions = this.entries.filter(iter => {
                    // These are always shown
                    if (iter.type == "start" || iter.type == "finish" || iter.type == "zone_change") {
                        return true;
                    }

                    if (iter.type == "kill" && this.show.kills == true) { return true; }
                    if (iter.type == "death" && this.show.deaths == true) { return true; }
                    if (iter.type == "assist" && this.show.assists == true) { return true; }
                    if (iter.type == "vehicle_destroy" && this.show.vehicleDestroy == true) { return true; }

                    // session character is source
                    if (iter.type == "heal" && this.show.heals == true) { return true; }
                    if (iter.type == "shield_repair" && this.show.shieldRepairs == true) { return true; }
                    if (iter.type == "revive" && this.show.revives == true) { return true; }
                    if (iter.type == "resupply" && this.show.resupplies == true) { return true; }
                    if (iter.type == "max_repair" && this.show.maxRepairs == true) { return true; }

                    // session character is other_id
                    if (iter.type == "healed" && this.show.healed == true) { return true; }
                    if (iter.type == "revived" && this.show.revived == true) { return true; }
                    if (iter.type == "shield_repaired" && this.show.shieldRepaired == true) { return true; }
                    if (iter.type == "resupplied" && this.show.resupplied == true) { return true; }
                    if (iter.type == "max_repaired" && this.show.maxRepaired == true) { return true; }

                    // uncategorized
                    if (iter.type == "exp_other" && this.show.expOther == true) { return true; }
                    if (iter.type == "exp_npc" && this.show.expNpc) { return true; }
                    if (iter.type == "exp_self" && this.show.expSelf) { return true; }

                    return false;
                });

                actions.forEach((iter, index, arr) => {
                    if (index == 0) {
                        return;
                    }

                    const time = arr[index - 1].timestamp;
                    iter.diff = Math.floor((iter.timestamp.getTime() - time.getTime()) / 1000);
                });

                return Loadable.loaded(actions);
            },

            tableActionSource: function() {
                let options: any[] = [
                    { key: "All", value: null }
                ];

                this.typeNames.forEach((value: string, key: string) => {
                    // Confusing huh, this is correct tho. In an <a-table>, the key is what's displayed
                    options.push({ key: value, value: key });
                });

                return options;
            }
        },

        watch: {
            show: {
                deep: true,
                handler: function(): void {
                    this.saveToStorage();
                }
            },

            options: {
                deep: true,
                handler: function(): void {
                    this.saveToStorage();
                    this.setAll();
                }
            }
        }

    });
    export default SessionActionLog;
</script>