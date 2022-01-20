
export default class ZoneUtils {

    public static getZoneName(zoneID: number): string {
        const defID: number = zoneID & 0xFFFF;
        const instanceID: number = (zoneID & 0xFFFF0000) >> 16;

        switch (defID) {
            case 2: return "Indar";
            case 4: return "Hossin";
            case 6: return "Amerish";
            case 8: return "Esamir";
            case 14: return "Koltyr";
            case 361: return (instanceID > 0) ? `Desolation (instance ${instanceID})` : "Desolation";
            case 362: return "Sancutary";
            case 96: return "VR training (NC)";
            case 97: return "VR training (TR)";
            case 98: return "VR training (VS)";
        }

        return `Unchecked zone ID ${zoneID}`;
    }

}