<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/jaegernsa">Jaeger NSA</a>
            </li>
        </honu-menu>

        <div>
            <h2>
                Timestamp: {{when * 1000 | moment("YYYY-MM-DD hh:mm:ss A")}}
            </h2>
        </div>

        <div class="mb-2">
            <a :href="'/jaegernsa/' + (when - 3600)" class="btn btn-primary">
                -1 hour
            </a>
            <a :href="'/jaegernsa/' + (when - 300)" class="btn btn-primary">
                -5 min
            </a>
            <a :href="'/jaegernsa/' + (when - 60)" class="btn btn-primary">
                -1 min
            </a>
            <a :href="'/jaegernsa/' + (when + 60)" class="btn btn-primary">
                +1 min
            </a>
            <a :href="'/jaegernsa/' + (when + 300)" class="btn btn-primary">
                +5 min
            </a>
            <a :href="'/jaegernsa/' + (when + 3600)" class="btn btn-primary">
                +1 hour
            </a>
        </div>

        <a-table
            :entries="entries" :show-filters="true" display-type="table" :hover="true"
            default-sort-field="start" default-sort-order="asc">

            <a-col sort-field="characterName">
                <a-header>
                    <b>Character</b>
                </a-header>

                <a-filter method="input" type="string" field="characterName"
                          :conditions="[ 'contains' ]">
                </a-filter>

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
                    <b>Faction</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.character != null">
                        <faction :faction-id="entry.character.factionID"></faction>
                    </span>
                    <span v-else>
                        --
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="start">
                <a-header>
                    <b>Start</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.session.start | moment("YYYY-MM-DD hh:mm:ss A")}}
                </a-body>
            </a-col>

            <a-col sort-field="end">
                <a-header>
                    <b>End</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.session.end == null">
                        &lt;unfinished&gt;
                    </span>
                    <span v-else>
                        {{entry.session.end | moment("YYYY-MM-DD hh:mm:ss A")}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Duration</b>
                </a-header>

                <a-body v-slot="entry">
                    {{(((entry.session.end || new Date()).getTime() - entry.session.start.getTime()) / 1000) | mduration}}
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
    import "filters/FactionNameFilter";
    import CharacterUtils from "util/Character";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Faction from "components/Faction";

    import { ExpandedSession, SessionApi } from "api/SessionApi";

    type FlatSession = ExpandedSession & {
        start: Date;
        end: Date | null;
        characterName: string;
    };

    export const JaegerNsa = Vue.extend({
        props: {

        },

        data: function() {
            return {
                when: 0 as number,
                worldID: 19 as number, // default to jaeger

                entries: Loadable.idle() as Loading<FlatSession[]>
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

                const whenn: number = Number.parseInt(parts[2]);
                if (Number.isNaN(whenn) == false) {
                    this.when = whenn;
                    console.log(`From ${this.when}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${whenn}`;
                }

                if (parts.length >= 4) {
                    const worldID: number = Number.parseInt(parts[3]);
                    if (Number.isNaN(worldID) == true) {
                        throw `failed to parse parts[3] '${parts[3]}' into a number, got ${worldID}`;
                    } else {
                        this.worldID = worldID;
                    }
                }

            },

            bind: async function(): Promise<void> {
                if (this.when <= 0) {
                    return;
                }

                this.entries = Loadable.loading();

                const sessions: Loading<ExpandedSession[]> = await SessionApi.getByRange(this.when, this.worldID);
                if (sessions.state == "loaded") {
                    this.entries = Loadable.loaded(sessions.data.map((iter: ExpandedSession): FlatSession => {
                        const flat: FlatSession = {
                            ...iter,
                            start: iter.session.start,
                            end: iter.session.end,
                            characterName: ""
                        };
                        flat.characterName = CharacterUtils.display(flat.session.characterID, flat.character);

                        return flat;
                    }));
                } else {
                    this.entries = Loadable.rewrap(sessions);
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader, 
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Faction
        }
    });

    export default JaegerNsa;
</script>