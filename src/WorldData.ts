
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
	public kills: number = 0;
	public deaths: number = 0;
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

export class KillBlock {
	public entries: KillData[] = [];
}

export class OutfitKillBlock {
	public entires: OutfitKillData[] = [];
}

export class FactionData {
	public factionID: string = "";
	public factionName: string = "";
	public playerKills: KillBlock = new KillBlock();
	public outfitKills: OutfitKillBlock = new OutfitKillBlock();
	public outfitHeals: Block = new Block();
	public outfitResupplies: Block = new Block();
	public outfitRevives: Block = new Block();
	public playerHeals: Block = new Block();
	public playerResupplies: Block = new Block();
	public playerRevives: Block = new Block();
}

export class WorldData {
	public worldID: string = "";
	public worldName: string = "";
	public nc: FactionData = new FactionData();
	public tr: FactionData = new FactionData();
	public vs: FactionData = new FactionData();
}