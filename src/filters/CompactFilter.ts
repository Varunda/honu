import Vue from "vue";
import LocaleUtil from "util/Locale";

function compact(value: number): string {
    if ((compact as any).intl == undefined) {
        (compact as any).intl = Intl.NumberFormat(undefined, {
            notation: "compact",
            minimumFractionDigits: 1,
            maximumFractionDigits: 1
        }) as Intl.NumberFormat;
    }

    return LocaleUtil.format(value, (compact as any).intl);
}

Vue.filter("compact", compact);