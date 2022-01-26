<template>
    <div>
        <div v-if="extra.state == 'idle'"></div>

        <div v-else-if="extra.state == 'loading'">
            <busy style="height: 1.5rem;"></busy>
            Loading...
        </div>

        <div v-else-if="extra.state == 'loaded'">
            <div v-for="set in extra.data" class="mb-3">
                <h2 class="wt-header">
                    {{set.name}}

                    <info-hover :text="set.description"></info-hover>
                </h2>

                <div class="d-flex">
                    <div v-for="stat in set.stats" class="card flex-grow-0 mr-3">
                        <div class="card-body">
                            <h5 class="card-title">
                                {{stat.name}}
                                <info-hover v-if="stat.description == null" icon="exclamation-circle" text="This stat has not been figured out"></info-hover>

                                <info-hover v-else :text=stat.description>
                                </info-hover>
                            </h5>
                            <p class="card-text">
                                {{stat.value | locale}}
                            </p>

                            <p class="card-text text-muted text-small">
                                Figured out by

                                <span v-if="stat.figuredOutBy == null">
                                    &lt;no one&gt;
                                </span>

                                <span v-else>
                                    {{stat.figuredOutBy}}

                                    <span v-if="stat.figuredOutOn != null">
                                        <br />on {{stat.figuredOutOn | moment("YYYY-MM-DD")}}
                                    </span>
                                </span>
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <hr class="border" />

            <div class="d-flex mb-2">
                <div class="flex-grow-1"></div>

                <div>
                    These numbers are meaningless, and have no statistical bearing on performance, or how much they rely on <i>what could be considered cheese</i>, or other such ways to play.
                    <br />
                    Fun stats are inspired by DasAnfall's fun stats.
                    What each stat is may or may not be known. 
                    The calculation for the stats is NOT available in the <a href="https://github.com/varunda/honu">source code</a>.
                    <br />
                    All the numbers used to calculate these stats are displayed somewhere in this character viewer. 
                    <br />
                    If you think you've figured out what a stat is, contact the dev.
                    If the guess is correct, the stat will be updated to reflect who figured it out
                </div>

                <div class="flex-grow-1"></div>
            </div>

        </div>

        <div v-else-if="extra.state == 'error'">
            Error loading extra stats: {{extra.message}}
        </div>

        <div v-else class="text-danger">
            Unchecked state of extra: '{{extra.state}}'
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";

    import "MomentFilter";
    import "filters/LocaleFilter";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter } from "api/CharacterApi";
    import { ExtraStatSet, CharacterStatBase, CharacterExtraStatApi } from "api/CharacterExtraStatApi";

    export const CharacterExtraStats = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                extra: Loadable.idle() as Loading<ExtraStatSet[]>
            }
        },

        mounted: function(): void {
            this.bindExtra();
        },

        methods: {
            bindExtra: async function(): Promise<void> {
                this.extra = Loadable.loading();
                this.extra = await CharacterExtraStatApi.getByCharacterID(this.character.id);
            }
        },

        components: {
            InfoHover,
            Busy
        }
    });

    export default CharacterExtraStats;
</script>