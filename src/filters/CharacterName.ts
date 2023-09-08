import Vue from "vue";
import { PsCharacter } from "api/CharacterApi";

Vue.filter("characterName", (data: PsCharacter | null): string => {
    if (data == null) {
        return "no character passed!";
    }

    let s: string = data.name;

    if (data.outfitID != null) {
        s = `[${data.outfitTag}] ` + s;
    }

    return s;
});

