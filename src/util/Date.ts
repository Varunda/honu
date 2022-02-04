

export default class DateUtil {

	/**
	 * Turn a Date object into a ISO8601 compliant string, but represented in the user's local timezone, instead of UTC
	 * @param date
	 */
	public static getLocalDateString(date: Date): string {
		const year: string = date.getFullYear().toString().padStart(4, "0");
		const month: string = (date.getMonth() + 1).toString().padStart(2, "0");
		const day: string = date.getDate().toString().padStart(2, "0");
		const hours: string = date.getHours().toString().padStart(2, "0");
		const minutes: string = date.getMinutes().toString().padStart(2, "0");

		return `${year}-${month}-${day}T${hours}:${minutes}`;
	}

	public static getLocalDateOnlyString(date: Date): string {
		const year: string = date.getFullYear().toString().padStart(4, "0");
		const month: string = (date.getMonth() + 1).toString().padStart(2, "0");
		const day: string = date.getDate().toString().padStart(2, "0");

		return `${year}-${month}-${day}`;
    }

	/**
	 * Zero out parts of a Date, for example setting the seconds of a Date to 0

	 * @param input Input date to produce the output from
	 * @param parts What parts of the date will be zeroed
	 */
	public static zeroParts(input: Date,
		parts: { years?: boolean, months?: boolean, days?: boolean, hours?: boolean, minutes?: boolean, seconds?: boolean, milliseconds?: boolean }): Date {

		let date: Date = new Date(input);

		if (parts.years == true) {
			date.setFullYear(0);
        }

		if (parts.months == true) {
			date.setMonth(0);
        }

		if (parts.days == true) {
			date.setDate(0);
        }

		if (parts.minutes == true) {
			date.setMinutes(0);
        }

		if (parts.seconds == true) {
			date.setSeconds(0);
        }

		if (parts.milliseconds == true) {
			date.setMilliseconds(0);
        }

		return date;
    }

}