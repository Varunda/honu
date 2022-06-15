
export class RealtimeReconnectEntry {

    /**
     * Unique ID
     */
    public id: number = 0;

    /**
     * How many seconds the stream went without events
     */
    public duration: number = 0;

    /**
     * How many times the stream failed the health check before it reconnected
     */
    public failedCount: number = 0;

    /**
     * What stream failed the health check
     */
    public streamType: string = "";

    /**
     * ID of the world 
     */
    public worldID: number = 0;

    /**
     * When this failure ended
     */
    public timestamp: Date = new Date();

    /**
     * How many events on this stream were received before failure
     */
    public eventCount: number = 0;

}