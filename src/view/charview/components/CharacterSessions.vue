<template>
    <div>
        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <hr class="border" />

        <table class="table table-sm">
            <thead>
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

            <tbody v-else-if="sessions.state == 'loaded'">
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
                        <a :href="'/session/' + session.id">
                            View
                        </a>
                    </td>
                </tr>
            </tbody>
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
                try {
                    this.sessions = Loadable.loaded((await SessionApi.getByCharacterID(this.character.id)).sort((a, b) => b.id - a.id));
                } catch (err: any) {
                    this.sessions = Loadable.error(err);
                }
            }
        },

        computed: {

        }

    });
    export default CharacterSessions;

    (window as any).SessionApi = SessionApi;
</script>
