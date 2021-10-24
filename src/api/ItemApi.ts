import * as axios from "axios";

export class PsItem {
	public id: string = "";
	public typeID: number = 0;
	public categoryID: number = 0;
	public name: string = "";
}

export class ItemApi {
	private static _instance: ItemApi = new ItemApi();
	public static get(): ItemApi { return ItemApi._instance; }

	public static parse(elem: any): PsItem {
		return {
			...elem
		};
	};

	public static async getMultiple(IDs: string[]): Promise<PsItem[]> {
		let params: URLSearchParams = new URLSearchParams();
		IDs.map(iter => params.append("IDs", iter));

		const response: axios.AxiosResponse = await axios.default.get(`/api/item?${params.toString()}`);

		if (response.status != 200) {
			throw response.data;
		}

		if (Array.isArray(response.data) == false) {
			throw `response.data is supposed to be an array`;
		}

		return response.data.map((iter: any) => ItemApi.parse(iter));
	}

}
