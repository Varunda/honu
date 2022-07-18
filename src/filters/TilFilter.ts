import Vue from "vue";
import * as moment from "moment";

Vue.filter("til2", (date: Date): string => {
	const m = moment(date);
	const now = moment(Date.now()).utc();

	const years = m.diff(now, "years");
	const months = m.diff(now, "months") % 12;

	if (years > 0) {
		return `${years}Y ${months}M`;
	}

	const days = m.diff(now, "days");
	if (months > 0) {
		return `${months}M ${days % 30}d`;
	}

	const hours = m.diff(now, "hours") % 24;
	if (days > 0) {
		return `${days}d ${hours}h`;
	}

	const mins = m.diff(now, "minutes") % 60;
	if (hours > 0) {
        return `${hours}h ${mins}m`;
    }

	const secs = m.diff(now, "seconds") % 60;
	return `${mins}m ${secs}s`;
});
