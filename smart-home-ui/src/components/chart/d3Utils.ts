
import type { PointDto } from "../../types";
import * as d3 from "d3";
import moment from "moment";
import { httpFetch } from "../../api/httpServise";

let timeoutId;

export const formatDate = (d) => moment(d).format("YYYY-MM-DD HH:mm:ss");
export const smallFormatDate = (d) => moment(d).format("HH:mm");

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

    xAxis
        .attr("transform", "rotate(-90)")
        .style("text-anchor", "end")
        .attr("dx", "-0.8em")
        .attr("dy", "0.15em");

    return xAxis;
};

export const createSVG = (selector, width, height, margin) => {
    const svg = d3
        .select(selector)
        .append("svg")
        .attr("width", width)
        .attr("height", height)
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

export const createPath = (svg, points, valueline, margin) => {
    return svg
        .append("path")
        .data([points])
        .attr("transform", `translate(${margin.left}, 0)`)
        .attr("class", "line")
        .attr("d", valueline);
};

export const createTracker = (svg, width, height, translateX) => {
    var tracker = svg
        .append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("transform", `translate(${translateX - 45}, 0)`)
        .style("fill", "transparent")
        .style("stroke", "blue")
        .style("stroke-width", 1);
    return tracker;
};

export const createCircle = (chartId, svg, points, margin, x, y) => {
    return svg
        .selectAll(`.dot-${chartId}`)
        .data(points)
        .enter()
        .append("circle")
        .attr("transform", `translate(${margin.left}, 0)`)
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
    circle,
    width,
    margin,
    minimapXScale,
    trackerWidth,
    x,
    y,
    xAxis,
    chartId,
    dispatch
) => {
    const drag = d3
        .drag()
        .on("start", function () {
            tracker.style("cursor", "grabbing");
        })
        .on("drag", function (d) {
            cancelAnimationFrame(timeoutId);
            timeoutId = requestAnimationFrame(() => {
                let updatedXDomain;
                let xPos = d.x;
                let minimapElementWidth = width + margin.right + 20;
                let xValue = minimapXScale.invert(xPos);
                //let clampedXPos = d3.clamp(0, minimapWidth - trackerWidth, xPos);
                let clampedXPos = Math.max(
                    0,
                    Math.min(xPos, minimapElementWidth - trackerWidth)
                );

                // console.log(`${xPos} ${minimapElementWidth} ${trackerWidth}`);
                //console.log(clampedXPos);

                //tracker.attr("x", clampedXPos);
                tracker.attr("transform", `translate(${clampedXPos}, 0)`);
                if (d.dx === 1) {
                    updatedXDomain = x.domain(
                        x.domain().map((d) => new Date(d.getTime() + 5000))
                    );
                    // y.domain();
                } else if (d.dx === -1) {
                    updatedXDomain = x.domain(
                        x.domain().map((d) => new Date(d.getTime() - 5000))
                    );
                    //y.domain(d3.extent(y.ticks));
                }
                //const xAxisGroup = xAxis.select(".x-axis");
                const updatedXAxis = d3.axisBottom(updatedXDomain);
                // .tickFormat(d3.timeFormat("%d %b"));
                xAxis.call(updatedXAxis);

                // let xData = d3.line<PointDto>().x((point) => {
                //   return itemsInRange(
                //     point,
                //     xValue,
                //     x.invert(width),
                //     x(new Date(formatDate(point.dateTime)))
                //   );
                // });

                // console.log(xData);

                path.attr(
                    "d",
                    d3
                        .line<PointDto>()
                        .x((point) => {
                            return itemsInRange(
                                point,
                                xValue,
                                x.invert(width),
                                x(new Date(formatDate(point.dateTime)))
                            );
                        })
                        .y((point) => {
                            return y(point.value);
                        })
                );

                circle
                    .attr("cx", (point) => {
                        return itemsInRange(
                            point,
                            xValue,
                            x.invert(width),
                            x(new Date(formatDate(point.dateTime)))
                        );
                    })
                    .attr("cy", (point) => {
                        return itemsInRange(
                            point,
                            xValue,
                            x.invert(width),
                            y(point.value)
                        );
                    });
            });
        })
        .on("end", (e) => {
            // console.log(e);
            dispatch("chartEvent", { dataId: chartId, page: 1 });
            tracker.style("cursor", "grab");
        });

    return drag;
};

export const filterPoints = async (points, date, chartId, chartIndex) => {
    let filtered = points.filter((point) => {
        const pointDate = new Date(point.dateTime);
        return moment(pointDate).format("YYYY-MM-DD") === date;
    });

    if (!filtered.length) {
        let sensor = await httpFetch.get(`api/home/sensors/${chartIndex}/${date}`);
        filtered = sensor.chartData[chartId].data;
    }

    return filtered;
};

export const getFirstPoint = (points: any) => {
    if (points && points.length) {
        if (points.length > 30) {
            return new Date(
                new Date(points[points.length - 1].dateTime).getTime() -
                20 * 60 * 1000
            );
        }
        return points[0].dateTime;
    } else {
        return new Date();
    }
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
    yAxis.call(d3.axisLeft(y));

    // Select the line and bind the new data to it

    const line = svg.select(".line");
    const miniline = svgMap.select(".line");

    //console.log(miniline);
    if (line.size()) {
        line.datum(data);
        miniline.data([data]);
    } else {
        const valueLine = createValueLine(x, y);
        const path = createPath(svg, data, valueLine, margin);
        const minMapPath = createPath(svgMap, data, minimapLine, margin);
    }

    // Redraw the line with the new data and scales
    line
        .transition()
        .duration(1000)
        .attr(
            "d",
            d3
                .line()
                .x((d: any) => x(new Date(formatDate(d.dateTime))))
                .y((d: any) => y(d.value))
        );
    //console.log(minix(new Date(formatDate(data[0].dateTime))));
    miniline
        .transition()
        .duration(1000)
        .attr(
            "d",
            d3
                .line()
                .x((d: any) => minix(new Date(formatDate(d.dateTime))))
                .y((d: any) => miniy(d.value))
        );

    // Select the circles and bind the new data to them
    let circles = svg.selectAll(`.dot-${chartId}`).data(data);
    if (circles.size()) {
        //circles.data(data);
        circles
            .transition()
            .duration(1000)
            .attr("transform", `translate(${margin.left}, 0)`)
            .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
            .attr("cy", (d) => y(d.value));

        circles
            .enter()
            .append("circle")
            .attr("class", `dot-${chartId}`)
            .attr("transform", `translate(${margin.left}, 0)`)
            .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
            .attr("cy", (d) => y(d.value))
            .attr("r", 5) // specify the radius or any other attributes for the new circles
            .attr("fill", "blue"); // specify the fill color or any other style for the new circles
    } else {
        circles = createCircle(chartId, svg, data, margin, x, y);
    }
};

export const crateZoom = (width, height, xScale, yScale) => {
    return d3
        .zoom()
        .scaleExtent([1, 100])
        .translateExtent([
            [0, 0],
            [width, height],
        ])
        .on("zoom", (event) => {
            const transform1 = event.transform;

            //console.log(transform1);
            xScale.domain(transform1.rescaleX(xScale).domain());
            yScale.domain(transform1.rescaleY(yScale).domain());
        });
};