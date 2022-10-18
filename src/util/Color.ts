
export class RGB {
    public red: number = 0;
    public green: number = 0;
    public blue: number = 0;
}

export type BootstrapColor = "primary" | "secondary" | "success" | "info" | "warning" | "danger"
    | "blue" | "indigo" | "purple" | "pink" | "red" | "orange" | "yellow" | "green" | "teal" | "cyan"
    | "white" | "gray" | "gray-dark" | "light" | "dark";

export default class ColorUtils {

    /*
	public static VS: string = "#cf17cf";
	public static NC: string = "#3f7fff";
	public static TR: string = "#ea5e5e";
	public static NS: string = "#cbcbcb";
    */

    /*
	public static VS: string = "#8640a4";
	public static NC: string = "#1a5dab";
	public static TR: string = "#c33d2f";
	public static NS: string = "#cbcbcb";
    */

	public static VS: string = "#ba60df";
	public static NC: string = "#5194e7";
	public static TR: string = "#f1615b";
	public static NS: string = "#cbcbcb";

	public static BG_VS: string = "#8640a4";
	public static BG_NC: string = "#1a5dab";
	public static BG_TR: string = "#c33d2f";
	public static BG_NS: string = "#cbcbcb";


	public static getFactionColor(factionID: number): string {
		if (factionID == 1) {
			return ColorUtils.VS;
		} else if (factionID == 2) {
			return ColorUtils.NC;
		} else if (factionID == 3) {
			return ColorUtils.TR;
		} else if (factionID == 4) {
			return ColorUtils.NS;
		} 

		return "";
	}

    /**
     * Create a single random color
     */
    public static randomColorSingle(): string {
        return ColorUtils.randomColor(Math.random(), 1, 0);
    }

    /**
     * Create a series of random colors
     * @param hue
     * @param total
     */
    public static randomColors(hue: number, total: number, opacity: number = 1): string[] {
        return Array.from(Array(total)).map((_, index) => ColorUtils.randomColor(hue, total, index, opacity));
    }

    /**
     * Create a random pastel color
     * @param hue
     * @param total
     * @param index
     */
    public static randomColor(hue: number, total: number, index: number, opacity: number = 1): string {
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

        return `rgba(${~~(r * 256)}, ${~~(g * 256)}, ${~~(b * 256)}, ${opacity})`;
    }

    /**
     * Create a gradient between 2 colors, using HSV for the transform, instead of RGB
     * @param fade What percentage of the gradient the resulting value will be
     * @param c1
     * @param c2
     * @param c3
     */
    public static colorGradient(fade: number, c1: RGB, c2: RGB, c3?: RGB): RGB {
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

    /**
     * Convert a RGB value into a CSS string
     * @param rgb
     */
    public static rgbToString(rgb: RGB): string {
        return `rgb(${rgb.red}, ${rgb.green}, ${rgb.blue})`;
    }

    /**
     * Get a random RGB color
     */
    public static randomRGB(): RGB {
        return {
            red: Math.floor(Math.random() * (235 - 52 + 1) + 52),
            green: Math.floor(Math.random() * (235 - 52 + 1) + 52),
            blue: Math.floor(Math.random() * (235 - 52 + 1) + 52)
        };
    }

}
