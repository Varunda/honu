
export default class ItemUtils {

    public static infantryCategoryIDs: number[] = [2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 17, 18, 19, 24, 100, 102, 103, 207, 157, 147, 139];

    public static maxCategoryIDs: number[] = [9, 10, 15, 16, 20, 21, 22, 23];

    public static airCategoryIDs: number[] = [110, 111, 112, 113, 115, 116, 117, 118, 121, 122, 125, 126, 127, 128, 138, 208, 209, 210];

    public static armorCategoryIDs: number[] = [109, 114, 118, 119, 120, 123, 124, 129, 130, 131, 132, 144, 145, 211, 212, 213, 214, 215];

    public static isInfantryWeaponCategoryID(id: number): boolean {
        return ItemUtils.infantryCategoryIDs.includes(id);
    }

    public static isMaxWeaponCategoryID(id: number): boolean {
        return ItemUtils.maxCategoryIDs.includes(id);
    }

    public static isAirWeaponCategoryID(id: number): boolean {
        return ItemUtils.airCategoryIDs.includes(id);
    }

    public static isArmorWeaponCategoryID(id: number): boolean {
        return ItemUtils.armorCategoryIDs.includes(id);
    }

}