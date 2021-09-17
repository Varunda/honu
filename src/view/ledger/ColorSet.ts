import { RGB } from "util/Color";

export type ColorSet = { start: RGB, end: RGB, middle?: RGB };

export const colorMin: ColorSet = {
	start: { red: 0, green: 255, blue: 0 },
	end: { red: 64, green: 255, blue: 0 }
};

export const colorNormal: ColorSet = {
	start: { red: 64, green: 255, blue: 0 },
	middle: { red: 255, green: 64, blue: 0 },
	end: { red: 255, green: 255, blue: 0 }
};

export const colorMax: ColorSet = {
	start: { red: 255, green: 64, blue: 0 },
	end: { red: 255, green: 0, blue: 0 }
};

export const bwMin: ColorSet = {
	start: { red: 0, green: 0, blue: 0 },
	end: { red: 20, green: 20, blue: 20 }
};

export const bwNormal: ColorSet = {
	start: { red: 20, green: 20, blue: 20 },
	end: { red: 240, green: 240, blue: 240 }
};

export const bwMax: ColorSet = {
	start: { red: 240, green: 240, blue: 240 },
	end: { red: 255, green: 255, blue: 255 }
};
