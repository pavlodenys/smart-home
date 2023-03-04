<script lang="ts">
  import { onMount } from "svelte";
  import type { ChartData, PointDto } from "../types";
  import * as d3 from "d3";
  import moment from "moment";

  //TODO: add real-time update

  export let chart: ChartData;
  export let chartId;

  let selectedDate = moment().format("YYYY-MM-DD");

  const scaleParamMin = 0.97;
  const scaleParam = 1.03;
  const formatDate = (d) => moment(d).format("YYYY-MM-DD HH:mm:ss");

  const createScales = (width, height, domainX, domainY) => {
    var x = d3.scaleTime().domain(domainX).range([0, width]);
    var y = d3.scaleLinear().domain(domainY).range([height, 0]);
    return { x, y };
  };

  const updateScales = (xDomain, yDomain, x, y, xAxis, yAxis) => {
    x.domain(xDomain);
    y.domain(yDomain);

    // xAxis.call(d3.axisBottom(x));
    //yAxis.call(d3.axisLeft(y));
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
      .attr("transform", `translate(${translateX - 45}, 0)`)
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
  let timeoutId;

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
        cancelAnimationFrame(timeoutId);
        timeoutId = requestAnimationFrame(() => {
          let xPos = d.x;
          let minimapElementWidth = width + margin.right + 20;
          let xValue = minimapXScale.invert(xPos);
          //let clampedXPos = d3.clamp(0, minimapWidth - trackerWidth, xPos);
          let clampedXPos = Math.max(
            0,
            Math.min(xPos, minimapElementWidth - trackerWidth)
          );

          console.log(`${xPos} ${minimapElementWidth} ${trackerWidth}`);
          //console.log(clampedXPos);

          //tracker.attr("x", clampedXPos);
          tracker.attr("transform", `translate(${clampedXPos}, 0)`);
          if (d.dx === 1) {
            x.domain(x.domain().map((d) => new Date(d.getTime() + 1000)));
            y.domain();
          } else if (d.dx === -1) {
            x.domain(x.domain().map((d) => new Date(d.getTime() - 1000)));
            //y.domain(d3.extent(y.ticks));
          }

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

  const filterPoints = (points, date) => {
    return points.filter((point) => {
      const pointDate = new Date(point.dateTime);
      return moment(pointDate).format("YYYY-MM-DD") === date;
    });
  };

  const updateDataChart = (pointsData, x, y, xAxis, yAxis, svg) => {
    if (!pointsData || !pointsData.length) {
      return;
    }
    const yMin = d3.min(pointsData, (d: any) => d.value);
    const yMax = d3.max(pointsData, (d: any) => d.value);

    const firstPoint =
      pointsData && pointsData.length ? pointsData[2].dateTime : new Date();

    const xDomain = [
      new Date(formatDate(firstPoint)),
      d3.max(pointsData, (d: any) => new Date(formatDate(d.dateTime))),
    ];
    const yDomain = [scaleParamMin * yMin, scaleParam * yMax];

    const xDomainMap = d3.extent(
      pointsData,
      (d: any) => new Date(formatDate(d.dateTime))
    );
    const yDomainMap = [
      d3.min(pointsData, (d: any) => d.value),
      d3.max(pointsData, (d: any) => d.value),
    ];

    updateScales(xDomain, yDomain, x, y, xAxis, yAxis);

    const line = svg.select(".line");
    line.data([pointsData]).attr(
      "d",
      d3
        .line()
        .x((d: any) => x(new Date(d.dateTime)))
        .y((d: any) => y(d.value))
    );

    const circles = svg.selectAll(".circle").data(pointsData, (d) => d.id);
    circles
      .enter()
      .append("circle")
      .attr("class", "circle")
      .attr("cx", (d) => x(new Date(d.dateTime)))
      .attr("cy", (d) => y(d.value))
      .attr("r", 4);
    circles.exit().remove();
  };

  onMount(() => {
    const allPoints = chart.data;

    if (!allPoints || !allPoints.length) {
      return;
    }
    const datePicker = d3.select(`#date-${chartId}`);

    // let x, y, xAxis, yAxis, svg;

    const points = filterPoints(allPoints, selectedDate);

    // if (!points || !points.length) {
    //   return;
    // }

    var trackerWidth = 20;
    var trackerHeight = 50;
    const margin = { top: 5, right: 5, bottom: 10, left: 15 };
    const width = 460 - margin.left - margin.right;
    const height = 300 - margin.top - margin.bottom;
    const minimapHeight = 50;
    const minimapWidth = width;

    const yMin = d3.min(points, (d: any) => d.value);
    const yMax = d3.max(points, (d: any) => d.value);
    const firstPoint =
      points && points.length ? points[2].dateTime : new Date();

    const xDomain = [
      new Date(formatDate(firstPoint)),
      d3.max(points, (d: any) => new Date(formatDate(d.dateTime))),
    ];
    const yDomain = [scaleParamMin * yMin, scaleParam * yMax];

    const xDomainMap = d3.extent(
      points,
      (d: any) => new Date(formatDate(d.dateTime))
    );
    const yDomainMap = [
      d3.min(points, (d: any) => d.value),
      d3.max(points, (d: any) => d.value),
    ];

    const { x, y } = createScales(width, height, xDomain, yDomain);
    //x = x1;
    //y = y1;
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

    const svg = createSVG(`#chart-${chartId}`, svgWidth, svgHeigth, margin);
    const svgMinimap = createSVG(
      `#minimap-${chartId}`,
      svgWidth,
      svgMinimapHeigth,
      margin
    );

    datePicker.on("change", (e) => {
      const newDate = datePicker.node().value;
      selectedDate = newDate;
      const filteredPoints = filterPoints(allPoints, newDate);

      if (!filteredPoints || !filteredPoints.length) {
        d3.select(".line").remove();
        d3.selectAll(".circle").remove();
        return;
      }

      updateDataChart(filteredPoints, x, y, xAxis, yAxis, svg);
    });

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

    const zoom = crateZoom(width, height, x, y);

    svg.call(zoom);

    svg
      .append("g")
      .attr("transform", `translate(${margin.left}, ${height})`)
      .call(xAxis);

    svg
      .append("g")
      .attr("transform", `translate(${margin.left}, 0)`)
      .call(yAxis);

    svg.on("mousedown", (event) => {
      console.log(event);
    });
    svg.on("wheel", (event) => {
      console.log(event);
      const delta = event.deltaY;
      const scale = delta > 0 ? 1.1 : 0.9;
      const mouseX = event.clientX - svg.node().getBoundingClientRect().x;
      const zoomTransform = d3.zoomIdentity
        .translate(mouseX, 0)
        .scale(scale)
        .translate(-mouseX, 0);
      svg.call(zoom.transform, zoomTransform);
    });

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

  const crateZoom = (width, height, xScale, yScale) => {
    return d3
      .zoom()
      .scaleExtent([1, 100])
      .translateExtent([
        [0, 0],
        [width, height],
      ])
      .on("zoom", (event) => {
        const transform1 = event.transform;

        console.log(transform1);
        xScale.domain(transform1.rescaleX(xScale).domain());
        yScale.domain(transform1.rescaleY(yScale).domain());
      });
  };
</script>

<div>
  <div>{chart.name}</div>
  {#if chart.data}
    <div>
      <input id="date-{chartId}" type="date" bind:value={selectedDate} />
    </div>
    <div id="chart-{chartId}" />
    <div id="minimap-{chartId}" />
  {:else}
    No data available
  {/if}
</div>

<style>
  @import "../styles/Chart.scss";
</style>
