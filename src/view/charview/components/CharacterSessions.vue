<template>
    <div>
        <table class="table table-sm">
            <thead class="table-secondary">
                <tr>
                    <th>Start</th>
                    <th>Finish</th>
                    <th>Duration</th>
                    <th>View</th>
                </tr>
            </thead>

            <tbody v-if="sessions.state == 'idle'"></tbody>

            <tbody v-else-if="sessions.state == 'loading'">
                <tr>
                    <td colspan="4">Loading...</td>
                </tr>
            </tbody>

            <tbody v-else-if="sessions.state == 'loaded' && sessions.data.length > 0">
                <tr v-for="session in sessions.data" :key="session.id">
                    <td>{{session.start | moment}}</td>
                    <td>{{session.end | moment}}</td>
                    <td>
                        <span v-if="session.end == null">
                            &lt;In progress&gt;
                        </span>

                        <span v-else>
                            {{(session.end.getTime() - session.start.getTime()) / 1000 | mduration}}
                        </span>
                    </td>

                    <td>
                        <a :href="'/s/' + session.id">
                            View
                        </a>
                    </td>
                </tr>
            </tbody>

            <tr v-else-if="sessions.state == 'loaded' && sessions.data.length == 0">
                <td colspan="4">
                    No sessions recorded
                </td>
            </tr>
        </table>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

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
                sessions: Loadable.idle() as Loading<Session[]>
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

        }

    });
    export default CharacterSessions;
</script>
