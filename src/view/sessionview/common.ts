import { ItemCategory } from "api/ItemCategoryApi";
import { ExpandedKillEvent } from "api/KillStatApi";
import { PsVehicle } from "api/VehicleApi";
import { ExpandedVehicleDestroyEvent } from "api/VehicleDestroyEventApi";

export type FullKillEvent = ExpandedKillEvent & { itemCategory: ItemCategory | null };

export type FullVehicleDestroyEvent = ExpandedVehicleDestroyEvent & {
    itemCategory: ItemCategory | null;
};