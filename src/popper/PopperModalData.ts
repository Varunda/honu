export class PopperModalData {

    /**
     * Is the tooltip data loading
     */
    public loading: boolean = false;

    /**
     * Data being displayed
     */
    public data: any[] = [];

    /**
     * What to name each column being displayed
     */
    public columnNames: string[] = [];

    /**
     * Fields within the array of data that will be displayed
     */
    public columnFields: string[] = [];

    /**
     * Custom renderers for each column if needed. If not needed, leave the map empty
     */ 
    public renderers: Map<string, (data: any) => string> = new Map();

    /**
     * Title of the tooltip
     */
    public title: string = "";

    /**
     * What element the tooltip will be attached to
     */
    public root: HTMLElement | null = null;

}