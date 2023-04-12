import { Loading } from "../Loading";
import ApiWrapper from "./ApiWrapper";

export class WrappedEntry {
    public id: string = ""; // guid
    public inputCharacterIDs: string[] = [];
}

export class WrappedApi extends ApiWrapper<WrappedEntry> {
    private static _instance: WrappedApi = new WrappedApi();
    public static get(): WrappedApi { return WrappedApi._instance; }

    public static parse(elem: any): WrappedEntry {
        return {
            ...elem
        };
    }

    public static getByID(id: string): Promise<Loading<WrappedEntry>> {
        return WrappedApi.get().readSingle(`/api/wrapped/{id}`, WrappedApi.parse);
    }

    public static insert(input: string[]): Promise<Loading<string>> {
        return WrappedApi.get().postReplyForm("/api/wrapped", {
            input: {
                "IDs": input
            }
        }, (elem: any) => elem);
    }

}
