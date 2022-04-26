import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class AlertPopulation {
    public id: number = 0;
    public alertID: number = 0;
    public timestamp: Date = new Date();
    public countVS: number = 0;
    public countNC: number = 0;
    public countTR: number = 0;
    public countUnknown: number = 0;
}

export class AlertPopulationApi extends ApiWrapper<AlertPopulation> {
    private static _instance: AlertPopulationApi = new AlertPopulationApi;
    public static get(): AlertPopulationApi { return AlertPopulationApi._instance; }

    public static readEntry(elem: any): AlertPopulation {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static getByAlertID(alertID: number): Promise<Loading<AlertPopulation[]>> {
        return AlertPopulationApi.get().readList(`/api/alerts/${alertID}/population`, AlertPopulationApi.readEntry);
    }


}
