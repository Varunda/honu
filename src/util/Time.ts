import * as moment from "moment";

export default class TimeUtils {

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
        return moment(date).format(format);
    }

}
