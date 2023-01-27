import { Loading, Loadable } from "Loading";

import { PopperModalData } from "popper/PopperModalData";
import EventBus from "EventBus";

import { PsAlert } from "api/AlertApi";
import { AlertParticipantApi, AlertPlayerProfileData, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
import { ExpandedExpEvent, ExpStatApi, Experience } from "api/ExpStatApi";
import { ExpandedKillEvent, KillStatApi } from "api/KillStatApi";
import { Session, SessionApi } from "api/SessionApi";

import ColorUtils from "util/Color";
import TimeUtils from "util/Time";
import CharacterUtils from "util/Character";
import ProfileUtils from "../../util/Profile";

export default class TableDataSource {

    /**
     * Open the sessions of a character that took place during an alert
     */
    public static async openCharacterSessions(event: { target: HTMLElement | null }, alert: PsAlert, characterID: string): Promise<void> {
        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = "Sessions";
        modalData.columnFields = [ "session", "start", "end", "duration" ];
        modalData.columnNames = [ "Session", "Start", "End", "Duration" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const data: Loading<Session[]> = await SessionApi.getByCharacterIDAndPeriod(characterID, alert.timestamp, alert.end);

        if (data.state == "loaded") {
            modalData.data = data.data.map(iter => {
                return {
                    session: iter.id,
                    start: TimeUtils.format(iter.start),
                    end: iter.end == null ? "<in progress>" : TimeUtils.format(iter.end),
                    duration: TimeUtils.duration(((iter.end ?? new Date()).getTime() - iter.start.getTime()) / 1000)
                }
            }).sort((a, b) => a.session - b.session);
        }

        modalData.renderers.set("session", (data: any): string => {
            return `<a href="/s/${data.session}">${data.session}</a>`;
        });

        modalData.loading = false;

        EventBus.$emit("set-modal-data", modalData);
    }

    /**
     * Open the weapons a character used over an alert
     */
    public static async openCharacterKills(event: any, alert: PsAlert, characterID: string): Promise<void> {
        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = "Weapons used";
        modalData.columnFields = [ "itemName", "kills", "headshotRatio", "percent" ];
        modalData.columnNames = [ "Weapon", "Kills", "Headshots", "Usage" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const data: Loading<ExpandedKillEvent[]> = await KillStatApi.getByRange(characterID, alert.timestamp, alert.end);
        if (data.state == "loaded") {
            const kills = data.data.filter(iter => iter.event.attackerCharacterID == characterID && iter.event.attackerTeamID != iter.event.killedTeamID && iter.event.zoneID == alert.zoneID);

            const weaponsUsed: number[] = kills.map(iter => iter.event.weaponID)
                .filter((v, i, a) => a.indexOf(v) == i);

            modalData.data = weaponsUsed.map(weaponID => {
                const itemKills: ExpandedKillEvent[] = kills.filter(iter => iter.event.weaponID == weaponID);

                if (itemKills.length == 0) {
                    throw `why is there 0 kills but we have a weapon ID for it`;
                }

                let itemName: string = itemKills[0].item?.name ?? `<unknown ${itemKills[0].event.weaponID}`;
                if (weaponID == 0) {
                    itemName = "<no weapon>";
                }

                const headshotKills: number = itemKills.filter(iter => iter.event.isHeadshot == true).length;

                return {
                    itemID: weaponID,
                    itemName: itemName,
                    kills: itemKills.length,
                    headshotRatio: `${(headshotKills / itemKills.length * 100).toFixed(2)}%`,
                    percent: `${(itemKills.length / kills.length * 100).toFixed(2)}%`
                }
            }).sort((a, b) => b.kills - a.kills);
        }

        modalData.renderers.set("itemName", (data: any): string => {
            if (data.itemID == 0) {
                return `<span>&lt;no weapon&gt;</span>`;
            }
            return `<a href="/i/${data.itemID}">${data.itemName}</a>`;
        });

        modalData.loading = false;

        EventBus.$emit("set-modal-data", modalData);
    }

    /**
     * Open the characters in an outfit and how many kills each player got
     */
    public static openOutfitKills(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string): void {
        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = "Player kills";
        modalData.columnFields = [ "characterName", "killsDisplay", "kpm", "secondsOnline" ];
        modalData.columnNames = [ "Name", "Kills", "KPM", "Time online" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const players: FlattendParticipantDataEntry[] = participants.filter(iter => iter.outfitID == outfitID);

        const totalKills: number = players.reduce((acc: number, iter) => acc += iter.kills, 0);

        modalData.data = players.map(iter => {
            return {
                characterID: iter.characterID,
                characterName: CharacterUtils.getDisplay({ ...iter, name: iter.characterName }),
                kills: iter.kills,
                killsDisplay: `${iter.kills} (${(iter.kills / totalKills * 100).toFixed(2)}%)`,
                kpm: `${iter.kpm.toFixed(2)}`,
                secondsOnline: TimeUtils.duration(iter.secondsOnline)
            };
        }).sort((a, b) => (b.kills - a.kills));

        modalData.renderers.set("characterName", (data: any): string => {
            return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
        });

        modalData.loading = false;

        EventBus.$emit("set-modal-data", modalData);
    }

    /**
     * Get the kills made by medics of an outfit during an alert
     */
    public static openOutfitKillsByProfile(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string, profileID: number): void {
        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = "Player kills";
        modalData.columnFields = [ "characterName", "killsDisplay", "kpm", "secondsOnline" ];
        modalData.columnNames = [ "Name", "Kills", "KPM", "Time online" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const players: FlattendParticipantDataEntry[] = participants.filter(iter => {
            if (iter.outfitID != outfitID) {
                return false;
            }
            const profile: AlertPlayerProfileData | undefined = iter.profiles.find(iter => iter.profileID == profileID);
            if (profile == undefined) {
                return false;
            }

            return profile.timeAs > 60;
        });

        // Force is safe cause of filter
        const totalKills: number = players.reduce((acc: number, iter) => acc += iter.profiles.find(iter => iter.profileID == profileID)!.kills, 0);

        modalData.data = players.map(iter => {
            const profile: AlertPlayerProfileData = iter.profiles.find(iter => iter.profileID == profileID)!;
            return {
                characterID: iter.characterID,
                characterName: CharacterUtils.getDisplay({ ...iter, name: iter.characterName }),
                kills: profile.kills,
                killsDisplay: `${profile.kills} (${(profile.kills / totalKills * 100).toFixed(2)}%)`,
                kpm: `${(profile.kills / profile.timeAs * 60).toFixed(2)}`,
                secondsOnline: TimeUtils.duration(profile.timeAs)
            };
        }).sort((a, b) => (b.kills - a.kills));

        modalData.renderers.set("characterName", (data: any): string => {
            return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
        });

        modalData.loading = false;

        EventBus.$emit("set-modal-data", modalData);
    }

    /**
     * Open with the players who healed in an outfit over an alert
     */
    public static openOutfitMedicHeals(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string): void {
        this.openOutfitGeneric(
            event, alert, participants, outfitID, "Player heals", "Heals",
            (iter) => iter.heals, (iter) => iter.healsPerMinute, ProfileUtils.MEDIC
        );
    }

    /**
     * Open with the players who revived in an outfit over an alert
     */
    public static openOutfitMedicRevives(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string): void {
        this.openOutfitGeneric(
            event, alert, participants, outfitID, "Player revives", "Revives",
            (iter) => iter.revives, (iter) => iter.revivesPerMinute, ProfileUtils.MEDIC
        );
    }

    public static openOutfitEngiResupplies(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string): void {
        this.openOutfitGeneric(
            event, alert, participants, outfitID, "Player resupplies", "Resupplies",
            (iter) => iter.resupplies, (iter) => iter.resuppliesPerMinute, ProfileUtils.ENGINEER
        );
    }

    public static openOutfitEngiRepairs(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string): void {
        this.openOutfitGeneric(
            event, alert, participants, outfitID, "Player resupplies", "Resupplies",
            (iter) => iter.repairs, (iter) => iter.repairsPerMinute, ProfileUtils.ENGINEER
        );
    }

    public static openOutfitGeneric(event: any, alert: PsAlert, participants: FlattendParticipantDataEntry[], outfitID: string,
            name: string, valueName: string,
            valueSelector: (i: FlattendParticipantDataEntry) => number,
            perMinuteSelector: (i: FlattendParticipantDataEntry) => number,
            profileID: number): void {

        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = name;
        modalData.columnFields = [ "characterName", "display", "perMinute", "timeAs" ];
        modalData.columnNames = [ "Name", valueName, "Per min", "Time" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const players: FlattendParticipantDataEntry[] = participants.filter(iter => iter.outfitID == outfitID && valueSelector(iter) > 0);

        const totalHeals: number = players.reduce((acc: number, iter) => acc += valueSelector(iter), 0);

        modalData.data = players.map(iter => {
            const profile: AlertPlayerProfileData | undefined = iter.profiles.find(iter => iter.profileID == profileID);
            return {
                characterID: iter.characterID,
                characterName: CharacterUtils.getDisplay({ ...iter, name: iter.characterName }),
                value: valueSelector(iter),
                perMinute: perMinuteSelector(iter).toFixed(2),
                display: `${valueSelector(iter)} (${(valueSelector(iter) / totalHeals * 100).toFixed(2)}%)`,
                timeAs: TimeUtils.duration(profile?.timeAs ?? 0)
            };
        }).sort((a, b) => (b.value - a.value));

        modalData.renderers.set("characterName", (data: any): string => {
            return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
        });

        modalData.loading = false;

        EventBus.$emit("set-modal-data", modalData);
    }

    /**
     * Open the characters a player healed over an alert
     */
    public static openCharacterHeals(event: any, alert: PsAlert, characterID: string): Promise<void> {
        return this.openCharacterGenericSupport(event, alert, characterID, [Experience.HEAL, Experience.SQUAD_HEAL], "Players healed");
    }

    /**
     * Open the characters a player revived over an alert
     */
    public static openCharacterRevives(event: any, alert: PsAlert, characterID: string): Promise<void> {
        return this.openCharacterGenericSupport(event, alert, characterID, [Experience.REVIVE, Experience.SQUAD_REVIVE], "Players revived");
    }

    /**
     * Open the characters a player resupplied over an alert
     */
    public static openCharacterResupplies(event: any, alert: PsAlert, characterID: string): Promise<void> {
        return this.openCharacterGenericSupport(event, alert, characterID, [Experience.RESUPPLY, Experience.SQUAD_RESUPPLY], "Players resupplied");
    }

    /**
     * Open the characters a player repaired over an alert
     */
    public static openCharacterRepairs(event: any, alert: PsAlert, characterID: string): Promise<void> {
        return this.openCharacterGenericSupport(event, alert, characterID, [Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR], "Players repaired");
    }

    /**
     * Open a popper modal generically for characters supported by specific exp events
     * @param event
     * @param alert
     * @param characterID
     * @param ids
     * @param name
     */
    public static async openCharacterGenericSupport(event: any, alert: PsAlert, characterID: string, ids: number[], name: string): Promise<void> {
        const modalData: PopperModalData = new PopperModalData();
        modalData.root = event.target;
        modalData.title = name;
        modalData.columnFields = [ "characterName", "amount", "percent" ];
        modalData.columnNames = [ "Character", "Amount", "Percent" ];
        modalData.loading = true;

        EventBus.$emit("set-modal-data", modalData);

        const expEvents: Loading<ExpandedExpEvent[]> = await ExpStatApi.getByCharacterIDAndRange(characterID, alert.timestamp, alert.end);
        if (expEvents.state == "loaded") {
            const events: ExpandedExpEvent[] = expEvents.data.filter(iter => {
                // allow all events in a daily alert
                return ids.indexOf(iter.event.experienceID) > -1 && (alert.zoneID == 0 || iter.event.zoneID == alert.zoneID);
            });

            const supportedChars: string[] = events.map(iter => iter.event.otherID).filter((v, i, a) => a.indexOf(v) == i);

            modalData.data = supportedChars.map((characterID: string) => {
                const charEvents: ExpandedExpEvent[] = events.filter(iter => iter.event.otherID == characterID);
                if (charEvents.length == 0) { throw `how does ${characterID} have 0 support events`; }

                return {
                    characterID: charEvents[0].event.otherID,
                    characterName: charEvents[0].other != null ? CharacterUtils.getDisplay(charEvents[0].other) : `<missing ${charEvents[0].event.otherID}>`,
                    amount: charEvents.length,
                    percent: `${(charEvents.length / events.length * 100).toFixed(2)}%`
                };
            }).sort((a, b) => b.amount - a.amount);
        }

        modalData.renderers.set("characterName", (data: any): string => {
            return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
        });

        modalData.loading = false;
        EventBus.$emit("set-modal-data", modalData);
    }


}