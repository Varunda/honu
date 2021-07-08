
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

export class ContinentCount {
	public indar: FactionCount = new FactionCount();
	public hossin: FactionCount = new FactionCount();
	public amerish: FactionCount = new FactionCount();
	public esamir: FactionCount = new FactionCount();
	public other: FactionCount = new FactionCount();
}

export class FactionCount {
	public vs: number = 0;
	public nc: number = 0;
	public tr: number = 0;
	public ns: number = 0;
	public other: number = 0;
}

export class FactionData {
	public factionID: string = "";
	public factionName: string = "";

	public outfitKills: OutfitKillBlock = new OutfitKillBlock();
	public outfitHeals: Block = new Block();
	public outfitResupplies: Block = new Block();
	public outfitRevives: Block = new Block();
	public outfitSpawns: Block = new Block();

	public playerKills: KillBlock = new KillBlock();
	public playerHeals: Block = new Block();
	public playerResupplies: Block = new Block();
	public playerRevives: Block = new Block();
	public playerSpawns: Block = new Block();

	public totalKills: number = 0;
	public totalDeaths: number = 0;
	public totalAssists: number = 0;
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
	public onlineCount: number = 0;
}