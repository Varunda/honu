<template>
    <div>
        <div>
            <h4>Filters</h4>
            <div class="btn-group w-100 mb-3">
                <toggle-button v-model="show.kills">Kills</toggle-button>
                <toggle-button v-model="show.deaths">Deaths</toggle-button>
                <toggle-button v-model="show.vehicleDestroy">Vehicle destroy</toggle-button>
                <toggle-button v-model="show.assists">Assists</toggle-button>
                <toggle-button v-model="show.revives">Revives</toggle-button>
                <toggle-button v-model="show.heals">Heals</toggle-button>
                <toggle-button v-model="show.shieldRepairs">Shield repairs</toggle-button>
                <toggle-button v-model="show.maxRepairs">MAX repairs</toggle-button>
                <toggle-button v-model="show.resupplies">Resupplies</toggle-button>
                <toggle-button v-model="show.expOther">Other exp events</toggle-button>
            </div>
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
                </a-header>

                <a-body v-slot="entry">
                    <span v-html="entry.parts.map(iter => iter.html).join(' ')">

                    </span>

                    <span v-if="entry.count > 1">
                        ({{entry.count}} times)
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="timestamp">
                <a-header>
                    <b>Timestamp</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | moment("YYYY-MM-DD hh:mm:ss A")}}
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
    import { ExpandedExpEvent, Experience } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent } from "api/VehicleDestroyEventApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";

    import ZoneUtils from "util/Zone";
    import TimeUtils from "util/Time";
    import ColorUtils from "util/Color";
    import LoadoutUtils from "util/Loadout";

    type LogPart = {
        html: string;
    };

    type ActionLogEntry = {
        type: string;
        timestamp: Date;
        parts: LogPart[];
        count: number;
        otherID: string | null;
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
            exp: { type: Array as PropType<ExpandedExpEvent[]>, required: true },
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

                    ["heal", "Heal"],
                    ["revive", "Revive"],
                    ["shield_repair", "Shield repair"],

                    ["resupply", "Resupply"],
                    ["max_repair", "MAX repair"],
                ]) as Map<string, string>,

                assistScoreMult: 1 as number,

                show: {
                    kills: true as boolean,
                    assists: false as boolean,
                    deaths: true as boolean,
                    revives: true as boolean,
                    heals: false as boolean,
                    shieldRepairs: false as boolean,
                    resupplies: false as boolean,
                    maxRepairs: false as boolean,
                    vehicleDestroy: true as boolean,
                    expOther: false as boolean
                }
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {
            makeAll: function(): void {
                this.makeKills();
                this.makeDeaths();
                this.makeExp();
                this.makeZoneChange();
                this.makeVehicleDestroy();

                // Add the session start and end
                this.entries.push({
                    type: "start",
                    parts: [{ html: "<b>Session started</b>" }],
                    timestamp: new Date(this.session.start.getTime() - 50), // If the session started because of an event, have the session start before that event
                    count: 1,
                    otherID: null
                });

                if (this.session.end != null) {
                    const end: Date = this.session.end ?? new Date();

                    this.entries.push({
                        type: "finish",
                        parts: [{ html: `<b>Session finished</b> - lasted ${TimeUtils.duration((end.getTime() - this.session.start.getTime()) / 1000)}` }],
                        timestamp: this.session.end ?? new Date(),
                        count: 1,
                        otherID: null
                    });
                } else {
                    this.entries.push({
                        type: "finish",
                        parts: [{ html: `<span class="text-warning">Session has not ended</span>` }],
                        timestamp: this.session.end ?? new Date(),
                        count: 1,
                        otherID: null
                    });
                }

                this.entries.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());
            },

            makeKills: function(): void {
                this.entries.push(...this.makeDeathEventEntries(this.kills, true));
            },

            makeDeaths: function(): void {
                this.entries.push(...this.makeDeathEventEntries(this.deaths, false));
            },

            makeDeathEventEntries: function(events: ExpandedKillEvent[], asKill: boolean): ActionLogEntry[] {
                const entries: ActionLogEntry[] = events.map(iter => {
                    const entry: ActionLogEntry = {
                        parts: [
                            this.createCharacterLink(iter.attacker, iter.event.attackerCharacterID),
                            { html: `killed` },
                            this.createCharacterLink(iter.killed, iter.event.killedCharacterID),
                            { html: `using` },
                            { html: (iter.event.weaponID == 0) ? "no weapon" : this.createLink(iter.item?.name ?? `&lt;missing ${iter.event.weaponID}&gt;`, `/i/${iter.event.weaponID}`) },
                            ((LoadoutUtils.isEngineer(iter.event.attackerLoadoutID) || LoadoutUtils.isInfiltrator(iter.event.attackerLoadoutID)) ? this.createLogText("as an") : this.createLogText("as a")),
                            this.createLoadoutName(iter.event.attackerLoadoutID)
                        ],
                        timestamp: iter.event.timestamp,
                        type: (asKill == true) ? "kill" : "death",
                        count: 1,
                        otherID: (asKill == true) ? iter.event.killedCharacterID : iter.event.attackerCharacterID
                    };

                    if (iter.event.isHeadshot == true) {
                        entry.parts.push({
                            html: `(headshot)`
                        });
                    }

                    return entry;
                });

                return entries;
            },

            makeExp: function(): void {
                let scoreMult: number = 1;

                for (const ev of this.exp) {
                    const expID: number = ev.event.experienceID;

                    let baseAmount: number | null = null;

                    switch (expID) {
                        //case Experience.HEAL: baseAmount = 10; break;
                        //case Experience.SQUAD_HEAL: baseAmount = 15; break;
                        case Experience.REVIVE: baseAmount = 75; break;
                        case Experience.SQUAD_REVIVE: baseAmount = 100; break;
                        //case Experience.SHIELD_REPAIR: baseAmount = 10; break;
                        //case Experience.SQUAD_SHIELD_REPAIR: baseAmount = 10; break;

                        //case Experience.MAX_REPAIR: baseAmount = 5; break;
                        //case Experience.SQUAD_MAX_REPAIR: baseAmount = 15; break;
                        case Experience.RESUPPLY: baseAmount = 10; break;
                        case Experience.SQUAD_RESUPPLY: baseAmount = 15; break;

                        case Experience.SQUAD_SPAWN: baseAmount = 10; break;
                    }

                    if (baseAmount != null) {
                        scoreMult = ev.event.amount / baseAmount;
                    }
                }

                this.assistScoreMult = scoreMult;

                console.log(`Using a score mult of ${scoreMult.toFixed(2)}`);

                let prev: ActionLogEntry | null = null;

                for (let i = 0; i < this.exp.length; ++i) {
                    const iter: ExpandedExpEvent = this.exp[i];

                    const expID: number = iter.event.experienceID;

                    let type: string = `other - experience ${expID}`;

                    const parts: LogPart[] = [
                        this.createCharacterLink(iter.source, iter.event.sourceID)
                    ];

                    if (Experience.isMaxRepair(expID)) {
                        parts.push({ html: `repaired` });
                        type = "max_repair";

                        parts.push({
                            html: `<a href="/c/${iter.event.otherID}">${this.getCharacterName(iter.other, iter.event.otherID)}</a>'s MAX suit`
                        });
                    } else if (Experience.isShieldRepair(expID)) {
                        parts.push({ html: `repaired` });
                        type = "shield_repair";

                        parts.push({
                            html: `<a href="/c/${iter.event.otherID}">${this.getCharacterName(iter.other, iter.event.otherID)}</a>'s shield`
                        });
                    } else if (Experience.isVehicleKill(expID)) {
                        /*
                        parts.push(this.createLogText("destroyed a"));
                        type = "vehicle_destroy";

                        let vehicleName: string = "unknown vehicle";

                        switch (expID) {
                            case Experience.VKILL_FLASH: vehicleName = "Flash"; break;
                            case Experience.VKILL_CHIMERA: vehicleName = "Chimera"; break;
                            case Experience.VKILL_COLOSSUS: vehicleName = "Colossus"; break;
                            case Experience.VKILL_DERVISH: vehicleName = "Dervish"; break;
                            case Experience.VKILL_ANT: vehicleName = "ANT"; break;
                            case Experience.VKILL_GALAXY: vehicleName = "Galaxy"; break;
                            case Experience.VKILL_HARASSER: vehicleName = "Harasser"; break;
                            case Experience.VKILL_JAVELIN: vehicleName = "Javline"; break;
                            case Experience.VKILL_LIBERATOR: vehicleName = "Liberator"; break;
                            case Experience.VKILL_LIGHTNING: vehicleName = "Lightning"; break;
                            case Experience.VKILL_MAGRIDER: vehicleName = "Magrider"; break;
                            case Experience.VKILL_MOSQUITO: vehicleName = "Mosquito"; break;
                            case Experience.VKILL_PROWLER: vehicleName = "Prowler"; break;
                            case Experience.VKILL_REAVER: vehicleName = "Reaver"; break;
                            case Experience.VKILL_SCYTHE: vehicleName = "Scythe"; break;
                            case Experience.VKILL_VALKYRIE: vehicleName = "Valkyrie"; break;
                            case Experience.VKILL_VANGUARD: vehicleName = "Vanguard"; break;
                        }

                        parts.push(this.createLogText(vehicleName));
                        */
                    } else {
                        let verb: string = "supported (generic)";

                        if (Experience.isHeal(expID)) {
                            type = "heal"; verb = "healed";
                        } else if (Experience.isRevive(expID)) {
                            type = "revive"; verb = "revived";
                        } else if (Experience.isAssist(expID)) {
                            type = "assist"; verb = "assisted in killing";
                        } else if (Experience.isResupply(expID)) {
                            type = "resupply"; verb = "resupplied";
                        }

                        parts.push(this.createLogText(verb));
                        parts.push(this.createCharacterLink(iter.other, iter.event.otherID));

                        // For assist events, the amount of score gained is relative to the % of damage dealt (i think)
                        if (Experience.isAssist(expID)) {
                            let baseAssistAmount: number = 0;

                            switch (expID) {
                                case Experience.ASSIST: baseAssistAmount = 100; break;
                                case Experience.PRIORITY_ASSIST: baseAssistAmount = 150; break;
                                case Experience.HIGH_PRIORITY_ASSIST: baseAssistAmount = 300; break;
                                case Experience.SPAWN_ASSIST: break;
                            }

                            parts.push({
                                html: `(${((iter.event.amount / scoreMult) / Math.max(1, baseAssistAmount) * 100).toFixed(0)}% of damage)`
                            });
                        }
                    }

                    parts.push(this.createLogText(`as a`));
                    parts.push(this.createLoadoutName(iter.event.loadoutID));

                    if (prev != null && prev.type == type && prev.otherID != null && prev.otherID == iter.event.otherID) {
                        ++prev.count;
                    } else {
                        const entry: ActionLogEntry = {
                            parts: parts,
                            timestamp: iter.event.timestamp,
                            type: type,
                            count: 1,
                            otherID: iter.event.otherID
                        };

                        this.entries.push(entry);
                        prev = entry;
                    }
                }
            },

            makeVehicleDestroy: function(): void {
                const entries: ActionLogEntry[] = this.VehicleDestroy.map(iter => {
                    const killedColor: string = ColorUtils.getFactionColor(iter.killed?.factionID ?? iter.event.killedFactionID);

                    const entry: ActionLogEntry = {
                        parts: [
                            this.createCharacterLink(iter.attacker, iter.event.attackerCharacterID),
                            this.createLogText(`destroyed`),
                            this.createLogText(`<a style="color: ${killedColor}" href="/c/${iter.event.killedCharacterID}">${this.getCharacterName(iter.killed, iter.event.killedCharacterID)}</a>'s`),
                            this.createLogText(`${iter.killedVehicle?.name ?? `&lt;missing vehicle ${iter.event.killedVehicleID}&gt;`}`),
                            this.createLogText(`using the`),
                            this.createLogText((iter.event.attackerVehicleID == "0") ? "": `${iter.attackerVehicle?.name ?? `&lt;missing vehicle ${iter.event.attackerVehicleID}&gt;`}'s`),
                            { html: (iter.event.attackerWeaponID == 0) ? "no weapon" : this.createLink(iter.item?.name ?? `&lt;missing ${iter.event.attackerWeaponID}&gt;`, `/i/${iter.event.attackerWeaponID}`) },
                            this.createLogText("as a"),
                            this.createLoadoutName(iter.event.attackerLoadoutID)
                        ],
                        timestamp: iter.event.timestamp,
                        type: "vehicle_destroy",
                        count: 1,
                        otherID: null
                    };

                    return entry;
                });

                this.entries.push(...entries);
            },

            makeZoneChange: function(): void {
                const zonedEvents: ZonedEvent[] = [];

                zonedEvents.push(...this.kills.map((iter) => {
                    return { zoneID: iter.event.zoneID, timestamp: iter.event.timestamp };
                }));

                zonedEvents.push(...this.deaths.map((iter) => {
                    return { zoneID: iter.event.zoneID, timestamp: iter.event.timestamp };
                }));

                zonedEvents.push(...this.exp.map((iter) => {
                    return { zoneID: iter.event.zoneID, timestamp: iter.event.timestamp };
                }));

                zonedEvents.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                let currentZoneID: number = -1;

                for (const ev of zonedEvents) {
                    if (ev.zoneID != currentZoneID) {
                        this.entries.push({
                            type: "zone_change",
                            // Since events are created on a zone change, and then the actual event is included in the action list,
                            //      the timestamps would be the same. To ensure a zone change shows up before the event that caused it,
                            //      subtract 50ms off that event (since all other events are in increments of 1 second)
                            timestamp: new Date(ev.timestamp.getTime() - 50),
                            parts: [
                                { html: `Changed continents to ${ZoneUtils.getZoneName(ev.zoneID)}` }
                            ],
                            count: 1,
                            otherID: null
                        });

                        currentZoneID = ev.zoneID;
                    }
                }
            },

            createLogText: function(text: string, color?: string): LogPart {
                return { html: `<span ${(color != undefined ? `style="color: ${color}"` : ``)}>${text}</span>` };
            },

            createLink: function(name: string, link: string, color?: string): string {
                return `<a href="${link}" ${(color != undefined ? `style="color:${color};"` : "")}>${name}</a>`;
            },

            createCharacterLink: function(c: PsCharacter | null, id: string): LogPart {
                return {
                    html: this.createLink(this.getCharacterName(c, id), `/c/${id}`, (c != null) ? ColorUtils.getFactionColor(c.factionID) : undefined)
                }
            },

            createLoadoutName: function(loadoutID: number): LogPart {
                return this.createLogText(`${LoadoutUtils.getLoadoutName(loadoutID)}`);
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

                return Loadable.loaded(this.entries.filter(iter => {
                    // These are always shown
                    if (iter.type == "start" || iter.type == "finish" || iter.type == "zone_change") {
                        return true;
                    }

                    if (iter.type == "kill" && this.show.kills == true) {
                        return true;
                    }

                    if (iter.type == "death" && this.show.deaths == true) {
                        return true;
                    }

                    if (iter.type == "assist" && this.show.assists == true) {
                        return true;
                    }

                    if (iter.type == "heal" && this.show.heals == true) {
                        return true;
                    }

                    if (iter.type == "revive" && this.show.revives == true) {
                        return true;
                    }

                    if (iter.type == "resupply" && this.show.resupplies == true) {
                        return true;
                    }

                    if (iter.type == "max_repair" && this.show.maxRepairs == true) {
                        return true;
                    }

                    if (iter.type == "shield_repair" && this.show.shieldRepairs == true) {
                        return true;
                    }

                    if (iter.type == "vehicle_destroy" && this.show.vehicleDestroy == true) {
                        return true;
                    }

                    if (iter.type.startsWith("other - ") && this.show.expOther == true) {
                        return true;
                    }

                    return false;
                }));
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

    });
    export default SessionActionLog;
</script>