import { KillEvent } from "api/KillStatApi";
import { ExpEvent } from "api/ExpStatApi";
import { PsItem } from "api/ItemApi";
import { PsOutfit } from "api/OutfitApi";
import { PsCharacter } from "api/CharacterApi";
import { Session } from "api/SessionApi";

import Loadout from "util/Loadout";

export default class Report {
	public ID: number = 0;
	public generator: string = "";
	public timestamp: Date = new Date();
	public periodStart: Date = new Date();
	public periodEnd: Date = new Date();
	public teamID: number = 0;

	public kills: KillEvent[] = [];
	public deaths: KillEvent[] = [];
	public experience: ExpEvent[] = [];

	public items: Map<string, PsItem> = new Map();
	public characters: Map<string, PsCharacter> = new Map();
	public outfits: Map<string, PsOutfit> = new Map();
	public sessions: Session[] = [];

	public playerMetadata: Map<string, PlayerMetadata> = new Map();
}

export class PlayerClassStats {
	public name: string = "";
	public kills: number = 0;
	public deaths: number = 0;
	public timeAs: number = 0;

	public constructor(name: string) {
		this.name = name;
	}
}

export class PlayerMetadata {
	public ID: string = "";
	public name: string = "";
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

		function getEntry(charID: string): PlayerMetadata {
			if (map.has(charID) == false) {
				console.log(`Adding new metadata entry for ${charID}`);

				const char: PsCharacter | null = report.characters.get(charID) || null;

				const metadata: PlayerMetadata = new PlayerMetadata();
				metadata.ID = charID;
				metadata.name = char?.name ?? `<missing ${charID}>`;

				map.set(charID, metadata);
			}
			return map.get(charID)!;
		}

		function getClass(charID: string, loadoutID: number): PlayerClassStats | null {
			const meta: PlayerMetadata = getEntry(charID);

			if (Loadout.isInfiltrator(loadoutID)) {
				return meta.classes.infil;
			} else if (Loadout.isLightAssault(loadoutID)) {
				return meta.classes.lightAssault;
			} else if (Loadout.isMedic(loadoutID)) {
				return meta.classes.medic;
			} else if (Loadout.isEngineer(loadoutID)) {
				return meta.classes.engineer;
			} else if (Loadout.isHeavy(loadoutID)) {
				return meta.classes.heavy;
			} else if (Loadout.isMax(loadoutID)) {
				return meta.classes.max;
			}
			return null;
		}

		for (const kill of report.kills) {
			const entry: PlayerMetadata = getEntry(kill.attackerCharacterID);

			entry.kills.push(kill);

			const clazz: PlayerClassStats | null = getClass(entry.ID, kill.attackerLoadoutID);
			if (clazz != null) {
				++clazz.kills;
			}
		}

		for (const death of report.deaths) {
			const entry: PlayerMetadata = getEntry(death.killedCharacterID);

			entry.deaths.push(death);

			const clazz: PlayerClassStats | null = getClass(entry.ID, death.killedLoadoutID);
			if (clazz != null) {
				++clazz.deaths;
			}
		}

		for (const exp of report.experience) {
			const entry: PlayerMetadata = getEntry(exp.sourceID);
			entry.exp.push(exp);
		}

		for (const session of report.sessions) {
			const entry: PlayerMetadata = getEntry(session.characterID);
			entry.sessions.push(session);
		}

		const metas: PlayerMetadata[] = Array.from(map.values());

		console.log(`Have ${metas.length} metas to work on`);

		for (const meta of metas) {
			let timedEvents: { timestamp: Date, loadoutID: number }[] = meta.kills.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.attackerLoadoutID } });
			timedEvents.push(...meta.deaths.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.killedLoadoutID }; }));
			timedEvents.push(...meta.exp.map(iter => { return { timestamp: iter.timestamp, loadoutID: iter.loadoutID }; }));

			console.log(`Character ${meta.ID}/${meta.name} has ${timedEvents.length} events to build class playtime from`);

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

		return metas;
	}

}
