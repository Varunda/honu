import * as axios from "axios";
import { Loadable, Loading } from "Loading";

export default abstract class ApiWrapper<T> {

	public abstract parse(elem: any): T;

	public async readList(url: string): Promise<Loading<T[]>> {
		const data: Loading<any> = await this.getData(url);
		if (data.state != "loaded") {
			return data;
		}

		if (Array.isArray(data.data) == false) {
			return Loadable.error(`expected array for readList. Did you mean to use readSingle instead? URL: '${url}'`);
		}

		const arr: T[] = data.data.map((iter: any) => {
			return this.parse(iter);
		});

		return Loadable.loaded(arr);
	}

	public async readSingle(url: string): Promise<Loading<T>> {
		const data: Loading<any> = await this.getData(url);
		if (data.state != "loaded") {
			return data;
		}

		if (Array.isArray(data.data) == true) {
			return Loadable.error(`unexpected array for readSingle. Did you mean to use readList instead? URL: '${url}'`);
		}

		const datum: T = this.parse(data.data);
		return Loadable.loaded(datum);
	}

	private async getData(url: string): Promise<Loading<any>> {
		const response: axios.AxiosResponse<any> = await axios.default.get(url, { validateStatus: () => true });

		if (response.status == 204) {
			return Loadable.nocontent();
		} else if (response.status == 400) {
			return Loadable.error(`bad request: ${response.data}`);
		} else if (response.status == 404) {
			return Loadable.notFound(response.data);
		} else if (response.status == 500) {
			return Loadable.error(`internal server error: ${response.data}`);
		}

		if (response.status != 200) {
			throw `unchecked status code ${response.status}: ${response.data}`;
		}

		return Loadable.loaded(response.data);
	}


}