import { WrappedSession } from "./common";
import LocaleUtil from "util/Locale";

export class BestSessionEntry {
    public name: string = "";
    public value: string = "";
    public session: WrappedSession = new WrappedSession();
}

/**
 * Using a list of wrapped sessions, select the one with the highest value given a selector func
 * 
 * @param sessions Sessions to find the best of
 * @param target What list of best to put it into
 * @param name Name of the best
 * @param selector Selector function of the sessions to get the value of 
 * @param filter Optional function filter on the sessions
 * @param localePrecision How many decimals of precision are included, defaults to 2
 */
export function makeSessionBest(
    sessions: WrappedSession[],
    target: BestSessionEntry[],
    name: string,
    selector: (_: WrappedSession) => number,
    filter?: ((_: WrappedSession) => boolean) | undefined,
    localePrecision: number = 2
) {

    const b: WrappedSession | undefined = sessions.filter(iter => {
        if (filter) {
            return filter(iter);
        }
        return true;
    }).sort((a, b) => {
        return selector(b) - selector(a);
    }).at(0);

    if (b != undefined) {
        target.push({
            name: name,
            value: LocaleUtil.locale(selector(b), localePrecision),
            session: b
        });
    }
}
