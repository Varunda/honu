import Vue from "vue";

Vue.component("info-hover", {
	props: {
		text: { type: String, required: true }
	},

	data: function () {
		return {
			ID: 0 as number
		};
	},

	created: function () {
		this.ID = Math.floor(Math.random() * 10000);
	},

	mounted: function () {
		this.$nextTick(() => {
			$(`#info-hover-${this.ID}`).popover();
		});
	},

	template: `
		<span :id="'info-hover-' + ID"
				class="d-inline-block" data-toggle="popover" data-trigger="hover"
				:data-content="text"
				style="filter: invert(1);">

			<img src="/img/question-circle.svg" />
		</span>
	`
});