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
                    {{entry.session.start | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Finish</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.session.end">
                        {{entry.session.end | moment}}
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
                    <span v-if="entry.session.end == null">
                        &lt;in progress&gt;
                    </span>

                    <span v-else>
                        {{(entry.session.end.getTime() - entry.session.start.getTime()) / 1000 | mduration}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Outfit</b>

                    <info-hover text="What outfit this character was in during this session. Not 100% accurate!"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.session.outfitID == null">
                        &lt;no outfit&gt;
                    </span>
                    <span v-else>
                        <a :href="'/o/' + entry.session.outfitID">
                            <span v-if="entry.outfit == null">
                                &lt;missing outfit {{entry.session.outfitID}}&gt;
                            </span>
                            <span v-else>
                                [{{entry.outfit.tag}}] {{entry.outfit.name}}
                            </span>
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.session.id">
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
    import { PsOutfit } from "api/OutfitApi";
    import { Session, SessionBlock, SessionApi } from "api/SessionApi";

    type SessionOutfit = {
        session: Session;
        outfit: PsOutfit | null;
    }

    export const CharacterSessions = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                block: Loadable.idle() as Loading<SessionBlock>,

                showAll: false as boolean
            }
        },

        beforeMount: function(): void {
            this.loadSessions();
        },

        methods: {
            loadSessions: async function(): Promise<void> {
                this.block = Loadable.loading();
                this.block = await SessionApi.getBlockByCharacterID(this.character.id);
                if (this.block.state == "loaded") {
                    this.block.data.sessions = this.block.data.sessions.sort((a, b) => b.id - a.id);
                }
            }
        },

        computed: {
            filteredSessions: function(): Loading<SessionOutfit[]> {
                if (this.block.state != "loaded") {
                    return Loadable.rewrap(this.block);
                }

                let sessions: Session[] = [];

                if (this.showAll == true) {
                    sessions = this.block.data.sessions;
                } else {
                    sessions = this.block.data.sessions.filter(iter => {
                        // always show in progress sessions, even if it's only say a second old
                        if (iter.end == null) {
                            return true;
                        }

                        const end: number = (iter.end ?? new Date()).getTime();
                        const start: number = iter.start.getTime();

                        return (end - start) > 1000 * 60 * 5;
                    });
                }

                return Loadable.loaded(sessions.map(iter => {
                    if (this.block.state != "loaded") {
                        throw `how did block get unloaded?`;
                    }

                    return {
                        session: iter,
                        outfit: iter.outfitID == null ? null : (this.block.data.outfits.get(iter.outfitID) ?? null)
                    };
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
