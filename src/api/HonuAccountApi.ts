import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class HonuAccount {
    public id: number = 0;
    public name: string = "";
    public email: string = "";
    public discord: string = "";
    public discordID: string = "0";
    public deletedOn: Date | null = null;
    public deletedBy: number | null = null;
}

export class HonuAccountApi extends ApiWrapper<HonuAccount> {
    private static _instance: HonuAccountApi = new HonuAccountApi();
    public static get(): HonuAccountApi { return HonuAccountApi._instance; };

    public static parse(elem: any): HonuAccount {
        return {
            ...elem,
            discordID: elem.discordID.toString(),
            deletedOn: (elem.deletedOn != null) ? new Date(elem.deletedOn) : null
        };
    }

    public static async getAll(): Promise<Loading<HonuAccount[]>> {
        return HonuAccountApi.get().readList(`/api/account/`, HonuAccountApi.parse);
    }

    public static create(account: HonuAccount): Promise<Loading<number>> {
        const parms: URLSearchParams = new URLSearchParams();
        parms.set("name", account.name);
        parms.set("email", account.email);
        parms.set("discord", account.discord);
        parms.set("discordID", account.discordID + "");

        return HonuAccountApi.get().postReply(`/api/account/create?${parms.toString()}`, (elem: any) => elem);
    }

    public static deactivate(accountID: number): Promise<Loading<void>> {
        return HonuAccountApi.get().delete(`/api/account/${accountID}`);
    }

}
