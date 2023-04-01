import Vue from "vue";

import * as moment from "moment";

function vueMoment(input: Date | string | number | null | undefined, format: string = "YYYY-MM-DD hh:mmA") {
    // Who knew that you could assign properties to a function
    if (typeof (vueMoment as any).tz == "undefined") {
        (vueMoment as any).tz = new Date().getTimezoneOffset();
    }
    if (typeof (vueMoment as any).tzname == "undefined") {
        const today = new Date();
        const short = today.toLocaleDateString();
        const full = today.toLocaleDateString(undefined, { timeZoneName: "short" });

        const shortIndex = full.indexOf(short);
        if (shortIndex >= 0) {
            const trimmed = full.substring(0, shortIndex) + full.substring(shortIndex + short.length);
            (vueMoment as any).tzname = trimmed.replace(/^[\s,.\-:;]+|[\s,.\-:;]+$/g, '');
        }
    }
    if (input == null || input == undefined || input == "") {
        return "";
    }

    const tzname: string = (vueMoment as any).tzname;

    if (typeof input == "string") {
        // Date strings ending with Z mean this ISO8601 date string is formatted in UTC time
        //      without the Z, it means local time, and since all dates as passed as UTC, just force it
        if (input.endsWith("Z") == false) {
            input += "Z";
        }
        return moment(input).format(format) + " " + tzname;
    } else if (typeof input == "number") {
        return moment(new Date(input)).format(format) + " " + tzname;
    } else if (input instanceof Date) {
        return moment(input).format(format) + " " + tzname;
        //return moment(input).add(-(vueMoment as any).tz, "minutes").format(format);
    } else if (typeof input == "number") {
        return moment(new Date(input)).format(format) + " " + tzname;
    } else {
        throw `Unknown type of input in moment filter, cannot parse to the format: ${input} (${typeof input})`;
    }
}

Vue.filter("moment", vueMoment);

Vue.filter("duration", (input: string | number, format: string): string => {
    const val = (typeof(input) == "string") ? Number.parseInt(input) : input;
    if (Number.isNaN(val)) {
        return `NaN ${val}`;
    }

    let isPast: boolean = false;
    let str: string = "";

    if (val < 0) {
        isPast = true;
    }

    if (val == 0) {
        return "Never";
    }

    const parts = {
        seconds: 0 as number,
        minutes: 0 as number,
        hour: 0 as number
    };

    if (val == 1) {
        parts.seconds = 1;
        str = "1s";
    } else if (val < 60) {
        parts.seconds = val % 60;
        str = `${val % 60}s`;
    } else if (val == 0) {
        parts.minutes = 1;
        str = `00:01`;
    } else if (val < (60 * 60)) {
        parts.minutes = Math.round(val / 60);
        parts.seconds = val % 60;
        str = `00:${Math.round(val / 60).toString().padStart(2, "0")}`;
    } else if (val == 60 * 60) {
        parts.hour = 1;
        str = `01:00`;
    }

    const hours = Math.floor(val / 3600);
    const mins = Math.floor((val - (3600 * hours)) / 60);
    const secs = val % 60;

    parts.hour = hours;
    parts.minutes = mins;
    parts.seconds = secs;

    str = `${hours.toString().padStart(2, "0")}:${mins.toString().padStart(2, "0")}`;

    return `${str}${(isPast == true ? " ago" : "")}`;
});

Vue.filter("mduration", (input: string | number): string => {
    const val: number = (typeof(input) == "string") ? Number.parseInt(input) : input;
    if (Number.isNaN(val)) {
        return `NaN ${val}`;
    }

    const dur: moment.Duration = moment.duration(val * 1000);

    if (dur.asDays() >= 1) {
        return `${Math.floor(dur.asDays())}d ${dur.hours().toString().padStart(2, "0")}h`;
    }

    if (dur.asHours() >= 1) {
        return `${dur.hours()}h ${dur.minutes().toString().padStart(2, "0")}m`;
    }

    return `${dur.minutes().toString().padStart(2, "0")}m ${dur.seconds().toString().padStart(2, "0")}s`;
});

Vue.filter("tduration", (input: string | number): string => {
    const val: number = (typeof (input) == "string") ? Number.parseInt(input) : input;
    if (Number.isNaN(val)) {
        return `NaN ${val}`;
    }

    const dur: moment.Duration = moment.duration(val * 1000);

    if (dur.asDays() >= 1) {
        return `${Math.floor(dur.asDays())} days, ${dur.hours()} hours`;
    }

    if (dur.asHours() >= 1) {
        return `${dur.hours()} hours, ${dur.minutes()} minutes`;
    }

    return `${dur.minutes()} minutes, ${dur.seconds()} seconds`;
});

Vue.filter("til", (time: Date) => {
    return moment(time).fromNow();
});
