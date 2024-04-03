import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";
import { PsbDriveFile, PsbDriveFileApi } from "./PsbDriveFileApi";
import { Session } from "../SessionApi";

export class PsbOvOSheet {
    public fileID: string = "";
    public emails: string[] = [];
    public when: Date = new Date();
    public state: string = "";
    public type: string = "";

    public accounts: PsbSheetAccount[] = [];
}

export class PsbSheetAccount {
    public accountNumber: string = "";
    public username: string = "";
    public player: string | null = null;
}

export class PsbSheetAccountUsage {
    public sheetUsage: PsbSheetAccount = new PsbSheetAccount();
    public sessions: Session[] = [];
}

export class PsbOvOSheetApi extends ApiWrapper<PsbOvOSheet> {
    private static _instance: PsbOvOSheetApi = new PsbOvOSheetApi();
    public static get(): PsbOvOSheetApi { return PsbOvOSheetApi._instance; }

    public static parse(elem: any): PsbOvOSheet {
        return {
            fileID: elem.fileID,
            emails: elem.emails,
            when: new Date(elem.when),
            state: elem.state,
            type: elem. type,
            accounts: elem.accounts.map((iter: any) => PsbOvOSheetApi.parseAccount(iter))
        }
    }

    public static parseAccount(elem: any): PsbSheetAccount {
        return {
            accountNumber: elem.accountNumber,
            username: elem.username,
            player: elem.player
        }
    }

    public static parseUsage(elem: any): PsbSheetAccountUsage {
        return {
            sheetUsage: PsbOvOSheetApi.parseAccount(elem.sheetUsage),
            sessions: []
        }
    }

    public static getSheets(name: string): Promise<Loading<PsbDriveFile[]>> {
        return PsbOvOSheetApi.get().readList(`/api/psb/usage?name=${name}`, PsbDriveFileApi.parse);
    }

    public static getSheet(fileID: string): Promise<Loading<PsbOvOSheet>> {
        return PsbOvOSheetApi.get().readSingle(`/api/psb/usage/${fileID}`, PsbOvOSheetApi.parse);
    }

    public static getSheetUsage(fileID: string): Promise<Loading<PsbSheetAccountUsage[]>> {
        return PsbOvOSheetApi.get().readList(`/api/psb/usage/${fileID}/usage`, PsbOvOSheetApi.parseUsage);
    }

}
