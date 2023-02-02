import ApiWrapper from "./ApiWrapper";
import { Loading, Loadable } from "Loading";
import { ItemApi, PsItem } from "./ItemApi";

export class ItemAddedEvent {
    public id: number = 0;
    public characterID: string = "";
    public itemID: number = 0;
    public itemCount: number = 0;
    public context: string = "";
    public timestamp: Date = new Date();
    public zoneID: number = 0;
    public worldID: number = 0;
}

export class ItemAddedEventBlock {
    public events: ItemAddedEvent[] = [];
    public items: PsItem[] = [];
}

export class ItemAddedEventApi extends ApiWrapper<ItemAddedEvent> {
    private static _instance: ItemAddedEventApi = new ItemAddedEventApi();
    public static get(): ItemAddedEventApi { return ItemAddedEventApi._instance; }

    public static parse(elem: any): ItemAddedEvent {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static parseBlock(elem: any): ItemAddedEventBlock {
        return {
            events: elem.events.map((iter: any) => ItemAddedEventApi.parse(iter)),
            items: elem.items.map((iter: any) => ItemApi.parse(iter))
        };
    }

    public static getBySessionID(sessionID: number): Promise<Loading<ItemAddedEventBlock>> {
        return ItemAddedEventApi.get().readSingle(`/api/item-added/session/${sessionID}`, ItemAddedEventApi.parseBlock);
    }

}