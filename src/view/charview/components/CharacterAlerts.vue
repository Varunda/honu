<template>
    <div>
        <div class="alert alert-primary text-center">
            This data is populated only AFTER and  alert is finished
        </div>

        <div class="border mb-3 mt-0 rounded">
            <collapsible header-text="Column selection" size-class="h4" class="mb-0">
                <div class="pl-3">
                    <h5 class="mb-0">General</h5>

                    <div class="btn-group mb-2">
                        <toggle-button v-model="columns.kills">
                            Kills
                        </toggle-button>

                        <toggle-button v-model="columns.deaths">
                            Deaths
                            <info-hover text="Revives remove a death"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.kd">
                            K/D
                            <info-hover text="Revives remove a death"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.kpm">
                            KPM
                        </toggle-button>

                        <toggle-button v-model="columns.vkills">
                            Vehicle kills
                        </toggle-button>

                        <toggle-button v-model="columns.vkpm">
                            V.Kills/Min
                        </toggle-button>

                        <toggle-button v-model="columns.spawns">
                            Spawns
                        </toggle-button>

                        <toggle-button v-model="columns.spawnsPerMinute">
                            Spawns/Min
                        </toggle-button>
                    </div>

                    <h5 class="mb-0">Medic</h5>

                    <div class="btn-group mb-2">
                        <toggle-button v-model="columns.heals">
                            Heals
                        </toggle-button>

                        <toggle-button v-model="columns.healsPerMinute">
                            Heals/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.revives">
                            Revives
                        </toggle-button>

                        <toggle-button v-model="columns.revivesPerMinute">
                            Revive/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.shieldRepairs">
                            Shield reps
                        </toggle-button>

                        <toggle-button v-model="columns.shieldRepairsPerMinute">
                            Shield reps/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>
                    </div>

                    <h5 class="mb-0">Engineer</h5>

                    <div class="btn-group pb-2">
                        <toggle-button v-model="columns.maxRepairs">
                            MAX repairs
                        </toggle-button>

                        <toggle-button v-model="columns.maxRepairsPerMinute">
                            MAX repairs/Min
                            <info-hover text="This is not per minute of engineer, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.resupplies">
                            Resupplies
                        </toggle-button>

                        <toggle-button v-model="columns.resuppliesPerMinute">
                            Resupplies/Min
                            <info-hover text="This is not per minute of engineer, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>
                    </div>
                </div>
            </collapsible>
        </div>

        <a-table v-if="showTable"
            :entries="data"
            :striped="true"
            default-sort-field="timestamp" default-sort-order="desc"
            display-type="table">

            <a-col>
                <a-header>
                    <b>Alert</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/alert/' + entry.alertID">
                        {{entry.displayName}}
                    </a>
                </a-body>
            </a-col>

            <a-col sort-field="timestamp">
                <a-header>
                    <b>Timestamp</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | moment}}
                </a-body>
            </a-col>

            <a-col sort-field="percentPlayed">
                <a-header>
                    <b>Time played</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.secondsOnline | mduration}} / {{entry.duration | mduration}}

                    <span class="text-muted">
                        ({{entry.percentPlayed * 100 | locale(2)}}%)
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Alert type</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.zoneID != 0">
                        {{entry.metagameAlertName}}
                    </span>
                    <span v-else class="text-muted">
                        Daily alert (honu specific)
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Winner</b>
                </a-header>

                <a-body v-slot="entry">

                    <div v-if="entry.zoneID == 0">
                        <faction-image :faction-id="4" style="height: 1.25rem;"></faction-image>
                    </div>

                    <div v-else-if="entry.victorFactionID != null">
                        <faction-image :faction-id="entry.victorFactionID" style="height: 1.25rem"></faction-image>
                    </div>
                    <div v-else>
                        ?
                    </div>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Continent</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.zoneID == 0">
                        --
                    </span>
                    <span v-else>
                        {{entry.zoneID | zone}}
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.kills" sort-field="kills">
                <a-header>
                    <b>Kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kills | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.deaths" sort-field="deaths">
                <a-header>
                    <b>Deaths</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.deaths | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.kd" sort-field="kd">
                <a-header>
                    <b>K/D</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kd | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.kpm" sort-field="kpm">
                <a-header>
                    <b>KPM</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kpm | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.vkills" sort-field="vkills">
                <a-header>
                    <b>V.Kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.vkills | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.vkpm" sort-field="vkpm">
                <a-header>
                    <b>V.KPM</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.vkpm | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.spawns" sort-field="spawns">
                <a-header>
                    <b>Spawns</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.spawns | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.spawnsPerMinute" sort-field="spawnsPerMinute">
                <a-header>
                    <b>Spawns/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.spawnsPerMinute | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.heals" sort-field="heals">
                <a-header>
                    <b>Heals</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.heals | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.healsPerMinute" sort-field="healsPerMinute">
                <a-header>
                    <b>Heals/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.healsPerMinute | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.revives" sort-field="revives">
                <a-header>
                    <b>Revives</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.revives | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.revivesPerMinute" sort-field="revivesPerMinute">
                <a-header>
                    <b>Revives/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.revivesPerMinute | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.shieldRepairs" sort-field="shieldRepairs">
                <a-header>
                    <b>Shield reps</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.shieldRepairs | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.shieldRepairsPerMinute" sort-field="shieldRepairsPerMinute">
                <a-header>
                    <b>SHield reps/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.shieldRepairsPerMinute | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.maxRepairs" sort-field="maxRepairs">
                <a-header>
                    <b>MAX repairs</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.maxRepairs | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.maxRepairsPerMinute" sort-field="maxRepairsPerMinute">
                <a-header>
                    <b>MAX repairs/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.maxRepairsPerMinute | locale(2)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.resupplies" sort-field="resupplies">
                <a-header>
                    <b>Resupplies</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.resupplies | locale(0)}}
                </a-body>
            </a-col>

            <a-col v-if="columns.resuppliesPerMinute" sort-field="resuppliesPerMinute">
                <a-header>
                    <b>Resupplies/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.resuppliesPerMinute | locale(2)}}
                </a-body>
            </a-col>

        </a-table>

        <div class="text-center">
            <small>
                Alerts before 2021-07-23 are not tracked
            </small>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ToggleButton from "components/ToggleButton";
    import Collapsible from "components/Collapsible.vue";
    import FactionImage from "components/FactionImage";

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/FixedFilter";
    import "filters/ZoneNameFilter";
    import "filters/FactionNameFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterAlertBlock, AlertParticipantApi, CharacterAlertPlayer } from "api/AlertParticipantApi";

    type FlatCharacterAlertPlayer = {
        alertID: number;
        timestamp: Date;
        victorFactionID: number | null;

        displayName: string;
        metagameAlertName: string;

        zoneID: number;
        secondsOnline: number;
        duration: number;
        percentPlayed: number;

        kills: number;
        vkills: number;
        deaths: number;
        kd: number;
        kpm: number;
        vkpm: number;

        spawns: number;
        spawnsPerMinute: number;

        heals: number;
        revives: number;
        shieldRepairs: number;
        healsPerMinute: number;
        revivesPerMinute: number;
        shieldRepairsPerMinute: number;

        maxRepairs: number;
        resupplies: number;
        maxRepairsPerMinute: number;
        resuppliesPerMinute: number;
    }

    export const CharacterAlerts = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                alerts: Loadable.idle() as Loading<CharacterAlertBlock>,

                showTable: true as boolean,

                columns: {
                    kills: true as boolean,
                    deaths: true as boolean,
                    kd: true as boolean,
                    kpm: true as boolean,

                    vkills: false as boolean,
                    vkpm: false as boolean,

                    spawns: false as boolean,
                    spawnsPerMinute: false as boolean,

                    heals: false as boolean,
                    revives: false as boolean,
                    shieldRepairs: false as boolean,
                    healsPerMinute: false as boolean,
                    revivesPerMinute: false as boolean,
                    shieldRepairsPerMinute: false as boolean,

                    resupplies: false as boolean,
                    maxRepairs: false as boolean,
                    resuppliesPerMinute: false as boolean,
                    maxRepairsPerMinute: false as boolean
                }
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.bind();
            });
        },

        methods: {
            bind: async function(): Promise<void> {
                this.alerts = Loadable.loading();
                this.alerts = await AlertParticipantApi.getByCharacterID(this.character.id);
            }
        },

        computed: {

            data: function(): Loading<FlatCharacterAlertPlayer[]> {
                if (this.alerts.state != "loaded") {
                    return Loadable.rewrap(this.alerts);
                }

                const alerts: CharacterAlertBlock = this.alerts.data;

                return Loadable.loaded(this.alerts.data.alerts.map(iter => {
                    const flat: FlatCharacterAlertPlayer = {
                        alertID: iter.alertID,
                        timestamp: iter.timestamp,
                        victorFactionID: iter.victorFactionID,

                        displayName: "",
                        metagameAlertName: "",

                        zoneID: iter.zoneID,
                        secondsOnline: iter.secondsOnline,
                        duration: iter.duration,
                        percentPlayed: iter.secondsOnline / Math.max(1, iter.duration),

                        spawns: iter.spawns,
                        spawnsPerMinute: iter.spawns / Math.max(1, iter.secondsOnline) * 60,

                        kills: iter.kills,
                        vkills: iter.vehicleKills,
                        deaths: iter.deaths,
                        kd: iter.kills / Math.max(1, iter.deaths),
                        kpm: iter.kills / Math.max(1, iter.secondsOnline) * 60,
                        vkpm: iter.vehicleKills / Math.max(1, iter.secondsOnline) * 60,

                        heals: iter.heals,
                        revives: iter.revives,
                        shieldRepairs: iter.shieldRepairs,

                        healsPerMinute: iter.heals / Math.max(1, iter.secondsOnline) * 60,
                        revivesPerMinute: iter.revives / Math.max(1, iter.secondsOnline) * 60,
                        shieldRepairsPerMinute: iter.shieldRepairs / Math.max(1, iter.secondsOnline) * 60,

                        resupplies: iter.resupplies,
                        maxRepairs: iter.repairs,
                        resuppliesPerMinute: iter.resupplies / Math.max(1, iter.secondsOnline) * 60,
                        maxRepairsPerMinute: iter.repairs / Math.max(1, iter.secondsOnline) * 60
                    };

                    if (iter.name != "") {
                        flat.displayName = iter.name;
                    } else {
                        flat.displayName = `${iter.worldID}-${iter.instanceID}`;
                    }

                    flat.metagameAlertName = alerts.metagameEvents.get(iter.metagameAlertID)?.name
                        ?? `<missing ${iter.metagameAlertID}>`;

                    return flat;
                }));
            }

        },

        watch: {
            columns: {
                deep: true,
                handler: async function(): Promise<void> {
                    // force the table to be destroyed then re-created
                    this.showTable = false;
                    await this.$nextTick();
                    this.showTable = true;
                }
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            Busy, ToggleButton, InfoHover, Collapsible,
            FactionImage
        },
    });
    export default CharacterAlerts;
</script>