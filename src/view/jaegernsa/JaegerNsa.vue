<template>
    <div>
        <h2 class="wt-header">Jaeger NSA - {{(when * 1000) | moment}}</h2>

        <a-table
            :entries="entries" display-type="table" :hover="true">

            <a-col>
                <a-header>
                    <b>Character</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/c/' + entry.session.characterID">
                        <span v-if="entry.character == null">
                            &lt;missing {{entry.session.characterID}}&gt;
                        </span>

                        <span v-else>
                            <span v-if="entry.character.outfitID != null">
                                [{{entry.character.outfitTag}}]
                            </span>
                            {{entry.character.name}}
                        </span>
                    </a>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Start</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.session.start | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>End</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.session.end || new Date() | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Session</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.session.id">
                        {{entry.session.id}}
                    </a>
                </a-body>
            </a-col>
        </a-table>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    import { ExpandedSession, SessionApi } from "api/SessionApi";

    export const JaegerNsa = Vue.extend({
        props: {

        },

        data: function() {
            return {
                when: 0 as number,

                entries: Loadable.idle() as Loading<ExpandedSession[]>
            }
        },

        created: function(): void {
            this.getWhenFromUrl();
            this.bind();
        },

        methods: {
            getWhenFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");

                if (parts.length < 3) {
                    return;
                }

                console.log(parts);

                const whenn: number = Number.parseInt(parts[2]);
                if (Number.isNaN(whenn) == false) {
                    this.when = whenn;
                    console.log(`From ${this.when}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${whenn}`;
                }
            },

            bind: async function(): Promise<void> {
                if (this.when <= 0) {
                    return;
                }

                this.entries = Loadable.loading();
                this.entries = await SessionApi.getByRange(this.when, 19);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
        }
    });

    export default JaegerNsa;
</script>