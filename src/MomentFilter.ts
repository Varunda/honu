﻿import Vue from "vue";

import * as moment from "moment";

function vueMoment(input: Date | string | null | undefined, format: string = "YYYY-MM-DD hh:mmA") {
    // Who knew that you could assign properties to a function
    if (typeof (vueMoment as any).tz == "undefined") {
        (vueMoment as any).tz = new Date().getTimezoneOffset();
    }
    if (input == null || input == undefined || input == "") {
        return "";
    }

    if (typeof input == "string") {
        // Date strings ending with Z mean this ISO8601 date string is formatted in UTC time
        //      without the Z, it means local time, and since all dates as passed as UTC, just force it
        if (input.endsWith("Z") == false) {
            input += "Z";
        }
        return moment(input).format(format);
    } else if (input instanceof Date) {
        return moment(input).format(format);
    } else if (typeof input == "number") {
        return moment(new Date(input)).format(format);
    } else {
        throw `Unknown type of input: ${input} (${typeof input})`;
    }
}

Vue.filter("moment", vueMoment);

Vue.filter("duration", (input: string | number, format: string): string => {
    const val = (typeof(input) == "string") ? Number.parseInt(input) : input;
    if (Number.isNaN(val)) {
        return `NaN ${val}`;
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
        return "1s";
    }

    if (val < 60) {
        parts.seconds = val % 60;
        return `${val % 60}s`;
    }

    if (val == 0) {
        parts.minutes = 1;
        return `00:01`;
    }

    if (val < (60 * 60)) {
        parts.minutes = Math.round(val / 60);
        parts.seconds = val % 60;
        return `00:${Math.round(val / 60).toString().padStart(2, "0")}`;
    }

    if (val == 60 * 60) {
        parts.hour = 1;
        return `01:00`;
    }

    const hours = Math.floor(val / 3600);
    const mins = Math.floor((val - (3600 * hours)) / 60);
    const secs = val % 60;

    parts.hour = hours;
    parts.minutes = mins;
    parts.seconds = secs;

    return `${hours.toString().padStart(2, "0")}:${mins.toString().padStart(2, "0")}`;
});