﻿
export default class ZoneUtils {

    public static readonly Indar: number = 2;

    public static readonly Hossin: number = 4;

    public static readonly Amerish: number = 6;

    public static readonly Esamir: number = 8;

    public static readonly Oshur: number = 344;

    public static getZoneName(zoneID: number): string {
        const defID: number = zoneID & 0xFFFF;
        const instanceID: number = (zoneID & 0xFFFF0000) >> 16;

        switch (defID) {
            case 2: return "Indar";
            case 4: return "Hossin";
            case 6: return "Amerish";
            case 8: return "Esamir";
            case 10: return (instanceID > 0) ? `Nexus (instance ${instanceID})` : "Nexus";
            case 344: return "Oshur";

            case 14: return "Koltyr";
            case 361: return (instanceID > 0) ? `Desolation (instance ${instanceID})` : "Desolation";
            case 362: return "Sancutary";

            case 96: return "VR training (NC)";
            case 97: return "VR training (TR)";
            case 98: return "VR training (VS)";

            case 364: return (instanceID > 0) ? `Tutorial (instance ${instanceID})` : "Tutorial";
        }

        return `Unchecked zone ID ${zoneID}`;
    }

}