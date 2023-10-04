import { KillEvent } from "api/KillStatApi";
import { ExpEvent, ExperienceType, Experience } from "api/ExpStatApi";
import { PsItem } from "api/ItemApi";
import { PsOutfit } from "api/OutfitApi";
import { PsCharacter } from "api/CharacterApi";
import { Session } from "api/SessionApi";
import { FacilityControlEvent } from "api/FacilityControlEventApi";
import { PlayerControlEvent } from "api/PlayerControlEventApi";
import { PsFacility } from "api/MapApi";
import { RealtimeReconnectEntry } from "api/RealtimeReconnectApi";
import { ItemCategory } from "api/ItemCategoryApi";
import { VehicleDestroyEvent } from "api/VehicleDestroyEventApi";
import { FireGroupToFireMode } from "api/FireGroupToFireModeApi";

import LoadoutUtils from "util/Loadout";
import { InfantryDamageEntry } from "./InfantryDamage";

export class ReportParameters {
	public id: string = ""; // guid
	public timestamp: Date = new Date();
	public generator: string = "";
	public teamID: number = -1;
	public zoneID: number | null = null;
	public characterIDs: string[] = [];
	public outfitIDs: string[] = [];
	public ignoredCharacters: string[] = [];
	public periodStart: Date = new Date();
	public periodEnd: Date = new Date();
}

export default class Report {
	public parameters: ReportParameters = new ReportParameters();
	public reconnects: RealtimeReconnectEntry[] = [];

	public trackedCharacters: string[] = [];

	public kills: KillEvent[] = [];
	public deaths: KillEvent[] = [];
	public experience: ExpEvent[] = [];
	public vehicleDestroy: VehicleDestroyEvent[] = [];
	public sessions: Session[] = [];
	public control: FacilityControlEvent[] = [];
	public playerControl: PlayerControlEvent[] = [];

	public items: Map<number, PsItem> = new Map();
	public itemCategories: Map<number, ItemCategory> = new Map();
	public experienceTypes: Map<number, ExperienceType> = new Map();
	public characters: Map<string, PsCharacter> = new Map();
	public outfits: Map<string, PsOutfit> = new Map();
	public facilities: Map<number, PsFacility> = new Map();
	public fireModeXrefs: Map<number, FireGroupToFireMode[]> = new Map();

	// These are added and calculated on the frontend
	public playerMetadata: Map<string, PlayerMetadata> = new Map();
	public playerInfantryDamage: Map<string, InfantryDamageEntry> = new Map();
}

export class PlayerClassStats {
	public name: string = "";
	public kills: number = 0;
	public deaths: number = 0;
	public timeAs: number = 0;
	public exp: number = 0;

	public constructor(name: string) {
		this.name = name;
	}
}

export class PlayerMetadata {
	public ID: string = "";
	public name: string = "";
	public scoreMultiplier: number = 1;
	public outfitID: string | null = null;
	public outfitTag: string | null = null;
	public outfitName: string | null = null;

	public kills: KillEvent[] = [];
	public deaths: KillEvent[] = [];
	public exp: ExpEvent[] = [];
	public timeAs: number = 0;
	public sessions: Session[] = [];

	public classes = {
		infil: new PlayerClassStats("Infiltrator") as PlayerClassStats,
		lightAssault: new PlayerClassStats("Light Assault") as PlayerClassStats,
		medic: new PlayerClassStats("Medic") as PlayerClassStats,
		engineer: new PlayerClassStats("Engineer") as PlayerClassStats,
		heavy: new PlayerClassStats("Heavy") as PlayerClassStats,
		max: new PlayerClassStats("Max") as PlayerClassStats,
		mostPlayed: new PlayerClassStats("") as PlayerClassStats
	};
}

export class PlayerMetadataGenerator {

