import Vue from "vue";
import * as moment from "moment";

Vue.filter("duration", (data: number): string => {

    if (data <= 10) {
        return `${data.toPrecision(2)}ms`;
    }

    if (data <= 1_000) {
        return `${Math.floor(data)}ms`;
    }

    if (data <= 60_000) {
        return `${Math.floor(data / 1000)}s ${(Math.floor(data % 1000)).toString().padStart(3, "0")}ms`;
    }

    const m: number = Math.floor(data / (1000 * 60));
    const s: number = Math.floor((data - m) % 60);

    return `${m}m ${s.toString().padStart(2, "0")}s`;
});
