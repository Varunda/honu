import Vue from "vue";

Vue.component("continent-metadata", {
	props: {
		metadata: { required: false }
	},

	template: `
		<span v-if="metadata != null">
			<span v-if="metadata.isOpened == false">
				<span class="fas fa-lock" title="This continent is locked"></span>
			</span>

			<span v-if="metadata.alertEnd != null">
				<span class="fas fa-exclamation-triangle" title="Active alert!"></span>
				Locks
				{{metadata.alertEnd | til}}
			</span>
		</span>
	`

});