import Vue from "vue";
import LocaleUtil from "util/Locale";

function compact(value: number): string {
    if ((compact as any).over1k == undefined) {
        (compact as any).over1k = Intl.NumberFormat(undefined, {
            notation: "compact",
            minimumFractionDigits: 1,
            maximumFractionDigits: 1
        }) as Intl.NumberFormat;
    }
    if ((compact as any).under1k == undefined) {
        (compact as any).under1k = Intl.NumberFormat(undefined, {
            notation: "compact",
            maximumFractionDigits: 0,
            minimumFractionDigits: 0
        });
    }

    // we cannot have 1.3k and 3 using the same number format
    // so, use a different if the value is over/under 1k
    if (value > 999) {
        return LocaleUtil.format(value, (compact as any).over1k);
    } else {
        return LocaleUtil.format(value, (compact as any).under1k);
    }
}

Vue.filter("compact", compact);