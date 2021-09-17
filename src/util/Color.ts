
export class RGB {
	public red: number = 0;
	public green: number = 0;
	public blue: number = 0;
}

export function colorGradient(fade: number, c1: RGB, c2: RGB, c3?: RGB): RGB {
	let color1: RGB = c1;
	let color2: RGB = c2;

	// Do we have 3 colors for the gradient? Need to adjust the params.
	if (c3) {
		fade = fade * 2;

		// Find which interval to use and adjust the fade percentage
		if (fade >= 1) {
			fade -= 1;
			color1 = c2;
			color2 = c3;
		}
	}

	const diffRed: number = color2.red - color1.red;
	const diffGreen: number = color2.green - color1.green;
	const diffBlue: number = color2.blue - color1.blue;

	const gradient: RGB = new RGB();
	gradient.red = parseInt(Math.floor(color1.red + (diffRed * fade)).toString(), 10);
	gradient.green = parseInt(Math.floor(color1.green + (diffGreen * fade)).toString(), 10);
	gradient.blue = parseInt(Math.floor(color1.blue + (diffBlue * fade)).toString(), 10);

	return gradient;
}
