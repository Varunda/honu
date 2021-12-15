import * as axios from "axios";
import { Loadable, Loading } from "Loading";

/**
 * Base api wrapper used for all other api classes
 */
export default class ApiWrapper<T> {

	/**
	 * Read a list of paramtype T from a URL
	 * 
	 * @param url		URL to perform the GET request on
	 * @param reader	Reader to transform the entires into the type
	 */
	public async readList<U>(url: string, reader: ApiReader<U>): Promise<Loading<U[]>> {
		const data: Loading<any> = await this.getData(url);
		if (data.state != "loaded") {
			return data;
		}

		if (Array.isArray(data.data) == false) {
			return Loadable.error(`expected array for readList. Did you mean to use readSingle instead? URL: '${url}'`);
		}

		const arr: U[] = data.data.map((iter: any) => {
			return reader(iter);
		});

		return Loadable.loaded(arr);
	}

	/**
	 * Read a single paramtype T from a URL
	 * 
	 * @param url		URL to perform the GET request on
	 * @param reader	Reader to transform the entires into the type
	 */
	public async readSingle<U>(url: string, reader: ApiReader<U>): Promise<Loading<U>> {
		const data: Loading<any> = await this.getData(url);
		if (data.state != "loaded") {
			return data;
		}

		if (Array.isArray(data.data) == true) {
			return Loadable.error(`unexpected array for readSingle. Did you mean to use readList instead? URL: '${url}'`);
		}

		const datum: U = reader(data.data);
		return Loadable.loaded(datum);
	}

	/**
	 * Common 
	 */
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

export type ApiReader<T> = (elem: any) => T;
