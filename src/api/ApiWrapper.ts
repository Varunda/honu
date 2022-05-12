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

	public async delete(url: string): Promise<Loading<void>> {
		const response: axios.AxiosResponse<any> = await axios.default.delete(url, { validateStatus: () => true });

		if (response.status == 204) {
			return Loadable.nocontent();
		} else if (response.status == 400) {
			return Loadable.error(`bad request: ${response.data}`);
		} else if (response.status == 404) {
			return Loadable.notFound(response.data);
		} else if (response.status == 500) {
			return Loadable.error(`internal server error: ${response.data}`);
		} else if (response.status == 524) {
			return Loadable.error(`timeout from cloudflare`);
        }

		if (response.status != 200) {
			throw `unchecked status code ${response.status}: ${response.data}`;
		}

		return Loadable.loaded(undefined as any);
    }

	public async postReply<U>(url: string, reader: ApiReader<U>): Promise<Loading<U>> {
		try {
			const response: axios.AxiosResponse<any> = await axios.default.post(url, { validateStatus: () => true });

            if (response.status == 204) {
                return Loadable.nocontent();
            } else if (response.status == 400) {
                return Loadable.error(`bad request: ${response.data}`);
            } else if (response.status == 404) {
                return Loadable.notFound(response.data);
            } else if (response.status == 500) {
                return Loadable.error(`internal server error: ${response.data}`);
            } else if (response.status == 524) {
                return Loadable.error(`timeout from cloudflare`);
            }

            if (response.status != 200) {
                throw `unchecked status code ${response.status}: ${response.data}`;
            }

            const datum: U = reader(response.data);
            return Loadable.loaded(datum);
		} catch (err) {
			const responseData = err.response.data;
			const responseCode = err.response.status;

			if (responseCode == 400) {
				return Loadable.error(`bad request: ${responseData}`);
			} else if (responseCode == 404) {
				return Loadable.notFound(responseData);
			} else if (responseCode == 500) {
				return Loadable.error(`internal server error: ${responseData}`);
			} else if (responseCode == 524) {
				return Loadable.error(`timeout from cloudflare`);
			}

			throw `unchecked status code ${responseCode}: ${responseData}`;
        }
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
        } else if (response.status == 524) {
            return Loadable.error(`timeout from cloudflare`);
		}

		if (response.status != 200) {
			throw `unchecked status code ${response.status}: ${response.data}`;
		}

		return Loadable.loaded(response.data);
	}

}

export type ApiReader<T> = (elem: any) => T;
