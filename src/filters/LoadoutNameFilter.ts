import Vue from "vue";
import LoadoutUtils from "../util/Loadout";

Vue.filter("loadoutName", (loadoutID: number): string => {
    return LoadoutUtils.getLoadoutName(loadoutID);
});