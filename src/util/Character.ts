
export default class CharacterUtils {

    public static getDisplay(character: { name: string, outfitTag: string | null }): string {
        return `${(character.outfitTag != null ? `[${character.outfitTag}] ` : ``)} ${character.name}`;
    }

}