<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="/character">Character</a>

                <span>/</span>

                <span v-if="character.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <a v-else-if="character.state == 'loaded'" :href="'/c/' + character.data.id + '/sessions'">
                    {{character.data.name}}
                </a>

                <span>/ Session {{sessionID}}</span>
            </h1>
        </div>

        <hr class="border" />

        <div>
            <h2>Session</h2>
            <hr class="border" />

            <div v-if="session.state == 'idle'"></div>
            <div v-else-if="session.state == 'loading'">
                Loading...
            </div>

            <div v-else-if="session.state == 'error'" class="text-danger">
                Error loading session {{sessionID}}: {{session.message}}
            </div>

            <div v-else-if="session.state == 'loaded'">
                <table class="table table-sm w-auto">
                    <tr>
                        <td><b>ID</b></td>
                        <td>
                            {{session.data.id}}
                        </td>
                    </tr>

                    <tr v-if="character.state == 'loaded'">
                        <td><b>Character</b></td>
                        <td>
                            <a :href="'/c/' + character.data.id">
                                <span v-if="character.data.outfitID != null">
                                    [{{character.data.outfitTag}}]
                                </span>
                                {{character.data.name}}
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Start</b></td>
                        <td>
                            {{session.data.start | moment}}
                        </td>
                    </tr>

                    <tr>
                        <td><b>End</b></td>
                        <td>
                            <span v-if="session.data.end == null">
                                &lt;In progress&gt;
                            </span>
                            <span v-else>
                                {{session.data.end | moment}}
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Duration</b></td>
                        <td>
                            {{durationInSeconds | mduration}}
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div>
            <h2>General</h2>
            <hr class="border" />

            <div v-if="kills.state == 'idle'"></div>
            <div v-else-if="kills.state == 'loading'">
                Loading...
            </div>

            <div v-else-if="kills.state == 'error'" class="text-danger">
                Error loading kills: {{kills.message}}
            </div>

            <div v-else-if="kills.state == 'loaded'">
                <table class="table table-sm w-auto">
                    <tr>
                        <td><b>Kill count</b></td>
                        <td>
                            {{kills.data.length | locale}}
                        </td>
                    </tr>

                    <tr>
                        <td><b>HSR</b></td>
                        <td>
                            {{(kills.data.filter(iter => iter.event.isHeadshot == true).length / kills.data.length) * 100 | fixed | locale}}%
                        </td>
                    </tr>

                    <tr>
                        <td><b>KPM</b></td>
                        <td>
                            {{kills.data.length / durationInSeconds * 60 | fixed | locale}}
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div>
            <h2>Kills</h2>
            <hr class="border" />

            <div v-if="kills.state == 'loading'">
                Loading...
            </div>

            <div v-else-if="kills.state == 'loaded'">
                <session-viewer-kills :kills="kills"></session-viewer-kills>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import SessionViewerKills from "./components/SessionViewerKills.vue";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { PsItem, ItemApi } from "api/ItemApi";

    export const SessionViewer = Vue.extend({
        props: { },

        data: function() {
            return {
                sessionID: 0 as number,

                session: Loadable.idle() as Loading<Session>,
                character: Loadable.idle() as Loading<PsCharacter>,
                kills: Loadable.idle() as Loading<ExpandedKillEvent[]>,
                exp: Loadable.idle() as Loading<ExpEvent[]>,

                charts: {
                    kills: null as Chart | null
                }
            }
        },

        beforeMount: function(): void {
            this.getSessionIDFromUrl();
            this.bindAll();
        },

        methods: {
            getSessionIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const sessionID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(sessionID) == false) {
                    this.sessionID = sessionID;
                    console.log(`Session ID is ${this.sessionID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${sessionID}`;
                }
            },

            bindAll: function(): void {
                this.bindSession();
                // Character is not bound, cause it uses the .characterID field from the session, so it's done when session is bound
                this.bindKills();
                this.bindExp();
            },

            bindSession: async function(): Promise<void> {
                this.session = Loadable.loading();
                try {
                    this.session = Loadable.loaded(await SessionApi.getBySessionID(this.sessionID));
                    this.bindCharacter();
                } catch (err: any) {
                    this.session = Loadable.error(err);
                }
            },

            bindCharacter: async function(): Promise<void> {
                if (this.session.state != "loaded") {
                    this.character = Loadable.idle();
                    return;
                }

                this.character = Loadable.loading();
                try {
                    const c: PsCharacter | null = await CharacterApi.getByID(this.session.data.characterID);
                    if (c != null) {
                        this.character = Loadable.loaded(c);
                    } else {
                        this.character = Loadable.nocontent();
                    }
                } catch (err: any) {
                    this.character = Loadable.error(err);
                }
            },

            bindExp: async function(): Promise<void> {
                this.exp = Loadable.loading();
                try {
                    this.exp = Loadable.loaded(await ExpStatApi.getBySessionID(this.sessionID));
                } catch (err: any) {
                    this.exp = Loadable.error(err);
                }

            },

            bindKills: async function(): Promise<void> {
                this.kills = Loadable.loading();
                try {
                    this.kills = Loadable.loaded(await KillStatApi.getSessionKills(this.sessionID));
                } catch (err: any) {
                    this.kills = Loadable.error(err);
                }
            }

        },

        computed: {
            durationInSeconds: function(): number {
                if (this.session.state != "loaded") {
                    return -1;
                }
                return ((this.session.data.end || new Date()).getTime() - this.session.data.start.getTime()) / 1000;
            }
        },

        components: {
            SessionViewerKills
        }

    });
    export default SessionViewer;
</script>