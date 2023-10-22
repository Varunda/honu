
type WrappedSimpleType = "character" | "outfit" | "item" | "facility" | "none";

export class WrappedSimpleData {
    public name: string = "";
    public data: WrappedSimpleEntry[] = [];
    public total: number = 0;
    public totalDisplay: string | null = null;
    public type: WrappedSimpleType = "none";

    public constructor(name: string, type: WrappedSimpleType) {
        this.name = name;
        this.data = [];
        this.type = type;
    }

}

export class WrappedSimpleEntry {

    public display: string = "";
    public id: string = "";
    public link: string | null = null;
    public value: string = "";

}