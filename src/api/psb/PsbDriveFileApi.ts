

export class PsbDriveFile {
    public id: string = "";
    public name: string = "";
    public kind: string = "";
    public drdiveId: string | null = null;
    public mimeType: string = "";
}

export class PsbDriveFileApi {

    public static parse(elem: any): PsbDriveFile {
        return {
            ...elem
        };
    }

}