

export default class LocaleUtil {

    /**
     * Apply the .toLocaleString function on a number, which adds separators to numbers,
     * optionally specifying how many digits to keep of the decimal part. 
     * @param n         Number to be localized
     * @param digits    How many digits in the decimal part to keep. 
     *                  By default, it will use 0 for integers and 2 for non-integers, but it can be changed
     */
    public static locale(n: number, digits?: number): string {
        return n.toLocaleString(undefined, {
            minimumFractionDigits: (digits != undefined) ? digits : ((Number.isInteger(n)) ? 0 : 2),
            maximumFractionDigits: (digits != undefined) ? digits : ((Number.isInteger(n)) ? 0 : 2)
        });
    }

    public static format(n: number, format: Intl.NumberFormat): string {
        return format.format(n);
    }

}