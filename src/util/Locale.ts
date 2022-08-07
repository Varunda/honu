

export default class LocaleUtil {

    public static locale(n: number, digits?: number): string {
        return n.toLocaleString(undefined, {
            minimumFractionDigits: digits ? digits : (Number.isInteger(n)) ? 0 : 2,
            maximumFractionDigits: digits ? digits : (Number.isInteger(n)) ? 0 : 2
        });
    }

}