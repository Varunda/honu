<template>
    <div>
        <a-table
            display-type="table" row-padding="compact" :striped="false" :hover="true"
            :entries="filteredSessions" :page-sizes="[50, 100, 200, 500]" :default-page-size="200">

            <a-col>
                <a-header>
                    <b>Start</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.start | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Finish</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.end">
                        {{entry.end | moment}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Duration</b>
                    <button type="button" class="btn btn-sm py-0 mx-2 border" @click="showAll = !showAll" :class="[ showAll ? 'btn-success' : 'btn-secondary' ]">
                        All
                    </button>

                    <info-hover text="Sessions under 5 minutes are hidden by default. Click 'All' to see all sessions">
                    </info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.end == null">
                        &lt;in progress&gt;
                    </span>

                    <span v-else>
                        {{(entry.end.getTime() - entry.start.getTime()) / 1000 | mduration}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.id">
                        View
                    </a>
                </a-body>
            </a-col>
        </a-table>

        <div class="text-center">
            <small>
                Sessions before 2021-07-23 are not tracked
            </small>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    import "filters/LocaleFilter";
    import "filters/FixedFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { Session, SessionApi } from "api/SessionApi";

    export const CharacterSessions = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                sessions: Loadable.idle() as Loading<Session[]>,

                showAll: false as boolean
            }
        },

        beforeMount: function(): void {
            this.loadSessions();
        },

        methods: {
            loadSessions: async function(): Promise<void> {
                this.sessions = Loadable.loading();
                this.sessions = await SessionApi.getByCharacterID(this.character.id);
                if (this.sessions.state == "loaded") {
                    this.sessions.data = this.sessions.data.sort((a, b) => b.id - a.id);
                }
            }
        },

        computed: {
            filteredSessions: function(): Loading<Session[]> {
                if (this.sessions.state != "loaded") {
                    return this.sessions;
                }

                if (this.showAll == true) {
                    return this.sessions;
                }

                return Loadable.loaded(this.sessions.data.filter(iter => {
                    const end: number = (iter.end ?? new Date()).getTime();
                    const start: number = iter.start.getTime();

                    return (end - start) > 1000 * 60 * 5;
                }));
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover
        }
    });
    export default CharacterSessions;
</script>
