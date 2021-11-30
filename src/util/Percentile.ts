
export default class Percentile {

	/**
	 * Get the percentile value of a sorted array
	 * @param arr	Sorted array
	 * @param p		Percentile value to get
	 * 
	 * @returns The value at the percentile given, using linear interpolation if necessary
	 * https://gist.github.com/IceCreamYou/6ffa1b18c4c8f6aeaad2
	 */
	public static percentile(arr: number[], p: number): number {
		if (arr.length == 0) {
			return 0;
		}

		if (p <= 0) {
			return arr[0];
		}
		if (p >= 1) {
			return arr[arr.length - 1];
		}

		const index = (arr.length - 1) * p;
		const lower = Math.floor(index);
		const upper = lower + 1;
		const weight = index % 1;

		if (upper >= arr.length) {
			return arr[lower];
		}

		return arr[lower] * (1 - weight) + arr[upper] * weight;
	}

	/**
	 * Get the rank of a certain value in a sorted array
	 * @param arr	Sorted array
	 * @param value	Rank value to get
	 * 
	 * @returns The percentile of the rank, using linear interpolation if needed
	 * https://gist.github.com/IceCreamYou/6ffa1b18c4c8f6aeaad2
	 */
	public static rank(arr: number[], value: number): number {
		for (let i = 0, l = arr.length; i < l; ++i) {
			if (value <= arr[i]) {
				while (i < l && value == arr[i]) {
					++i;
				}
				if (i == 0) {
					return 0;
				}
				if (value != arr[i - 1]) {
					i += (value - arr[i - 1]) / (arr[i] - arr[i - 1]);
				}
				return i / l;
			}

		}
		return 1;
	}

}