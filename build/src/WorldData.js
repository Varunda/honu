export class BlockEntry {
    constructor() {
        this.id = "";
        this.name = "";
        this.value = 0;
    }
}
export class Block {
    constructor() {
        this.name = "";
        this.entires = [];
        this.total = 0;
    }
}
export class KillData {
    constructor() {
        this.id = "";
        this.name = "";
        this.kills = 0;
        this.deaths = 0;
    }
}
export class OutfitKillData {
    constructor() {
        this.id = "";
        this.factionId = "";
        this.tag = null;
        this.name = "";
        this.kills = 0;
        this.deaths = 0;
        this.members = 0;
    }
}
export class KillBlock {
    constructor() {
        this.entries = [];
    }
}
export class OutfitKillBlock {
    constructor() {
        this.entires = [];
    }
}
export class FactionData {
    constructor() {
        this.factionID = "";
        this.factionName = "";
        this.playerKills = new KillBlock();
        this.outfitKills = new OutfitKillBlock();
        this.outfitHeals = new Block();
        this.outfitResupplies = new Block();
        this.outfitRevives = new Block();
        this.playerHeals = new Block();
        this.playerResupplies = new Block();
        this.playerRevives = new Block();
    }
}
export class WorldData {
    constructor() {
        this.worldID = "";
        this.worldName = "";
        this.nc = new FactionData();
        this.tr = new FactionData();
        this.vs = new FactionData();
    }
}
//# sourceMappingURL=WorldData.js.map