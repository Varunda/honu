
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

export function rgbToString(rgb: RGB): string {
	return `rgb(${rgb.red}, ${rgb.green}, ${rgb.blue}`;
}

export function randomRGB(): RGB {
	return {
		red: Math.floor(Math.random() * (235 - 52 + 1) + 52),
		green: Math.floor(Math.random() * (235 - 52 + 1) + 52),
		blue: Math.floor(Math.random() * (235 - 52 + 1) + 52)
	};
}

export function randomColor(hue: number, total: number, index: number): string {
    const diff: number = 0.618033988749895;
    //let hue: number = Math.random();
    hue += index / total;
    hue += diff;
    hue %= 1;
    let sat: number = 0.5;
    let val: number = 0.95;

    let r: number = 0;
    let g: number = 0;
    let b: number = 0;

    let i = ~~(hue * 6);
    let f = hue * 6 - i;

    let p = val * (1 - sat);
    let q = val * (1 - f * sat);
    let t = val * (1 - (1 - f) * sat);
    switch (i % 6) {
        case 0: r = val; g = t; b = p; break;
        case 1: r = q; g = val; b = p; break;
        case 2: r = p; g = val; b = t; break;
        case 3: r = p; g = q; b = val; break;
        case 4: r = t; g = p; b = val; break;
        case 5: r = val; g = p; b = q; break;
    }

    return `rgba(${~~(r * 256)}, ${~~(g * 256)}, ${~~(b * 256)}, 1)`;
}

export function randomColors(hue: number, total: number): string[] {
	return Array.from(Array(total)).map((_, index) => randomColor(hue, total, index));
}

export function randomColorSingle(): string {
	return randomColor(Math.random(), 1, 0);
}
