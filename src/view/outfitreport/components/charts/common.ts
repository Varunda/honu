
export class Block {
	public entries: BlockEntry[] = [];
	public total: number = 0;
}

export class BlockEntry {
	public name: string = "";
	public count: number = 0;
	public link?: string;
}