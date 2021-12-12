

export default class DateUtil {

	public static getLocalDateString(date: Date): string {
		const year: string = date.getFullYear().toString().padStart(4, "0");
		const month: string = (date.getMonth() + 1).toString().padStart(2, "0");
		const day: string = date.getDate().toString().padStart(2, "0");
		const hours: string = date.getHours().toString().padStart(2, "0");
		const minutes: string = date.getMinutes().toString().padStart(2, "0");

		return `${year}-${month}-${day}T${hours}:${minutes}`;
	}

}