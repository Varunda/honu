<template>
    <div>
        <div class="d-flex align-items-center">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Realtime 
                    {{worldID | world}}
                </li>
            </honu-menu>

            <div>
                <table class="table table-sm">
                    <tr>
                        <td>Socket status:</td>
                        <td>{{socketState}}</td>
                    </tr>

                    <tr>
                        <td>
                            Last socket update:
                            <info-hover text="When the last data was received from the server"></info-hover>
                        </td>
                        <td>
                            {{lastUpdate | moment("YYYY-MM-DD hh:mm:ssA")}}
                        </td>
                    </tr>

                    <tr>
                        <td>
                            Last data update:
                            <info-hover text="When the data was last updated by the server"></info-hover>
                        </td>
                        <td>
                            {{worldData.timestamp | moment("YYYY-MM-DD hh:mm:ssA")}}
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div class="mb-2">
            <a :href="'/realtimenetwork/' + worldID">
                View interaction map
            </a>
        </div>

        <div class="mb-2 text-center" @click="toggleDuration">
            <div v-if="useShort == true" class="alert alert-info">
                Currently viewing the past hour. Click here to view the last 2 hours
            </div>
            <div v-else class="alert alert-primary">
                Currently viewing the past 2 hours. Click here to view the last hour
            </div>
        </div>

        <div v-if="worldData != null" class="wt-grid-container" style="display: grid;">
            <div class="grid-health-warning">
                <div v-if="streamFailureCount > 0" class="alert alert-danger text-center h5">
                    The health check on the realtime event stream for this server has failed {{streamFailureCount}} times!
                    <br />
                    Honu is currently attempting to reconnect, but all events from this server will be missing

                    <div class="h6">
                        Last event received was at {{streamMostRecentEvent | moment("YYYY-MM-DD hh:mm:ssA")}}
                    </div>
                </div>

                <div v-if="badStreams.length > 0" class="alert alert-warning text-center h5">
                    <div data-toggle="collapse" data-target="#reconnect-info">
                        Honu reconnected to the API due to a bad realtime event stream from this server, this has caused:
                    </div>

                    <ul class="d-inline-block text-left mb-0">
                        <li v-for="stream in badStreams">
                            {{stream.secondsMissed | tduration}} of {{stream.streamType}} events to be missed
                        </li>
                    </ul>

                    <div id="reconnect-info" class="collapse hide border-top h6 mt-2 pt-2">
                        <div v-for="reconnect in worldData.reconnects">
                            {{reconnect.streamType}}@{{reconnect.timestamp | moment("YYYY-MM-DD hh:mm:ssA")}} - {{reconnect.duration | mduration}}
                        </div>
                    </div>
                </div>

                <div v-if="worldData.processLag > 30" class="alert alert-warning text-center h5">
                    Honu is currently {{worldData.processLag | mduration}} behind on events
                    <div class="h6 mt-2 mb-1">
                        Honu is receiving events faster than it can process them. No data has been lost,
                        but reporting will be behind
                    </div>
                </div>

                <div v-if="worldData.lastError != null" class="alert alert-warning text-center h5">
                    An error occured during the last update: {{worldData.lastError.type}}

                    <div class="h6">
                        honu will automatically try again
                    </div>

                    <a href="javascript:void(0);" @click="showErrorDetails = !showErrorDetails">Toggle details</a>
                    <div class="d-flex">
                        <div class="flex-grow-1"></div>
                        <div v-if="showErrorDetails" class="h6 mt-2 mb-1 text-left flex-grow-0">
                            <pre>{{worldData.lastError.detail}}</pre>
                        </div>
                        <div class="flex-grow-1"></div>
                    </div>
                </div>
            </div>

            <div class="grid-continent-population">
                <h4>
                    {{worldID | world}}
                    player count
                    <info-hover text="Continents are hidden if they have less than 20 players"></info-hover>
                </h4>

                <table id="world_pop" class="table" style="table-layout: fixed; text-align: center;">
                    <tr>
                        <th colspan="5" class="border-right">
                            Server
                            <info-hover text="Players currently online"></info-hover>
                        </th>
                        <th colspan="5" v-if="indarCount > 19" class="border-right">
                            <a :href="'/realtimemap?worldID=' + worldID + '&zoneID=2&showUI=true'">Indar</a>
                            <continent-metadata :metadata="worldData.continentCount.indar.metadata"></continent-metadata>
                        </th>
                        <th colspan="5" v-if="hossinCount > 19" class="border-right">
                            <a :href="'/realtimemap?worldID=' + worldID + '&zoneID=4&showUI=true'">Hossin</a>
                            <continent-metadata :metadata="worldData.continentCount.hossin.metadata"></continent-metadata>
                        </th>
                        <th colspan="5" v-if="amerishCount > 19" class="border-right">
                            <a :href="'/realtimemap?worldID=' + worldID + '&zoneID=6&showUI=true'">Amerish</a>
                            <continent-metadata :metadata="worldData.continentCount.amerish.metadata"></continent-metadata>
                        </th>
                        <th colspan="5" v-if="esamirCount > 19" class="border-right">
                            <a :href="'/realtimemap?worldID=' + worldID + '&zoneID=8&showUI=true'">Esamir</a>
                            <continent-metadata :metadata="worldData.continentCount.esamir.metadata"></continent-metadata>
                        </th>
                        <th colspan="5" v-if="oshurCount > 19" class="border-right">
                            <a :href="'/realtimemap?worldID=' + worldID + '&zoneID=344&showUI=true'">Oshur</a>
                            <continent-metadata :metadata="worldData.continentCount.oshur.metadata"></continent-metadata>
                        </th>
                        <th colspan="5">
                            Other
                            <info-hover text="Zones such as Koltyr, Desolation or Santuary"></info-hover>
                        </th>
                    </tr>

                    <tr>
                        <!-- All across the server -->
                        <td>All: {{totalCount}}</td>
                        <td>{{totalVSCount}} VS</td>
                        <td>{{totalNCCount}} NC</td>
                        <td>{{totalTRCount}} TR</td>
                        <td class="border-right">{{totalNSCount}} NS</td>

                        <template v-if="indarCount > 19">
                            <td>All: {{indarCount}}</td>
                            <td>{{worldData.continentCount.indar.vs}} VS</td>
                            <td>{{worldData.continentCount.indar.nc}} NC</td>
                            <td>{{worldData.continentCount.indar.tr}} TR</td>
                            <td class="border-right">{{worldData.continentCount.indar.ns}} NS</td>
                        </template>

                        <template v-if="hossinCount > 19">
                            <td>All: {{hossinCount}}</td>
                            <td>{{worldData.continentCount.hossin.vs}} VS</td>
                            <td>{{worldData.continentCount.hossin.nc}} NC</td>
                            <td>{{worldData.continentCount.hossin.tr}} TR</td>
                            <td class="border-right">{{worldData.continentCount.hossin.ns}} NS</td>
                        </template>

                        <template v-if="amerishCount > 19">
                            <td>All: {{amerishCount}}</td>
                            <td>{{worldData.continentCount.amerish.vs}} VS</td>
                            <td>{{worldData.continentCount.amerish.nc}} NC</td>
                            <td>{{worldData.continentCount.amerish.tr}} TR</td>
                            <td class="border-right">{{worldData.continentCount.amerish.ns}} NS</td>
                        </template>

                        <template v-if="esamirCount > 19">
                            <td>All: {{esamirCount}}</td>
                            <td>{{worldData.continentCount.esamir.vs}} VS</td>
                            <td>{{worldData.continentCount.esamir.nc}} NC</td>
                            <td>{{worldData.continentCount.esamir.tr}} TR</td>
                            <td class="border-right">{{worldData.continentCount.esamir.ns}} NS</td>
                        </template>

                        <template v-if="oshurCount > 19">
                            <td>All: {{oshurCount}}</td>
                            <td>{{worldData.continentCount.oshur.vs}} VS</td>
                            <td>{{worldData.continentCount.oshur.nc}} NC</td>
                            <td>{{worldData.continentCount.oshur.tr}} TR</td>
                            <td class="border-right">{{worldData.continentCount.oshur.ns}} NS</td>
                        </template>

                        <!-- Other, currently unknown -->
                        <td>All: {{otherCount}}</td>
                        <td>{{worldData.continentCount.other.vs}} VS</td>
                        <td>{{worldData.continentCount.other.nc}} NC</td>
                        <td>{{worldData.continentCount.other.tr}} TR</td>
                        <td>{{worldData.continentCount.other.ns}} NS</td>
                    </tr>
                </table>
            </div>

            <h4 class="grid-title-focus">
                Current faction focus (5 mins)
                <info-hover text="What percentage of kills have come from the other factions within the last 5 minutes"></info-hover>
            </h4>

            <h4 class="grid-title-heals">
                Heals
            </h4>

            <h4 class="grid-title-revives">
                Revives
            </h4>

            <h4 class="grid-title-shields">
                Shield repairs
            </h4>

            <h4 class="grid-title-resupplies">
                Resupplies
            </h4>

            <h4 class="grid-title-spawns">
                Spawns &lt;3
            </h4>

            <h4 class="grid-title-weapon-kills">
                Weapon kills
            </h4>

            <h4 class="grid-title-vehicle-kills">
                Vehicle kills
                <info-hover text="Counts all vehicle kills except sunderer kills, that is a hill I will die on"></info-hover>
            </h4>

            <h4 class="grid-title-outfits">
                Outfits currently online
                <info-hover text="Only members who are online, not in the last 2 hours, like the kill list does"></info-hover>
            </h4>

            <h4 class="grid-title-vs p-1" id="header-vs">
                <img src="/img/logo_vs.png" style="width: 32px; "/>
                Vanu Sovereignty
                <info-hover text="NS kills only count when that player is on this faction"></info-hover>
            </h4>

            <div class="grid-vs-player-kills">
                <player-kill-block :block="worldData.vs" link="'/c/'" :use-short="useShort"></player-kill-block>
            </div>

            <div class="grid-vs-outfit-kills">
                <outfit-kill-block :block="worldData.vs.outfitKills"></outfit-kill-block>
            </div>

            <div class="grid-vs-focus">
                <faction-focus :focus="worldData.vs.factionFocus"></faction-focus>
            </div>

            <div class="grid-vs-heals d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerHeals" link="/c/"
                    :source="expSources.charHeal" source-title="Players healed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitHeals" title="Outfits" link="/o/"
                    :source="expSources.outfitHeal" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-revives d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerRevives" link="/c/"
                    :source="expSources.charRevive" source-title="Players revived" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitRevives" title="Outfits" link="/o/"
                    :source="expSources.outfitRevive" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-shields d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerShieldRepair" link="/c/"
                    :source="expSources.charShield" source-title="Players shielded" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitShieldRepair" link="/o/"
                    :source="expSources.outfitShield" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-resupplies d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerResupplies" link="/c/"
                    :source="expSources.charResupply" source-title="Players resupplies" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitResupplies" title="Outfits" link="/o/"
                    :source="expSources.outfitResupply" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-spawns d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerSpawns" link="/c/"
                    :source="expSources.charSpawn" source-title="Types of spawn" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitSpawns" title="Outfits" link="/o/"
                    :source="expSources.outfitSpawn" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-vehicle-kills d-flex">
                <block-view class="mr-3" :block="worldData.vs.playerVehicleKills" link="/c/"
                    :source="expSources.charVKills" source-title="Vehicles destroyed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.vs.outfitVehicleKills" title="Outfit" link="/o/"
                    :source="expSources.outfitVKills" :source-team-id="1" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <div class="grid-vs-weapon-kills">
                <weapon-kills :weapon-kills="worldData.vs.weaponKills"
                    :total="worldData.vs.totalKills">
                </weapon-kills>
            </div>

            <div class="grid-vs-outfits">
                <outfits-online :data="worldData.vs.outfits"></outfits-online>
            </div>

            <!-- NC -->

            <h4 class="grid-title-nc p-1" id="header-nc">
                <img src="/img/logo_nc.png" style="width: 32px; "/>
                New Conglomerate
                <info-hover text="NS kills only count when that player is on this faction"></info-hover>
            </h4>

            <div class="grid-nc-player-kills">
                <player-kill-block :block="worldData.nc" :use-short="useShort"></player-kill-block>
            </div>
            
            <div class="grid-nc-outfit-kills">
                <outfit-kill-block :block="worldData.nc.outfitKills"></outfit-kill-block>
            </div>

            <h4 class="grid-nc-title-focus">
                NC focus
            </h4>

            <div class="grid-nc-focus">
                <faction-focus :focus="worldData.nc.factionFocus"></faction-focus>
            </div>

            <h4 class="grid-nc-title-heals">
                Heals
            </h4>

            <div class="grid-nc-heals d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerHeals" link="/c/"
                    :source="expSources.charHeal" source-title="Players healed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitHeals" title="Outfits" link="/o/"
                    :source="expSources.outfitHeal" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-revives">
                Revives
            </h4>

            <div class="grid-nc-revives d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerRevives" link="/c/"
                    :source="expSources.charRevive" source-title="Players revived" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitRevives" title="Outfits" link="/o/"
                    :source="expSources.outfitRevive" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-shields">
                Shield repairs
            </h4>

            <div class="grid-nc-shields d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerShieldRepair" link="/c/"
                    :source="expSources.charShield" source-title="Players shielded" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitShieldRepair" link="/o/"
                    :source="expSources.outfitShield" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-resupplies">
                Resupplies
            </h4>

            <div class="grid-nc-resupplies d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerResupplies" link="/c/"
                    :source="expSources.charResupply" source-title="Players resupplies" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitResupplies" title="Outfits" link="/o/"
                    :source="expSources.outfitResupply" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-spawns">
                Spawns
            </h4>

            <div class="grid-nc-spawns d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerSpawns" link="/c/"
                    :source="expSources.charSpawn" source-title="Types of spawns" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitSpawns" title="Outfits" link="/o/"
                    :source="expSources.outfitSpawn" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-vehicle-kills">
                Vehicle kills
            </h4>

            <div class="grid-nc-vehicle-kills d-flex">
                <block-view class="mr-3" :block="worldData.nc.playerVehicleKills" link="/c/"
                    :source="expSources.charVKills" source-title="Vehicles destroyed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.nc.outfitVehicleKills" title="Outfit" link="/o/"
                    :source="expSources.outfitVKills" :source-team-id="2" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-nc-title-weapon-kills">
                Weapon kills
            </h4>

            <div class="grid-nc-weapon-kills">
                <weapon-kills :weapon-kills="worldData.nc.weaponKills"
                    :total="worldData.nc.totalKills">
                </weapon-kills>
            </div>

            <h4 class="grid-nc-title-outfits">
                Outfits currently online
                <info-hover text="Only members who are online, not in the last 2 hours, like the kill list does"></info-hover>
            </h4>

            <div class="grid-nc-outfits">
                <outfits-online :data="worldData.nc.outfits"></outfits-online>
            </div>

            <!-- TR -->

            <h4 class="grid-title-tr p-1" id="header-tr">
                <img src="/img/logo_tr.png" style="width: 32px; "/>
                Terran Republic
                <info-hover text="NS kills only count when that player is on this faction"></info-hover>
            </h4>

            <div class="grid-tr-player-kills">
                <player-kill-block :block="worldData.tr" :use-short="useShort"></player-kill-block>
            </div>

            <div class="grid-tr-outfit-kills">
                <outfit-kill-block :block="worldData.tr.outfitKills"></outfit-kill-block>
            </div>

            <h4 class="grid-tr-title-focus">
                TR Focus
            </h4>

            <div class="grid-tr-focus">
                <faction-focus :focus="worldData.tr.factionFocus"></faction-focus>
            </div>

            <h4 class="grid-tr-title-heals">
                Heals
            </h4>

            <div class="grid-tr-heals d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerHeals" link="/c/"
                    :source="expSources.charHeal" source-title="Players healed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitHeals" title="Outfits" link="/o/"
                    :source="expSources.outfitHeal" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-revives">
                Revives
            </h4>

            <div class="grid-tr-revives d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerRevives" link="/c/"
                    :source="expSources.charRevive" source-title="Players revived" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitRevives" title="Outfits" link="/o/"
                    :source="expSources.outfitRevive" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-shields">
                Shield repairs
            </h4>

            <div class="grid-tr-shields d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerShieldRepair" link="/c/"
                    :source="expSources.charShield" source-title="Players shielded" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitShieldRepair" link="/o/"
                    :source="expSources.outfitShield" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-resupplies">
                Resupplies
            </h4>

            <div class="grid-tr-resupplies d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerResupplies" link="/c/"
                    :source="expSources.charResupply" source-title="Players resupplied" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitResupplies" title="Outfits" link="/o/"
                    :source="expSources.outfitResupply" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-spawns">
                Spawns
            </h4>

            <div class="grid-tr-spawns d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerSpawns" link="/c/"
                    :source="expSources.charSpawn" source-title="Types of spawns" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitSpawns" title="Outfits" link="/o/"
                    :source="expSources.outfitSpawn" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-vehicle-kills">
                Vehicle kills
            </h4>

            <div class="grid-tr-vehicle-kills d-flex">
                <block-view class="mr-3" :block="worldData.tr.playerVehicleKills" link="/c/"
                    :source="expSources.charVKills" source-title="Vehicles destroyed" :source-use-short="useShort">
                </block-view>
                <block-view :block="worldData.tr.outfitVehicleKills" title="Outfit" link="/o/"
                    :source="expSources.outfitVKills" :source-team-id="3" :source-world-id="worldID" source-title="Players in outfit" :source-use-short="useShort">
                </block-view>
            </div>

            <h4 class="grid-tr-title-weapon-kills">
                TR weapons
            </h4>

            <div class="grid-tr-weapon-kills">
                <weapon-kills :weapon-kills="worldData.tr.weaponKills"
                    :total="worldData.tr.totalKills">
                </weapon-kills>
            </div>

            <h4 class="grid-tr-title-outfits">
                Outfits
            </h4>

            <div class="grid-tr-outfits">
                <outfits-online :data="worldData.tr.outfits"></outfits-online>
            </div>

            <div class="grid-title-misc">
                <h4>Misc stats (all factions)</h4>
                <hr />
            </div>

            <div class="grid-misc-spawns">
                <h4>
                    Current top spawns
                    <info-hover text="Spawns are removed when they haven't spawned anyone in 5 minutes, not when they're killed"></info-hover>
                </h4>

                <table class="table table-sm">
                    <tr class="table-secondary th-border-top-0">
                        <th>Owner</th>
                        <th>Type</th>
                        <th>Spawns</th>
                        <th>Spawns/min</th>
                        <th>Deployed at</th>
                        <th>Lifespan</th>
                    </tr>

                    <tr v-for="entry in worldData.topSpawns.entries">
                        <td :style="{ color: getFactionColor(entry.factionID) }">
                            <a :href="'/c/' + entry.ownerID" :style="'color: ' + getFactionColor(entry.factionID) + '!important'">
                                {{entry.owner}}
                            </a>
                        </td>
                        <td>
                            <span v-if="entry.npcType == 1" title="Sunderer">
                                S
                            </span>

                            <span v-else-if="entry.npcType == 2" title="Router">
                                R
                            </span>

                            <span v-else title="Unknown">
                                U {{entry.npcType}}
                            </span>
                        </td>
                        <td>{{entry.spawnCount}}</td>
                        <td>{{(entry.spawnCount / (entry.secondsAlive / 60)).toFixed(2)}}</td>
                        <td>{{entry.firstSeenAt | moment}}</td>
                        <td>{{entry.secondsAlive | mduration}}</td>
                    </tr>
                </table>
            </div>

            <div class="grid-fights">
                <fight-data :fights="worldData.fights" :world-id="worldID"></fight-data>
            </div>

        </div>

        <popper-modal :value="modalData"></popper-modal>

        <div id="mobile-seeker" class="mobile-seeker">
            <div class="d-flex text-center">
                <div class="flex-grow-1 py-2 ms-vs" @click="scrollToFaction(1)">
                    VS
                </div>
                <div class="flex-grow-1 py-2 ms-nc" @click="scrollToFaction(2)">
                    NC
                </div>
                <div class="flex-grow-1 py-2 ms-tr" @click="scrollToFaction(3)">
                    TR
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import * as sR from "signalR";
    import Vue from "vue";
    import { createPopper, Instance } from "node_modules/@popperjs/core/lib/popper";

    import { WorldData } from "./WorldData";
    import { PopperModalData } from "popper/PopperModalData";
    import { ExpStatApi } from "api/ExpStatApi";
    import { WorldTagApi } from "api/WorldTagApi";
    import { RealtimeMapStateApi } from "api/RealtimeMapStateApi";
    import { MapApi } from "api/MapApi";
    import FactionColors from "FactionColors";
    import EventBus from "EventBus";

    import BlockView from "./components/BlockView.vue";
    import WeaponKillsView from "./components/WeaponKillsView.vue";
    import KillData from "./components/KillData.vue";
    import OutfitKillData from "./components/OutfitKillData.vue";
    import OutfitsOnline from "./components/OutfitsOnline.vue";
    import FactionFocus from "./components/FactionFocus.vue";
    import WorldTag from "./components/WorldTag.vue";
    import FightData from "./components/FightData.vue";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ContinentMetadata from "components/ContinentMetadata.vue";
    import InfoHover from "components/InfoHover.vue";
    import PopperModal from "components/PopperModal.vue";

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/TilFilter";

    type StreamFailure = {
        streamType: "death" | "exp";
        secondsMissed: number;
    }

    export const RealtimeData = Vue.extend({
        props: {

        },

        data: function() {
            return {
                worldData: new WorldData() as WorldData,
                connection: null as sR.HubConnection | null,
                socketState: "" as string,
                lastUpdate: null as Date | null,
                trackingPeriodStart: null as Date | null,

                worldID: 0 as number,
                useShort: false as boolean,

                modalData: new PopperModalData() as PopperModalData,

                popperInstance: null as Instance | null,

                showErrorDetails: false as boolean,

                expSources: {
                    charHeal: ExpStatApi.getCharacterHealEntries,
                    charRevive: ExpStatApi.getCharacterReviveEntries,
                    charResupply: ExpStatApi.getCharacterResupplyEntries,
                    charSpawn: ExpStatApi.getCharacterSpawnEntries,
                    charVKills: ExpStatApi.getCharacterVehicleKillEntries,
                    charShield: ExpStatApi.getCharacterShieldEntries,

                    outfitHeal: ExpStatApi.getOutfitHealEntries,
                    outfitRevive: ExpStatApi.getOutfitReviveEntries,
                    outfitResupply: ExpStatApi.getOutfitResupplyEntries,
                    outfitSpawn: ExpStatApi.getOutfitSpawnEntries,
                    outfitVKills: ExpStatApi.getOutfitVehicleKillEntries,
                    outfitShield: ExpStatApi.getOutfitShieldEntries
                }
            }
        },

        created: function(): void {
            document.title = `Honu / Realtime`;

            this.socketState = "unconnected";

            this.connection = new sR.HubConnectionBuilder()
                .withUrl("/ws/data")
                .withAutomaticReconnect([5000, 10000, 20000, 20000])
                .build();

            this.connection.on("UpdateData", (data: WorldData) => {
                data.tagEntries = data.tagEntries.map((iter: any) => WorldTagApi.readEntry(iter));

                // convert the string to date objects
                data.realtimeHealth.forEach((iter) => {
                    iter.lastEvent = new Date(iter.lastEvent);
                    iter.firstEvent = (iter.firstEvent == null) ? null : new Date(iter.firstEvent);
                });
                data.reconnects.forEach((iter) => {
                    iter.timestamp = new Date(iter.timestamp);
                });
                data.fights.forEach((iter) => {
                    iter.mapState = RealtimeMapStateApi.parse(iter.mapState);
                    if (iter.facility != null) {
                        iter.facility = MapApi.parseFacility(iter.facility);
                    }
                });

                this.worldData = data;
                this.lastUpdate = new Date();
            });

            this.connection.start().then(() => {
                this.socketState = "opened";
                console.log(`connected`);
                this.subscribeBasedOnWorldPath();
            }).catch(err => {
                console.error(err);
            });

            this.connection.onreconnected(() => {
                console.log(`reconnected`);
                this.socketState = "opened";
                this.subscribeBasedOnWorldPath();
            });

            this.connection.onclose((err?: Error) => {
                this.socketState = "closed";
                if (err) {
                    console.error("onclose: ", err);
                }
            });

            this.connection.onreconnecting((err?: Error) => {
                this.socketState = "reconnecting";
                if (err) {
                    console.error("onreconnecting: ", err);
                }
            });

            EventBus.$on("set-modal-data", (modalData: PopperModalData) => {
                this.setModalData(modalData);
            });
        },

        methods: {

            scrollToFaction: function(facID: number): void {
                let elemID: HTMLElement | null = null;

                if (facID == 1) {
                    elemID = document.getElementById("header-vs");
                } else if (facID == 2) {
                    elemID = document.getElementById("header-nc");
                } else if (facID == 3) {
                    elemID = document.getElementById("header-tr");
                }

                if (elemID != null) {
                    elemID.scrollIntoView();
                }
            },

            subscribeToWorld: function(worldID: number, useShort: boolean): void {
                if (this.connection == null) {
                    console.warn(`Cannot subscribe to world ${worldID}, connection is null`);
                    return;
                }

                this.worldID = worldID;

                this.connection.invoke("SubscribeToWorld", worldID, useShort).then(() => {
                    console.log(`Successfully subscribed to ${worldID} ${useShort}`);
                }).catch((err: any) => {
                    console.error(`Error subscribing to world ${worldID}: ${err}`);
                });
            },

            subscribeBasedOnWorldPath: function(): void {
                const path: string = location.pathname;
                const parts: string[] = path.split("/");

                console.log(`path: ${path}, parts: ${parts.join(", ")}`);

                const params: URLSearchParams = new URLSearchParams(location.search);

                if (parts.length >= 3) {
                    const world: string = parts[2].toLowerCase();
                    this.useShort = params.has("short");

                    if (world == "connery" || world == "1") {
                        document.title = `Honu / Server / Connery`;
                        this.subscribeToWorld(1, this.useShort);
                    } else if (world == "miller" || world == "10") {
                        document.title = `Honu / Server / Miller`;
                        this.subscribeToWorld(10, this.useShort);
                    } else if (world == "cobalt" || world == "13") {
                        document.title = `Honu / Server / Cobalt`;
                        this.subscribeToWorld(13, this.useShort);
                    } else if (world == "emerald" || world == "17") {
                        document.title = `Honu / Server / Emerald`;
                        this.subscribeToWorld(17, this.useShort);
                    } else if (world == "jaeger" || world == "jeager" || world == "19") { // common misspelling
                        document.title = `Honu / Server / Jaeger`;
                        this.subscribeToWorld(19, this.useShort);
                    } else if (world == "soltech" || world == "40") {
                        document.title = `Honu / Server / SolTech`;
                        this.subscribeToWorld(40, this.useShort);
                    } else {
                        console.error(`Unknown world ${world}`);
                    }
                }
            },

            toggleDuration: function(): void {
                const params: URLSearchParams = new URLSearchParams();

                if (this.useShort == false) {
                    params.set("short", "");
                }

                location.search = params.toString();
            },

            getFactionColor: function (factionID: number): string {
                return FactionColors.getFactionColor(factionID);
            },

            setModalData: function (modalData: PopperModalData): void {
                this.modalData = modalData;

                if (this.modalData.root == null) {
                    console.error(`Missing root element`);
                    return;
                }

                const tooltip: HTMLElement | null = document.getElementById("stat-table");
                if (tooltip == null) {
                    console.error(`Missing tooltip element '#stat-table'`);
                    return;
                }

                tooltip.style.display = "block";

                if (this.popperInstance != null) {
                    this.popperInstance.destroy();
                    this.popperInstance = null;
                }

                const popper: Instance = createPopper(this.modalData.root, tooltip, {
                    placement: "auto",
                });
                this.popperInstance = popper;
            },

            closeStatTooltip: function(): void {
                if (this.popperInstance == null) {
                    return console.warn(`popperInstance is null, no stat tooltip to close`);
                }

                this.popperInstance.destroy();

                const tooltip: HTMLElement | null = document.getElementById("stat-table");
                if (tooltip != null) {
                    tooltip.style.display = "none";
                } else {
                    console.log(`#stat-table is null, cannot close`);
                }
            }

        },

        computed: {
            indarCount: function (): number {
                return this.worldData.continentCount.indar.vs
                    + this.worldData.continentCount.indar.nc
                    + this.worldData.continentCount.indar.tr
                    + this.worldData.continentCount.indar.ns;
            },

            hossinCount: function (): number {
                return this.worldData.continentCount.hossin.vs
                    + this.worldData.continentCount.hossin.nc
                    + this.worldData.continentCount.hossin.tr
                    + this.worldData.continentCount.hossin.ns;
            },

            amerishCount: function (): number {
                return this.worldData.continentCount.amerish.vs
                    + this.worldData.continentCount.amerish.nc
                    + this.worldData.continentCount.amerish.tr
                    + this.worldData.continentCount.amerish.ns;
            },

            esamirCount: function (): number {
                return this.worldData.continentCount.esamir.vs
                    + this.worldData.continentCount.esamir.nc
                    + this.worldData.continentCount.esamir.tr
                    + this.worldData.continentCount.esamir.ns;
            },

            oshurCount: function(): number {
                return this.worldData.continentCount.oshur.vs
                    + this.worldData.continentCount.oshur.nc
                    + this.worldData.continentCount.oshur.tr
                    + this.worldData.continentCount.oshur.ns;
            },

            otherCount: function (): number {
                return this.worldData.continentCount.other.vs
                    + this.worldData.continentCount.other.nc
                    + this.worldData.continentCount.other.tr
                    + this.worldData.continentCount.other.ns;
            },

            totalCount: function (): number {
                return this.worldData.onlineCount;
            },

            totalVSCount: function (): number {
                return this.worldData.continentCount.indar.vs
                    + this.worldData.continentCount.hossin.vs
                    + this.worldData.continentCount.amerish.vs
                    + this.worldData.continentCount.esamir.vs
                    + this.worldData.continentCount.oshur.vs
                    + this.worldData.continentCount.other.vs;
            },

            totalNCCount: function (): number {
                return this.worldData.continentCount.indar.nc
                    + this.worldData.continentCount.hossin.nc
                    + this.worldData.continentCount.amerish.nc
                    + this.worldData.continentCount.esamir.nc
                    + this.worldData.continentCount.oshur.nc
                    + this.worldData.continentCount.other.nc;
            },

            totalTRCount: function (): number {
                return this.worldData.continentCount.indar.tr
                    + this.worldData.continentCount.hossin.tr
                    + this.worldData.continentCount.amerish.tr
                    + this.worldData.continentCount.esamir.tr
                    + this.worldData.continentCount.oshur.tr
                    + this.worldData.continentCount.other.tr;
            },

            totalNSCount: function (): number {
                return this.worldData.continentCount.indar.ns
                    + this.worldData.continentCount.hossin.ns
                    + this.worldData.continentCount.amerish.ns
                    + this.worldData.continentCount.esamir.ns
                    + this.worldData.continentCount.oshur.ns
                    + this.worldData.continentCount.other.ns;
            },

            badStreams: function(): StreamFailure[] {
                const cutoff: Date = new Date(new Date().getTime() - (1000 * 60 * 60 * 2));

                // Because the timestamp represents the end of an outage, and the duration can extend into before the current interval
                //		it's possible that a 2 hour outage 1 hour ago will instead show at 2 hours. To prevent this, the duration of the
                //		reconnect is adjusted to only include the period the realtime is for
                for (const reconnect of this.worldData.reconnects) {
                    const outageStart: Date = new Date(reconnect.timestamp.getTime() - (reconnect.duration * 1000));
                    const startDiff: number = outageStart.getTime() - cutoff.getTime();
                    const diff: number = -1 * Math.floor(startDiff / 1000);
                    if (startDiff < 0) {
                        //console.log(`outage at ${reconnect.timestamp.toISOString()} from a duration of ${reconnect.duration} - ${diff} = ${reconnect.duration - diff}`);
                        reconnect.duration -= diff;
                    }
                }

                const arr: StreamFailure[] = [];

                const deathCount: number = this.worldData.reconnects.filter(iter => iter.streamType == "death").reduce((acc, i) => acc += i.duration, 0);
                if (deathCount > 0) {
                    arr.push({ streamType: "death", secondsMissed: deathCount });
                }

                const expCount: number = this.worldData.reconnects.filter(iter => iter.streamType == "exp").reduce((acc, i) => acc += i.duration, 0);
                if (expCount > 0) {
                    arr.push({ streamType: "exp", secondsMissed: expCount });
                }

                return arr;
            },

            streamFailureCount: function(): number {
                return this.worldData.realtimeHealth.reduce((acc, i) => acc += i.failureCount, 0);
            },

            streamMostRecentEvent: function(): Date {
                let minDate: Date = new Date();

                for (const entry of this.worldData.realtimeHealth) {
                    if (entry.lastEvent != null && entry.lastEvent <= minDate) {
                        minDate = entry.lastEvent;
                    }
                }

                return minDate;
            },

            hasFailedStream: function(): boolean {
                return this.worldData.realtimeHealth.find(iter => iter.failureCount > 0) != undefined;
            }
        },

        components: {
            ContinentMetadata,
            BlockView,
            FactionFocus,
            "PlayerKillBlock": KillData,
            "OutfitKillBlock": OutfitKillData,
            OutfitsOnline,
            "WeaponKills": WeaponKillsView,
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            WorldTag, FightData,
            PopperModal
        }
    });

    export default RealtimeData;
</script>