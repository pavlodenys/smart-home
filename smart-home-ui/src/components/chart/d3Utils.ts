
import type { PointDto } from "../../types";
import * as d3 from "d3";
import moment from "moment";
import { httpFetch } from "../../api/httpServise";
import { getFirstPoint, getSensorDetailsUrl } from "./chartDates";
export { getFirstPoint, getSensorDetailsUrl } from "./chartDates";
export { crateZoom } from "./chartZoom";

let timeoutId;

export const formatDate = (d) => moment(d).format("YYYY-MM-DD HH:mm:ss");
export const smallFormatDate = (d) => moment(d).format("HH:mm");

// one <circle> DOM node per point (each with hover listeners) gets expensive well before a
// line path does, so circles are rendered from an evenly-sampled subset while the line still
// uses the full series; the most recent point is always kept so new readings show immediately
export const decimatePoints = (points: any[], maxPoints: number) => {
    if (!points || points.length <= maxPoints) {
        return points ?? [];
    }

    const step = Math.ceil(points.length / maxPoints);
    const sampled = points.filter((_, index) => index % step === 0);
    const last = points[points.length - 1];
    if (sampled[sampled.length - 1] !== last) {
        sampled.push(last);
    }

    return sampled;
};

const MAX_RENDERED_CIRCLES = 300;
const MAX_MINIMAP_LINE_POINTS = 400;

export const createScales = (width, height, domainX, domainY) => {
    const x = d3.scaleTime().domain(domainX).range([0, width]);
    const y = d3.scaleLinear().domain(domainY).range([height, 0]);
    return { x, y };
};

export const createAx = (x, axisType, tickCount, tickFormat = null) => {
    let xAxis = axisType(x).ticks(tickCount);
    if (tickFormat) {
        xAxis = xAxis.tickFormat(tickFormat);
    }

    return xAxis;
};

export const createSVG = (selector, width, height, margin) => {
    const svg = d3
        .select(selector)
        .append("svg")
        .attr("viewBox", `0 0 ${width} ${height}`)
        .attr("width", "100%")
        .style("height", "auto")
        .attr("preserveAspectRatio", "xMidYMid meet")
        .append("g")
        .attr("transform", `translate(${margin.left}, ${margin.top})`);
    return svg;
};

export const createValueLine = (x, y) => {
    return d3
        .line<PointDto>()
        .x((d) => x(new Date(formatDate(d.dateTime))))
        .y((d) => y(d.value));
};

export const createPath = (svgElement, points, valueline, margin) => {
    return svgElement
        .append("path")
        .datum(points)
        .attr("class", "line")
        .attr("d", valueline);
};

export const createTracker = (svgElement, width, height, translateX) => {
    var tracker = svgElement
        .append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("transform", `translate(${translateX - 45}, 0)`)
        .style("fill", "transparent")
        .style("stroke", "blue")
        .style("stroke-width", 1)
        // a "transparent" fill is not painted, so without this the drag only registers on the 1px stroke
        .style("pointer-events", "all")
        .style("cursor", "grab");
    return tracker;
};

// aligns the minimap tracker box with the window currently shown by the main chart's x scale.
// anchored to the window's END (not start): the main chart's start is a recent-window heuristic
// (see getFirstPoint) that doesn't line up with the minimap's full-range domain, but the end of
// the window is always "now"/the latest loaded point, so anchoring there keeps the box flush
// against the right edge whenever the user is looking at the latest data (as expected).
export const syncTrackerToDomain = (tracker, minimapXScale, mainX, trackerWidth, minimapElementWidth) => {
    const [, windowEnd] = mainX.domain();
    const rawX = minimapXScale(windowEnd) - trackerWidth;
    const clampedX = Math.max(0, Math.min(rawX, minimapElementWidth - trackerWidth));

    tracker.attr("transform", `translate(${clampedX}, 0)`);
};

