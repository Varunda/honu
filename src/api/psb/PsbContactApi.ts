import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsbContact {

}

export class PsbOvOContact {
    public accountLimit: number = 0;
    public accountPings: boolean = false;
    public basePings: boolean = false;
    public notes: string = "";
    public repType: number = 0;
    public groups: string[] = [];
    public discordID: number = 0;
    public email: string = "";
    public name: string = "";
}

export class PsbContactApi extends ApiWrapper<PsbContact> {
    private static _instance: PsbContactApi = new PsbContactApi();
    public static get(): PsbContactApi { return PsbContactApi._instance; }

    public static parse(elem: any): PsbContact {
        return {

        }
    }

    public static parseOvO(elem: any): PsbOvOContact {
        return {
            ...elem,
            groups: elem.groups
        }
    }

    public static getOvOContacts(): Promise<Loading<PsbOvOContact[]>> {
        return PsbContactApi.get().readList(`/api/psb/contact`, PsbContactApi.parseOvO);
    }


}
