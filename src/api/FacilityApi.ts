import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsFacility {
	public id: number = 0;
}

export class PsFacilityApi extends ApiWrapper<PsFacility> {

	public static parse(elem: any): PsFacility {
		return {
			...elem
		};
	}

}
