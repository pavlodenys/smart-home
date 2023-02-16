<script lang="ts">
  import { onMount } from "svelte";
  import type { ChartData, PointDto } from "../types";
  import * as d3 from "d3";
  import moment from "moment";

  //TODO: add real-time update

  export let chart: ChartData;
  const formatDate = (d) => moment(d).format("YYYY-MM-DD HH:mm:ss");

  const createScales = (width, height, domainX, domainY) => {
    var x = d3.scaleTime().domain(domainX).range([0, width]);
    var y = d3.scaleLinear().domain(domainY).range([height, 0]);
    return { x, y };
  };

  const createAx = (x, axisType, tickCount, tickFormat = null) => {
    let xAxis = axisType(x).ticks(tickCount);
    if (tickFormat) {
      xAxis = xAxis.tickFormat(tickFormat);
    }
    return xAxis;
  };

  const createSVG = (selector, width, height, margin) => {
    const svg = d3
      .select(selector)
      .append("svg")
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left}, ${margin.top})`);
    return svg;
  };

  const createValueLine = (x, y) => {
    const valueline = d3
      .line<PointDto>()
      .x((d) => x(new Date(formatDate(d.dateTime))))
      .y((d) => y(d.value));
    return valueline;
  };

  const createPath = (svg, points, valueline, margin) => {
    const path = svg
      .append("path")
      .data([points])
      .attr("transform", `translate(${margin.left}, 0)`)
      .attr("class", "line")
      .attr("d", valueline);
    return path;
  };

  const createTracker = (svg, width, height, translateX) => {
    var tracker = svg
      .append("rect")
      .attr("width", width)
      .attr("height", height)
      .attr("transform", `translate(${translateX - 100}, 0)`)
      .style("fill", "transparent")
      .style("stroke", "blue")
      .style("stroke-width", 1);
    return tracker;
  };

  const createCircle = (svg, points, margin, x, y) => {
    const circle = svg
      .selectAll(".dot")
      .data(points)
      .enter()
      .append("circle")
      .attr("transform", `translate(${margin.left}, 0)`)
      .attr("class", "dot")
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

    return circle;
  };

  const itemsInRange = (d, xValue, inverted, x) => {
    if (
      new Date(formatDate(d.dateTime)) >= xValue &&
      new Date(formatDate(d.dateTime)) <= inverted
    ) {
      return x;
    } else {
      return null;
    }
  };

  const createDragger = (
    tracker,
    path,
    circle,
    width,
    margin,
    minimapXScale,
    trackerWidth,
    x,
    y
  ) => {
    const drag = d3
      .drag()
      .on("start", function () {
        tracker.style("cursor", "grabbing");
      })
      .on("drag", function (d) {
        let xPos = d.x;
        let minimapElementWidth = width + margin.right + 20;
        let xValue = minimapXScale.invert(xPos);
        //let clampedXPos = d3.clamp(0, minimapWidth - trackerWidth, xPos);
        let clampedXPos = Math.max(
          0,
          Math.min(xPos, minimapElementWidth - trackerWidth)
        );

        //tracker.attr("x", clampedXPos);
        tracker.attr("transform", `translate(${clampedXPos}, 0)`);
        if(d.dx === 1) {
 x.domain(x.domain().map((d) => new Date(d.getTime() + 100)));
        } else if(d.dx === -1) {
 x.domain(x.domain().map((d) => new Date(d.getTime() - 100)));
        }


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
            return itemsInRange(point, xValue, x.invert(width), y(point.value));
          });
      })
      .on("end", function () {
        tracker.style("cursor", "grab");
      });

    return drag;
  };

  const updateChart = (svg, xAxis, valueline, x, y) => {
    svg.select(".x.axis").call(xAxis);
    svg.select(".line").attr("d", valueline);

    const path = svg.select(".line").attr("d", valueline);
    const totalLength = path.node().getTotalLength();
    svg
      .selectAll(".dot")
      .attr("cx", (d) => x(new Date(formatDate(d.dateTime))))
      .attr("cy", (d) => y(d.value));
  };

  onMount(() => {
    const points = chart.data;

    var trackerWidth = 20;
    var trackerHeight = 50;
    const margin = { top: 5, right: 5, bottom: 10, left: 15 };
    const width = 460 - margin.left - margin.right;
    const height = 300 - margin.top - margin.bottom;
    const minimapHeight = 50;
    const minimapWidth = width;
    const xDomain = [
      new Date(formatDate(points[points.length - 11].dateTime)),
      d3.max(points, (d) => new Date(formatDate(d.dateTime))),
    ];
    const yDomain = [
      d3.min(points, (d) => d.value),
      d3.max(points, (d) => d.value),
    ];
    const xDomainMap = d3.extent(
      points,
      (d) => new Date(formatDate(d.dateTime))
    );
    const yDomainMap = [
      d3.min(points, (d) => d.value),
      d3.max(points, (d) => d.value),
    ];
    const { x, y } = createScales(width, height, xDomain, yDomain);
    const { x: minimapXScale, y: minimapYScale } = createScales(
      minimapWidth,
      minimapHeight,
      xDomainMap,
      yDomainMap
    );

    const xAxis = createAx(x, d3.axisBottom, 5, d3.timeFormat("%H-%M-%S"));
    const yAxis = createAx(y, d3.axisLeft, 5);

    const svgWidth = width + margin.left + margin.right + 20;
    const svgHeigth = height + margin.top + margin.bottom + 10;

    const svgMinimapHeigth = minimapHeight + margin.top + margin.bottom + 10;

    const svg = createSVG("#chart", svgWidth, svgHeigth, margin);
    const svgMinimap = createSVG(
      "#minimap",
      svgWidth,
      svgMinimapHeigth,
      margin
    );

    const valueLine = createValueLine(x, y);
    const minimapLine = createValueLine(minimapXScale, minimapYScale);
    const path = createPath(svg, points, valueLine, margin);
    createPath(svgMinimap, points, minimapLine, margin);

    const tracker = createTracker(
      svgMinimap,
      trackerWidth,
      trackerHeight,
      svgWidth
    );

    const circle = createCircle(svg, points, margin, x, y);

    const drag = createDragger(
      tracker,
      path,
      circle,
      width,
      margin,
      minimapXScale,
      trackerWidth,
      x,
      y
    );

    tracker.call(drag);

    svg
      .append("g")
      .attr("transform", `translate(${margin.left}, ${height})`)
      .call(xAxis);

    svg
      .append("g")
      .attr("transform", `translate(${margin.left}, 0)`)
      .call(yAxis);

    d3.select(window).on("keydown", (event) => {
      if (event.key === "ArrowLeft") {
        // Shift chart data to the left
        x.domain(x.domain().map((d) => new Date(d.getTime() - 1000)));
      } else if (event.key === "ArrowRight") {
        x.domain(x.domain().map((d) => new Date(d.getTime() + 1000)));
      }

      updateChart(svg, xAxis, valueLine, x, y);
    });
  });
</script>

<div>
  {#if chart.data}
    <div id="chart" />
    <div id="minimap" />
  {:else}
    No data available
  {/if}
</div>

<style>
  @import "../styles/Chart.scss";
</style>
