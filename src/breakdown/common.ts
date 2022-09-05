export class Block {
	public entries: BlockEntry[] = [];
	public total: number = 0;
}

export class BlockEntry {

	/**
	 * What to display as the name of this entry
	 */
	public name: string = "";

	/**
	 * Count of this entry
	 */
	public count: number = 0;

	/**
	 * What color to display this entry as. Leaving null will use a random color
	 */
	public color: string | null = null;

	/**
	 * ?
	 */
	public link?: string;

}