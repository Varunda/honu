<template>
    <div>
        <div v-if="character == null">
            &lt;missing&gt;
        </div>

        <div v-else>
            <h5>
                <a :href="'/c/' + character.id">
                    {{character.name}}
                </a>
            </h5>

            <table class="table table-sm">
                <tr>
                    <th>Status</th>
                    <td>
                        <span v-if="status == 1">
                            Ok
                        </span>

                        <span v-else-if="status == 2" class="text-warning">
                            Does not exist
                        </span>

                        <span v-else-if="status == 3" class="text-danger">
                            Deleted
                        </span>

                        <span v-else-if="status == 4" class="text-danger">
                            Remade
                        </span>
                    </td>
                </tr>

                <tr>
                    <th>Outfit</th>
                    <td>
                        <span v-if="character.outfitID != null">
                            <a :href="'/o/' + character.outfitID">
                                <span v-if="character.outfitTag != null">
                                    [{{character.outfitTag}}]
                                </span>
                                {{character.outfitName}}
                            </a>
                        </span>
                        <span v-else>
                            &lt;no outfit&gt;
                        </span>
                    </td>
                </tr>

                <tr>
                    <th>Faction</th>
                    <td>
                        {{character.factionID | faction}}
                    </td>
                </tr>

                <tr>
                    <th>Last login</th>
                    <td>
                        {{character.dateLastLogin | moment}}
                    </td>
                </tr>

                <tr>
                    <th></th>
                    <td>
                        {{character.dateLastLogin | timeAgo}}
                    </td>
                </tr>

                <tr>
                    <th>Created</th>
                    <td>
                        {{character.dateCreated | moment}}
                    </td>
                </tr>
            </table>

            <div class="d-flex">
                <h5 class="flex-grow-1">
                    Sessions
                    <span v-if="sessions.state == 'loaded'">
                        ({{sessions.data.length}})
                    </span>
                </h5>

                <span @click="showAll = !showAll">
                    Show all
                </span>
            </div>

            <table class="table table-sm">
                <thead>
                    <tr class="table-secondary">
                        <th>Start</th>
                        <th>Duration</th>
                        <th>Link</th>
                    </tr>
                </thead>

                <tbody v-if="sessions.state == 'idle'"></tbody>
                
                <tbody v-else-if="sessions.state == 'loading'">
                    <tr>
                        <td colspan="3">Loading...</td>
                    </tr>
                </tbody>

                <tbody v-else-if="sessions.state == 'loaded'">
                    <tr v-for="session in showSessions">
                        <td>
                            <span :title="session.start | moment">
                                {{session.start | moment("YYYY-MM-DD")}}
                            </span>
                        </td>

                        <td>
                            {{((session.end || new Date()).getTime() - session.start.getTime()) / 1000 | mduration}}
                        </td>

                        <td>
                            <a :href="'/s/' + session.id">
                                view
                            </a>
                        </td>
                    </tr>

                </tbody>

                <tbody v-else-if="sessions.state == 'error'">
                    <tr class="table-danger">
                        <td colspan="3">
                            {{sessions.message}}
                        </td>
                    </tr>
                </tbody>

                <tbody v-else>
                    <tr class="table-danger">
                        <td colspan="3">
                            Unchecked state of sessions: {{sessions.state}}
                        </td>
                    </tr>
                </tbody>
            </table>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/FactionNameFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { Session, SessionApi } from "api/SessionApi";

    export const PsbNamedCharacterColumn = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter | null>, required: false },
            status: { type: Number, required: true }
        },

        data: function() {
            return {
                sessions: Loadable.idle() as Loading<Session[]>,

                showAll: false as boolean
            }
        },

        mounted: function() {
            this.loadAll();
        },

        methods: {

            loadAll: function(): void {
                this.loadSessions();
            },

            loadSessions: async function(): Promise<void> {
                if (this.character == null) {
                    return;
                }

                this.sessions = Loadable.loading();
                this.sessions = await SessionApi.getByCharacterID(this.character.id);

                if (this.sessions.state == "loaded") {
                    this.sessions.data = this.sessions.data.sort((a, b) => {
                        return (b.end ?? new Date()).getTime() - a.start.getTime();
                    });
                }
            }
        },

        computed: {
            showSessions: function(): Session[] {
                if (this.sessions.state != "loaded") {
                    return [];
                }

                if (this.showAll == true) {
                    return this.sessions.data;
                } else {
                    return this.sessions.data.slice(0, 8);
                }
            }
        },

        watch: {
            character: function(): void {
                this.loadAll();
            },
        },

        components: {

        }
    });
    export default PsbNamedCharacterColumn;
</script>