import { WorldTagEntry } from "api/WorldTagApi";
import { CensusRealtimeHealthEntry } from "api/HonuHealthApi";
import { RealtimeReconnectEntry } from "api/RealtimeReconnectapi";
import { RealtimeMapState } from "api/RealtimeMapStateApi";
import { PsFacility } from "api/MapApi";
import { VehicleUsageData } from "api/VehicleUsageApi";

export class BlockEntry {
	public id: string = "";
	public name: string = "";
	public value: number = 0;
}

export class Block {
	public name: string = "";
	public entires: BlockEntry[] = [];
	public total: number = 0;
}

export class KillData {
	public id: string = "";
	public name: string = "";
	public factionID: number = 0;
	public kills: number = 0;
	public deaths: number = 0;
	public assists: number = 0;
}

export class OutfitKillData {
	public id: string = "";
	public factionId: string = "";
	public tag: string | null = null;
	public name: string = "";
	public kills: number = 0;
	public deaths: number = 0;
	public members: number = 0;
}

export class SpawnEntries {
	public entries: SpawnEntry[] = [];
}

export class SpawnEntry {
	public ownerID: string = "";
	public owner: string = "";
	public spawnCount: number = 0;
	public secondsAlive: number = 0;
	public firstSeenAt: number = 0;
}

export class KillBlock {
	public entries: KillData[] = [];
}

export class OutfitKillBlock {
	public entires: OutfitKillData[] = [];
}

export class WeaponKillsBlock {
	public entries: WeaponKillEntry[] = [];
}

export class WeaponKillEntry {
	public itemId: string = "";
	public itemName: string = "";
	public kills: number = 0;
}

export class ContinentCount {
	public indar: FactionCount = new FactionCount();
	public hossin: FactionCount = new FactionCount();
	public amerish: FactionCount = new FactionCount();
	public esamir: FactionCount = new FactionCount();
	public oshur: FactionCount = new FactionCount();
	public other: FactionCount = new FactionCount();
}

export class FactionCount {
	public vs: number = 0;
	public nc: number = 0;
	public tr: number = 0;
	public ns: number = 0;
	public other: number = 0;
}

export class FactionFocus {
	public vs: FactionFocusEntry = new FactionFocusEntry();
	public nc: FactionFocusEntry = new FactionFocusEntry();
	public tr: FactionFocusEntry = new FactionFocusEntry();
}

export class FactionFocusEntry {
	public vsKills: number = 0;
	public ncKills: number = 0;
	public trKills: number = 0;
}

export class WorldZonePopulation {
	public worldID: number = 0;
	public zoneID: number = 0;
	public timestamp: Date = new Date();

	public total: number = 0;
	public factionVs: number = 0;
	public factionNc: number = 0;
	public factionTr: number = 0;
	public factionNs: number = 0;

	public teamVs: number = 0;
	public teamNc: number = 0;
	public teamTr: number = 0;
	public teamUnknown: number = 0;
}

export class FactionData {
	public factionID: string = "";
	public factionName: string = "";

	public outfitKills: OutfitKillBlock = new OutfitKillBlock();
	public outfitHeals: Block = new Block();
	public outfitResupplies: Block = new Block();
	public outfitRevives: Block = new Block();
	public outfitSpawns: Block = new Block();
	public outfitVehicleKills: Block = new Block();
	public outfitShieldRepair: Block = new Block();

	public playerKills: KillBlock = new KillBlock();
	public playerHeals: Block = new Block();
	public playerResupplies: Block = new Block();
	public playerRevives: Block = new Block();
	public playerSpawns: Block = new Block();
	public playerVehicleKills: Block = new Block();
	public playerShieldRepair: Block = new Block();

	public weaponKills: WeaponKillsBlock = new WeaponKillsBlock();

	public totalKills: number = 0;
	public totalDeaths: number = 0;
	public totalAssists: number = 0;
}

export class RealtimeDataFight {
	public mapState: RealtimeMapState = new RealtimeMapState();
	public facility: PsFacility | null = null;
}

export class WorldData {
	public worldID: string = "";
	public worldName: string = "";
	public trackingDuration: number = 0;
	public continentCount: ContinentCount = new ContinentCount();
	public nc: FactionData = new FactionData();
	public tr: FactionData = new FactionData();
	public vs: FactionData = new FactionData();
	public topSpawns: SpawnEntries = new SpawnEntries();
	public factionFocus: FactionFocus = new FactionFocus();
	public onlineCount: number = 0;
	public tagEntries: WorldTagEntry[] = [];
	public realtimeHealth: CensusRealtimeHealthEntry[] = [];
	public reconnects: RealtimeReconnectEntry[] = [];
	public fights: RealtimeDataFight[] = [];
	public population: WorldZonePopulation[] = [];
	public vehicleUsage: VehicleUsageData = new VehicleUsageData();

}