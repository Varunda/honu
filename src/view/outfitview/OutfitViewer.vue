<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="#">Outfit</a>

                <span>/</span>

                <span v-if="outfit.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else>
                    {{outfit.data.name}}
                </span>
            </h1>
        </div>

        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <hr class="border" />

        <div v-if="outfit.state == 'loading'">
            Loading...
        </div>

        <div v-if="outfit.state == 'loaded'">
            <table class="table table-sm">
                <tr>
                    <td><b>Tag</b></td>
                    <td>{{outfit.data.tag}}</td>
                </tr>

                <tr>
                    <td><b>Name</b></td>
                    <td>'{{outfit.data.name}}'</td>
                </tr>

                <tr>
                    <td><b>Faction</b></td>
                    <td>{{outfit.data.factionID}}</td>
                </tr>

                <tr>
                    <td><b>Census</b></td>
                    <td>
                        <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/outfit?outfit_id=' + outfit.data.id" target="_blank">
                            Census
                            <span class="fas fa-external-link-alt"></span>
                        </a>
                    </td>
                </tr>
            </table>

        </div>


    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    export const OutfitViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfitID: "0" as string,

                outfit: Loadable.idle() as Loading<PsOutfit | null>
            }
        },

        beforeMount: function(): void {
            this.parseOutfitIDFromUrl();
            this.bindOutfit();
        },

        methods: {
            parseOutfitIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const outfitID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(outfitID) == false) {
                    this.outfitID = parts[2];
                    console.log(`outfit id is ${this.outfitID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${outfitID}`;
                }
            },

            bindOutfit: async function(): Promise<void> {
                this.outfit = Loadable.loading();
                this.outfit = await Loadable.promise(OutfitApi.getByID(this.outfitID));
            }
        }

    });
    export default OutfitViewer;
</script>