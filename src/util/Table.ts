import TimeUtils from "util/Time";

export default class TableUtils {

    public static chart(elementName: string, context: any, valueFormatter: ((_: number) => string) | null) {
        // tooltip Element
        let tooltipEl: HTMLElement | null = document.getElementById(elementName);

        // create element on first render
        if (!tooltipEl) {
            tooltipEl = document.createElement('div');
            tooltipEl.id = elementName;
            tooltipEl.innerHTML = '<table class="table table-sm table-secondary"></table>';
            document.body.appendChild(tooltipEl);
        }

        // hide if no tooltip
        const tooltipModel = context.tooltip;
        if (tooltipModel.opacity === 0) {
            tooltipEl.style.opacity = "0";
            return;
        }

        // set caret Position
        tooltipEl.classList.remove('above', 'below', 'no-transform');
        if (tooltipModel.yAlign) {
            tooltipEl.classList.add(tooltipModel.yAlign);
        } else {
            tooltipEl.classList.add('no-transform');
        }

        // set Text
        if (tooltipModel.body) {
            const titleLines = tooltipModel.title || [];

            // find the closest point and save the index,
            //      which is used to bold the line of the tooltip that has the line closest to
            let closestDatasetIndexs: number[] = [];
            let closestDist: number = Number.MAX_VALUE;
            for (let i = 0; i < tooltipModel.dataPoints.length; ++i) {
                const point = tooltipModel.dataPoints[i];
                const pointy = (point.element.cp1y + point.element.cp2y) / 2;
                let diff = Math.abs(tooltipModel.caretY - pointy);
                //console.log(`diff ${diff}, dist ${closestDist}, iter ${point.datasetIndex}, index ${closestDatasetIndex}, y ${tooltipModel.caretY}, cp1y ${pointy}`);
                if (diff < closestDist) {
                    closestDatasetIndexs = [];
                    closestDatasetIndexs.push(point.datasetIndex);
                    closestDist = diff;
                }
                if (diff == closestDist) {
                    closestDatasetIndexs.push(point.datasetIndex);
                }
            }

            let innerHtml = '<thead>';

            titleLines.forEach((title: string) => {
                innerHtml += '<tr class="th-border-top-0"><th style="border-bottom: 0">' + title + '</th></tr>';
            });
            innerHtml += '</thead><tbody>';

            // sort the values from descending to ascending
            [...tooltipModel.dataPoints].sort((a, b) => {
                return b.parsed.y - a.parsed.y;
            }).forEach((value, i) => {
                if (closestDatasetIndexs.indexOf(value.datasetIndex) > -1) {
                    innerHtml += `<tr style="color: var(--white); font-weight: 600">`;
                } else {
                    innerHtml += `<tr style="color: var(--light); font-weight: 400">`;
                }

                innerHtml += `<td><span style="color: ${value.dataset.borderColor}">&#9632;</span>${value.dataset.label}&nbsp;</td>`;
                innerHtml += `<td>${valueFormatter ? valueFormatter(value.parsed.y) : value.parsed.y}</td>`;
                innerHtml += `</tr>`;
            });

            innerHtml += '</tbody>';

            let tableRoot = tooltipEl.querySelector('table');
            if (tableRoot == null) {
                throw `failed to find table element`;
            }
            tableRoot.innerHTML = innerHtml;
        }

        const position = context.chart.canvas.getBoundingClientRect();
        //console.log(position, "win x", window.pageXOffset, "win y", window.pageYOffset, "tt x", tooltipModel.caretX, "tt y", tooltipModel.caretY, "padding", tooltipModel.padding);

        // Display, position, and set styles for font
        tooltipEl.style.opacity = "1";
        tooltipEl.style.position = "absolute";
        if ((tooltipModel.caretX / position.width) < 0.5) {
            tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + "px";
            tooltipEl.style.right = "";
        } else {
            tooltipEl.style.left = "";
            // this is like 15px off and idk why and i've spent like 2 hours here now and i don't care
            tooltipEl.style.right = (position.right - tooltipModel.caretX - position.x) + "px"; // - window.pageXOffset + position.left) + 'px';
        }
        tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + "px";
        tooltipEl.style.fontFamily = "Consolas";
        tooltipEl.style.padding = tooltipModel.padding + "px " + tooltipModel.padding + "px";
        tooltipEl.style.pointerEvents = "none";
    }

}