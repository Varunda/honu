import Vue from "vue";

import ZoneUtils from "util/Zone";

Vue.filter("zone", (zoneID: number): string => {
	return ZoneUtils.getZoneName(zoneID);
});