	public static generate(report: Report): PlayerMetadata[] {

		const map: Map<string, PlayerMetadata> = new Map();

		function getEntry(charID: string, source?: string): PlayerMetadata {
			if (map.has(charID) == false) {
				const char: PsCharacter | null = report.characters.get(charID) || null;

				//console.log(`Adding new metadata entry for ${charID}/${char?.name} ${(source ? source : "")}`);

				const metadata: PlayerMetadata = new PlayerMetadata();
				metadata.ID = charID;
				metadata.name = char?.name ?? `<missing ${charID}>`;
				metadata.outfitID = char?.outfitID || null;
				metadata.outfitTag = char?.outfitTag || null;
				metadata.outfitName = char?.outfitName || null;

				map.set(charID, metadata);
			}
			return map.get(charID)!;
		}

		function getClass(charID: string, loadoutID: number): PlayerClassStats | null {
			const meta: PlayerMetadata = getEntry(charID);

			if (LoadoutUtils.isInfiltrator(loadoutID)) {
				return meta.classes.infil;
			} else if (LoadoutUtils.isLightAssault(loadoutID)) {
				return meta.classes.lightAssault;
			} else if (LoadoutUtils.isMedic(loadoutID)) {
				return meta.classes.medic;
			} else if (LoadoutUtils.isEngineer(loadoutID)) {
				return meta.classes.engineer;
			} else if (LoadoutUtils.isHeavy(loadoutID)) {
				return meta.classes.heavy;
			} else if (LoadoutUtils.isMax(loadoutID)) {
				return meta.classes.max;
			}
			return null;
		}

		for (const kill of report.kills) {
			const entry: PlayerMetadata = getEntry(kill.attackerCharacterID, "kill");

			entry.kills.push(kill);

			const clazz: PlayerClassStats | null = getClass(entry.ID, kill.attackerLoadoutID);
			if (clazz != null) {
				++clazz.kills;
			}
		}

		for (const death of report.deaths) {
			const entry: PlayerMetadata = getEntry(death.killedCharacterID, "death");

			entry.deaths.push(death);

			const clazz: PlayerClassStats | null = getClass(entry.ID, death.killedLoadoutID);
			if (clazz != null) {
				++clazz.deaths;
			}
		}

		for (const exp of report.experience) {
			const entry: PlayerMetadata = getEntry(exp.sourceID, "exp");
			entry.exp.push(exp);

			const clazz: PlayerClassStats | null = getClass(entry.ID, exp.loadoutID);
			if (clazz != null) {
				clazz.exp += exp.amount;
			}
		}

		for (const session of report.sessions) {
			const entry: PlayerMetadata = getEntry(session.characterID, "session");
			entry.sessions.push(session);
		}

		const metas: PlayerMetadata[] = Array.from(map.values());

		console.log(`Have ${metas.length} metas to work on`);

		// Get the time for each class
		for (const meta of metas) {
			let timedEvents: { timestamp: Date, loadoutID: number }[] = meta.kills.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.attackerLoadoutID } });
			timedEvents.push(...meta.deaths.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.killedLoadoutID }; }));
			timedEvents.push(...meta.exp.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.loadoutID }; }));

			//console.log(`Character ${meta.ID}/${meta.name} has ${timedEvents.length} events to build class playtime from`);

			if (timedEvents.length == 0) {
				continue;
			}

			timedEvents = timedEvents.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

			let iter = timedEvents[0];

			for (const ev of timedEvents) {
				const diff: number = (ev.timestamp.getTime() - iter.timestamp.getTime()) / 1000;

				//console.log(`Character ${meta.ID} had ${diff} seconds on ${iter.loadoutID}`);
				iter = ev;

				const entry: PlayerClassStats | null = getClass(meta.ID, ev.loadoutID);
				if (entry != null) {
					entry.timeAs += diff;
				}
			}

			let mostPlayed: PlayerClassStats = meta.classes.infil;
			if (meta.classes.lightAssault.timeAs > mostPlayed.timeAs) {
				mostPlayed = meta.classes.lightAssault;
			}
			if (meta.classes.medic.timeAs > mostPlayed.timeAs) {
				mostPlayed = meta.classes.medic;
			}
			if (meta.classes.engineer.timeAs > mostPlayed.timeAs) {
				mostPlayed = meta.classes.engineer;
			}
			if (meta.classes.heavy.timeAs > mostPlayed.timeAs) {
				mostPlayed = meta.classes.heavy;
			}
			if (meta.classes.max.timeAs > mostPlayed.timeAs) {
				mostPlayed = meta.classes.max;
			}
			meta.classes.mostPlayed = mostPlayed;

			meta.timeAs = meta.classes.infil.timeAs + meta.classes.lightAssault.timeAs
				+ meta.classes.medic.timeAs + meta.classes.engineer.timeAs
				+ meta.classes.heavy.timeAs + meta.classes.max.timeAs;
		}

        /**
         * Get the score multiplier that each character had. It's important to note that a character's score multiplier
         *      can change mid-session, such as if they lose membership or the double XP ends, but I'm not interested
         *      in tracking a changing score multiplier too
         */
		for (const ev of report.experience) {
			if (map.has(ev.sourceID) == false) {
				continue;
            }

			let base: number | null = null;

			switch (ev.experienceID) {
				case Experience.KILL: base = 100; break;
				case Experience.PRIORITY_KILL: base = 150; break;
				case Experience.HIGH_PRIORITY_KILL: base = 300; break;

				case Experience.REVIVE: base = 75; break;
				case Experience.SQUAD_REVIVE: base = 100; break;

				case Experience.RESUPPLY: base = 10; break;
				case Experience.SQUAD_RESUPPLY: base = 15; break;

				case Experience.SQUAD_SPAWN: base = 10; break;
			}

			if (base != null) {
				const metadata: PlayerMetadata = map.get(ev.sourceID)!;
				metadata.scoreMultiplier = ev.amount / base;
			}
		}

		return metas;
	}

}
