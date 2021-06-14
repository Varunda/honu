import Vue from "vue";

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