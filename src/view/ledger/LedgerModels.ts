﻿import { latLng, polyline, LatLng, Map as LMap, Polygon, Polyline, Marker, PolylineOptions, MarkerOptions } from 'leaflet';
import { PsFacility, PsMapHex } from 'api/MapApi';

export class VertexPoint {
	public x: number = 0;
	public y: number = 0;

	constructor(x: number, y: number) {
		this.x = Math.round(x * 1000) / 1000;
		this.y = Math.round(y * 1000) / 1000;
	}

	public equals(other: VertexPoint): boolean {
        return this.x === other.x && this.y == other.y;
	}

	public toLatLng(): LatLng {
		return latLng(this.y, this.x);
	}
}

export class VertexLine {
    constructor(
        public v1: VertexPoint,
        public v2: VertexPoint
    ) {
        this.v1 = v1;
        this.v2 = v2;
    }

    public equals(other: VertexLine): boolean {
		return (this.v1.equals(other.v1) && this.v2.equals(other.v2)) || (this.v1.equals(other.v2) && this.v2.equals(other.v1));
    }
}

export class ZoneRegion extends Polygon {
    public regionID: number;
    public facility: PsFacility | null = null;

    constructor(regionId: number, latLngs: LatLng[], options?: PolylineOptions) {
        super(latLngs, options);

        this.regionID = regionId;
    }

    public disablePopup() {
        this.unbindPopup();
    }

    public static setupMapRegions(hexes: PsMapHex[]): ZoneRegion[] {
        const regions: ZoneRegion[] = [];

        let width = 50 / 8;

        let b = width / 2;
        let c = b / Math.sqrt(3) * 2;
        let a = c / 2;

        const regionHexes = hexes.reduce((rv: Map<number, PsMapHex[]>, x) => {
            if (rv.has(x.regionID) == false) {
                rv.set(x.regionID, []);
            }

            const h: PsMapHex[] = rv.get(x.regionID) || [];
            h.push(x);

            rv.set(x.regionID, h);
            return rv;
        }, new Map());

        console.log(regionHexes);

        for (const regionId of regionHexes.keys()) {
            const hexs: PsMapHex[] | undefined = regionHexes.get(regionId);

            console.log(`Processing ${regionId}: `, hexs);

            if (hexs == undefined) {
                console.error(`Skipping region with no hexes: ${regionId}`);
                continue;
            }

            let regionLines: VertexLine[] = [];

            for (const hex of hexs) {
                const x = (2 * hex.x + hex.y) / 2 * width;

                let y;
                if (hex.y % 2 == 1) {
                    let t = Math.floor(hex.y / 2);
                    y = c * t + 2 * c * (t + 1) + c / 2;
                } else {
                    y = (3 * c * hex.y) / 2 + c;
                }

                const hexVerts = [
                    new VertexPoint(x - b, y - a),
                    new VertexPoint(x, y - c),
                    new VertexPoint(x + b, y - a),
                    new VertexPoint(x + b, y + a),
                    new VertexPoint(x, y + c),
                    new VertexPoint(x - b, y + a)
                ];

                const hexLines = [
                    new VertexLine(hexVerts[0], hexVerts[1]),
                    new VertexLine(hexVerts[1], hexVerts[2]),
                    new VertexLine(hexVerts[2], hexVerts[3]),
                    new VertexLine(hexVerts[3], hexVerts[4]),
                    new VertexLine(hexVerts[4], hexVerts[5]),
                    new VertexLine(hexVerts[5], hexVerts[0])
                ];

                regionLines.push(...hexLines);
            }

            let regionOuterLines: VertexLine[] = ZoneRegion.getOuterLines(regionLines);

            if (regionOuterLines.length == 0) {
                console.warn(`Failed to find outer lines for ${regionId}`);
            }

            let regionOuterVerts: VertexPoint[] = ZoneRegion.getOuterVerts(regionOuterLines);

            let latLngVerts = regionOuterVerts.map((a: VertexPoint) => a.toLatLng());

            const options: PolylineOptions = {
                weight: 2,
                color: '#FFF',
                opacity: 1,
                pane: 'regions',
            };

            let region = new ZoneRegion(regionId, latLngVerts, options);

            regions.push(region);
        }

        return regions;
    }

    public static getOuterLines(lines: VertexLine[]): VertexLine[] {
        const outerLines = [];

        for (let i = 0; i < lines.length; i++) {
            let count = 1;

            for (let k = 0; k < lines.length; k++) {
                if (i == k) {
                    continue;
                }

                if (lines[i].equals(lines[k])) {
                    //console.log(lines[i], lines[k]);
                    count++;
                    break;
                }
            }

            if (count == 1) {
                outerLines.push(lines[i]);
            }
        }

        return outerLines;
    };

    public static getOuterVerts(lines: VertexLine[]): VertexPoint[] {
        if (lines.length == 0) {
            return [];
        }

        let tmp: VertexLine[] = [ ...lines ];

        let verts = [tmp[0].v1, tmp[0].v2];
        tmp.splice(0, 1);

        let loopCount = 0;
        while (tmp.length > 0) {
            for (var i = 0; i < tmp.length; i++) {
                if (tmp[i].v1.equals(verts[verts.length - 1])) {
                    verts.push(tmp[i].v2);
                    tmp.splice(i, 1);
                    break;
                }
            }

            if (loopCount++ > 1000) {
                break;
            }
        }

        verts.pop();

        return verts;
    };

}

export class LatticeLink {
    public facilities: PsFacility[] = [];

    public outline: Polyline | null = null;
    public line: Polyline | null = null;

    public facilityA: PsFacility;
    public facilityB: PsFacility;

    constructor(facA: PsFacility, facB: PsFacility) {
        this.facilities = [facA, facB];

        this.facilityA = facA;
        this.facilityB = facB;

        const AlatLng = latLng(facA.locationY ?? 0, facA.locationX ?? 0);
        const BlatLng = latLng(facB.locationY ?? 0, facB.locationX ?? 0);

        const points = [AlatLng, BlatLng];

		this.outline = polyline(points, {
			className: 'lattice-outline',
			pane: 'latticePane',
			interactive: false
		});

		this.line = polyline(points, {
			className: 'lattice-line',
			pane: 'latticePane',
			dashArray: undefined,
			interactive: false
		});
    }

    public addTo(map: LMap) {
        if (this.line && this.outline) {
            this.outline.addTo(map);
            this.line.addTo(map);
        }
    }
}