export const createCircle = (chartId, svgElement, points, margin, x, y) => {
    return svgElement
        .selectAll(`.dot-${chartId}`)
        .data(points)
        .enter()
        .append("circle")
        .attr("class", `dot-${chartId}`)
        .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
        .attr("cy", (d) => y(d.value))
        .attr("r", 5)
        .on("mouseover", (d, e) => {
            const xPosition = d.pageX;
            const yPosition = d.pageY;

            const tooltip = d3
                .select("body")
                .append("div")
                .attr("id", `tooltip-${e.id}`)
                .attr("class", "tooltip");

            tooltip.append("span").attr("id", "value");

            tooltip
                .style("left", xPosition + "px")
                .style("top", yPosition + "px")
                .select("#value")
                .text(`${e.value} ${e.name}`);

            tooltip.classed("hidden", false);
        })
        .on("mouseout", (d, e) => {
            d3.select(`#tooltip-${e.id}`).remove();
        });
};

export const itemsInRange = (d, xValue, inverted, x) => {
    if (
        new Date(formatDate(d.dateTime)) >= xValue &&
        new Date(formatDate(d.dateTime)) <= inverted
    ) {
        return x;
    } else {
        return null;
    }
};

export const createDragger = (
    tracker,
    path,
    chartArea,
    dotClassId,
    width,
    margin,
    minimapXScale,
    trackerWidth,
    x,
    y,
    xAxis,
    dataId,
    dispatch
) => {
    const [domainStart, domainEnd] = x.domain();
    const domainSpan = domainEnd.getTime() - domainStart.getTime();
    // tracks how many extra pages of older points have already been requested from the server
    let loadedPage = 1;
    let lastClampedXPos = 0;

    const drag = d3
        .drag()
        .on("start", function () {
            tracker.style("cursor", "grabbing");
        })
        .on("drag", function (event) {
            cancelAnimationFrame(timeoutId);
            timeoutId = requestAnimationFrame(() => {
                const minimapElementWidth = width + margin.right + 20;
                const clampedXPos = Math.max(
                    0,
                    Math.min(event.x, minimapElementWidth - trackerWidth)
                );
                lastClampedXPos = clampedXPos;

                tracker.attr("transform", `translate(${clampedXPos}, 0)`);

                // shift the main chart's visible window to match the tracker position
                const windowStart = minimapXScale.invert(clampedXPos);
                const windowEnd = new Date(windowStart.getTime() + domainSpan);
                x.domain([windowStart, windowEnd]);

                xAxis.call(d3.axisBottom(x));
                xAxis
                    .selectAll(".tick text")
                    .attr("transform", "translate(-10, 0) rotate(-40)")
                    .style("text-anchor", "end");

                path.attr(
                    "d",
                    d3
                        .line<PointDto>()
                        .x((point) => x(new Date(formatDate(point.dateTime))))
                        .y((point) => y(point.value))
                );

                // re-select fresh each frame: new real-time/paged points are added as separate
                // circles by updateDataChart and wouldn't be picked up by a stale selection
                chartArea
                    .selectAll(`.dot-${dotClassId}`)
                    .attr("cx", (point) => x(new Date(formatDate(point.dateTime))))
                    .attr("cy", (point) => y(point.value));
            });
        })
        .on("end", () => {
            tracker.style("cursor", "grab");

            // only fetch more data once the tracker has been dragged to the earliest loaded point
            if (lastClampedXPos <= 1) {
                loadedPage += 1;
                dispatch("chartEvent", { dataId, page: loadedPage });
            }
        });

    return drag;
};

export const filterPoints = async (points, date, chartId, dataId, sensorId) => {
    let filtered = (points ?? []).filter((point) => {
        const pointDate = new Date(point.dateTime);
        return moment(pointDate).format("YYYY-MM-DD") === date;
    });

    if (!filtered.length && sensorId != null) {
        const sensor = await httpFetch.get(getSensorDetailsUrl(sensorId, date));
        // match by data source id rather than array position, since the response order isn't guaranteed
        const matchingChart = sensor?.chartData?.find((c) => c.id === dataId) ?? sensor?.chartData?.[chartId];
        filtered = matchingChart?.data ?? [];
    }

    // getFirstPoint/domain calculations assume chronological order, but real-time pushes and
    // paged/panned data are simply appended, so this can't be relied on without sorting here
    return [...filtered].sort(
        (a, b) => new Date(a.dateTime).getTime() - new Date(b.dateTime).getTime()
    );
};

