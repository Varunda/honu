import { ItemCategory } from "api/ItemCategoryApi";
import { ExpandedKillEvent } from "api/KillStatApi";

export type FullKillEvent = ExpandedKillEvent & { itemCategory: ItemCategory | null };