
export default class CharacterUtils {

    public static getDisplay(character: { name: string, outfitTag: string | null }): string {
        return `${(character.outfitTag != null ? `[${character.outfitTag}] ` : ``)} ${character.name}`;
    }

    public static display(id: string, character: { name: string, outfitTag: string | null } | null | undefined): string {
        if (character == null || character == undefined) {
            return `<missing ${id}>`;
        }
        return `${(character.outfitTag != null ? `[${character.outfitTag}] ` : ``)} ${character.name}`;
    }

}