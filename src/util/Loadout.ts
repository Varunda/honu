
export default class Loadout {

	public static NC_INFILTRATOR: number = 1;
	public static NC_LIGHT_ASSAULT: number = 3;
	public static NC_MEDIC: number = 4;
	public static NC_ENGINEER: number = 5;
	public static NC_HEAVY_ASSAULT: number = 6;
	public static NC_MAX: number = 7;

	public static TR_INFILTRATOR: number = 8;
	public static TR_LIGHT_ASSAULT: number = 10;
	public static TR_MEDIC: number = 11;
	public static TR_ENGINEER: number = 12;
	public static TR_HEAVY_ASSAULT: number = 13;
	public static TR_MAX: number = 14;

	public static VS_INFILTRATOR: number = 15;
	public static VS_LIGHT_ASSAULT: number = 17;
	public static VS_MEDIC: number = 18;
	public static VS_ENGINEER: number = 19;
	public static VS_HEAVY_ASSAULT: number = 20;
	public static VS_MAX: number = 21;

	public static NS_INFILTRATOR: number = 28;
	public static NS_LIGHT_ASSAULT: number = 29;
	public static NS_MEDIC: number = 30;
	public static NS_ENGINEER: number = 31;
	public static NS_HEAVY_ASSAULT: number = 32;
	public static NS_MAX: number = 45;

	public static isInfiltrator(id: number): boolean {
		return id == this.NC_INFILTRATOR
			|| id == this.TR_INFILTRATOR
			|| id == this.VS_INFILTRATOR
			|| id == this.NS_INFILTRATOR;
	}

	public static isLightAssault(id: number): boolean {
		return id == this.NC_LIGHT_ASSAULT
			|| id == this.TR_LIGHT_ASSAULT
			|| id == this.VS_LIGHT_ASSAULT
			|| id == this.NS_LIGHT_ASSAULT;
	}

	public static isMedic(id: number): boolean {
		return id == this.NC_MEDIC
			|| id == this.TR_MEDIC
			|| id == this.VS_MEDIC
			|| id == this.NS_MEDIC;
	}

	public static isEngineer(id: number): boolean {
		return id == this.NC_ENGINEER
			|| id == this.TR_ENGINEER
			|| id == this.VS_ENGINEER
			|| id == this.NS_ENGINEER;
	}

	public static isHeavy(id: number): boolean {
		return id == this.NC_HEAVY_ASSAULT
			|| id == this.TR_HEAVY_ASSAULT
			|| id == this.VS_HEAVY_ASSAULT
			|| id == this.NS_HEAVY_ASSAULT;
	}

	public static isMax(id: number): boolean {
		return id == this.NC_MAX
			|| id == this.TR_MAX
			|| id == this.VS_MAX
			|| id == this.NS_MAX;
	}

}