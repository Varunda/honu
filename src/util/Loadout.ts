
export default class LoadoutUtils {

	public static NAME_INFILTRATOR: string = "infiltrator";
	public static NAME_LIGHT_ASSAULT: string = "light assault";
	public static NAME_MEDIC: string = "medic";
	public static NAME_ENGINEER: string = "engineer";
	public static NAME_HEAVY_ASSAULT: string = "heavy assault";
	public static NAME_MAX: string = "MAX";

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

	public static getFactionID(loadoutID: number): number {
        switch (loadoutID) {
            case LoadoutUtils.NC_INFILTRATOR:
            case LoadoutUtils.NC_LIGHT_ASSAULT:
            case LoadoutUtils.NC_MEDIC:
            case LoadoutUtils.NC_ENGINEER:
            case LoadoutUtils.NC_HEAVY_ASSAULT:
            case LoadoutUtils.NC_MAX:
                return 2;

            case LoadoutUtils.TR_INFILTRATOR:
            case LoadoutUtils.TR_LIGHT_ASSAULT:
            case LoadoutUtils.TR_MEDIC:
            case LoadoutUtils.TR_ENGINEER:
            case LoadoutUtils.TR_HEAVY_ASSAULT:
            case LoadoutUtils.TR_MAX:
                return 3;

            case LoadoutUtils.VS_INFILTRATOR:
            case LoadoutUtils.VS_LIGHT_ASSAULT:
            case LoadoutUtils.VS_MEDIC:
            case LoadoutUtils.VS_ENGINEER:
            case LoadoutUtils.VS_HEAVY_ASSAULT:
            case LoadoutUtils.VS_MAX:
                return 1;

            case LoadoutUtils.NS_INFILTRATOR:
            case LoadoutUtils.NS_LIGHT_ASSAULT:
            case LoadoutUtils.NS_MEDIC:
            case LoadoutUtils.NS_ENGINEER:
            case LoadoutUtils.NS_HEAVY_ASSAULT:
            case LoadoutUtils.NS_MAX:
                return 4;

			default: return -1;
        }
    }

	public static getLoadoutName(loadoutID: number): string {
        switch (loadoutID) {
            case LoadoutUtils.VS_INFILTRATOR:
            case LoadoutUtils.NC_INFILTRATOR:
            case LoadoutUtils.TR_INFILTRATOR:
			case LoadoutUtils.NS_INFILTRATOR:
				return this.NAME_INFILTRATOR;

            case LoadoutUtils.NC_ENGINEER:
            case LoadoutUtils.TR_ENGINEER:
            case LoadoutUtils.VS_ENGINEER:
			case LoadoutUtils.NS_ENGINEER:
				return this.NAME_ENGINEER;

            case LoadoutUtils.NC_MAX:
            case LoadoutUtils.TR_MAX:
            case LoadoutUtils.VS_MAX:
			case LoadoutUtils.NS_MAX:
				return this.NAME_MAX;

            case LoadoutUtils.TR_LIGHT_ASSAULT:
            case LoadoutUtils.NC_LIGHT_ASSAULT:
            case LoadoutUtils.VS_LIGHT_ASSAULT:
			case LoadoutUtils.NS_LIGHT_ASSAULT:
				return this.NAME_LIGHT_ASSAULT;

            case LoadoutUtils.TR_MEDIC:
            case LoadoutUtils.NC_MEDIC:
            case LoadoutUtils.VS_MEDIC:
			case LoadoutUtils.NS_MEDIC:
				return this.NAME_MEDIC;

            case LoadoutUtils.TR_HEAVY_ASSAULT:
            case LoadoutUtils.NC_HEAVY_ASSAULT:
            case LoadoutUtils.VS_HEAVY_ASSAULT:
			case LoadoutUtils.NS_HEAVY_ASSAULT:
				return this.NAME_HEAVY_ASSAULT;

			default: return `unchecked loudoutID ${loadoutID}`;
        }

    }

}