export const updateDataChart = (
    chartId,
    data,
    x,
    y,
    xAxis,
    yAxis,
    svg,
    margin,
    svgMap,
    minimapLine,
    minix,
    miniy
) => {
    if (!data || !data.length) {
        return;
    }

    svg.select(".no-data-label").remove();

    //console.log(data.length);
    const firtPoint = getFirstPoint(data);
    const xDomain = [
        firtPoint,
        d3.max(data, (d: any) => new Date(formatDate(d.dateTime))),
    ];
    const yDomain = [
        d3.min(data, (d: PointDto) => d.value) * 0.95,
        d3.max(data, (d: PointDto) => d.value) * 1.05,
    ];
    x.domain(xDomain);
    y.domain(yDomain);

    const xminiDomain = [
        d3.min(data, (d: any) => new Date(formatDate(d.dateTime))),
        d3.max(data, (d: any) => new Date(formatDate(d.dateTime))),
    ];
    minix.domain(xminiDomain);
    miniy.domain(yDomain);

    // Update the x and y axes with the new domains

    xAxis.transition().duration(1000).call(d3.axisBottom(x));

    xAxis.selectAll(".tick text")
        .attr("transform", "translate(-10, 0) rotate(-40)") // Rotate the tick labels by -40 degrees
        .style("text-anchor", "end"); 
    yAxis.call(d3.axisLeft(y));

    // Select the line and bind the new data to it

    const line = svg.select(".line");
    const miniline = svgMap.select(".line");

    // the minimap is only a few hundred pixels wide, so a full-resolution series is wasted work
    const miniData = decimatePoints(data, MAX_MINIMAP_LINE_POINTS);

    //console.log(miniline);
    if (line.size()) {
        line.datum(data);
        miniline.data([miniData]);
    } else {
        const valueLine = createValueLine(x, y);
        const path = createPath(svg, data, valueLine, margin);
        const minMapPath = createPath(svgMap, miniData, minimapLine, margin);
    }

    // Redraw the line with the new data and scales
    // no transition here: tweening the "d" attribute as a raw string between
    // datasets of different sizes/dates produces a garbled zigzag artifact
    line.attr(
        "d",
        d3
            .line()
            .x((d: any) => x(new Date(formatDate(d.dateTime))))
            .y((d: any) => y(d.value))
    );
    //console.log(minix(new Date(formatDate(data[0].dateTime))));
    miniline.attr(
        "d",
        d3
            .line()
            .x((d: any) => minix(new Date(formatDate(d.dateTime))))
            .y((d: any) => miniy(d.value))
    );

    // Select the circles and bind the new data to them. Rendering every point as its own DOM
    // node (with hover listeners) gets expensive well past a few hundred points, so circles are
    // drawn from a decimated subset while the line keeps full resolution. Keying by point id
    // (instead of index) lets d3 recognize unchanged points instead of moving every circle on
    // every update, and updating positions immediately (no transition) keeps circles in sync
    // with the line, which is also redrawn immediately above.
    const circleData = decimatePoints(data, MAX_RENDERED_CIRCLES);
    let circles = svg.selectAll(`.dot-${chartId}`).data(circleData, (d: any) => d.id);
    if (circles.size()) {
        circles
            .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
            .attr("cy", (d) => y(d.value));

        circles
            .enter()
            .append("circle")
            .attr("class", `dot-${chartId}`)
            .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
            .attr("cy", (d) => y(d.value))
            .attr("r", 5) // specify the radius or any other attributes for the new circles
            .attr("fill", "blue"); // specify the fill color or any other style for the new circles

        // remove leftover dots from a smaller/older dataset
        circles.exit().remove();
    } else {
        circles = createCircle(chartId, svg, circleData, margin, x, y);
    }
};

