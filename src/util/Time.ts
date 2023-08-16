import * as moment from "moment";

export default class TimeUtils {

    private static _timezoneName: string = "";

    public static duration(seconds: number): string {
        const dur: moment.Duration = moment.duration(seconds * 1000);

        if (dur.asDays() >= 1) {
            return `${Math.floor(dur.asDays())}d ${dur.hours().toString().padStart(2, "0")}h`;
        }

        if (dur.asHours() >= 1) {
            return `${dur.hours()}h ${dur.minutes().toString().padStart(2, "0")}m`;
        }

        return `${dur.minutes().toString().padStart(2, "0")}m ${dur.seconds().toString().padStart(2, "0")}s`;
    }

    public static format(date: Date, format: string = "YYYY-MM-DD hh:mmA"): string {
        return moment(date).format(format) + " " + TimeUtils.getTimezoneName();
    }

    public static formatNoTimezone(date: Date, format: string = "YYYY-MM-DD hh:mmA"): string {
        return moment(date).format(format);
    }

    // https://stackoverflow.com/questions/9772955/how-can-i-get-the-timezone-name-in-javascript
    public static getTimezoneName(): string {
        if (TimeUtils._timezoneName == "") {
            const today = new Date();
            const short = today.toLocaleDateString();
            const full = today.toLocaleDateString(undefined, { timeZoneName: "short" });

            const shortIndex = full.indexOf(short);
            if (shortIndex >= 0) {
                const trimmed = full.substring(0, shortIndex) + full.substring(shortIndex + short.length);
                TimeUtils._timezoneName  = trimmed.replace(/^[\s,.\-:;]+|[\s,.\-:;]+$/g, '');
            }
        }

        return TimeUtils._timezoneName;
    }

}
