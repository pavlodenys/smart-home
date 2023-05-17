<script lang="ts">
  import { onMount, createEventDispatcher } from "svelte";
  import type { ChartData } from "../../types";
  import * as d3 from "d3";
  import moment from "moment";
  import * as signalR from "@microsoft/signalr";

  import {
    formatDate,
    createScales,
    createAx,
    createSVG,
    createValueLine,
    createPath,
    createTracker,
    createCircle,
    createDragger,
    filterPoints,
    updateDataChart,
    getFirstPoint,
    crateZoom,
  } from "./d3Utils";

  //TODO: add real-time update

  export let chart: ChartData;
  export let chartId;

  const connection = new signalR.HubConnectionBuilder()
    //.withUrl("https://localhost:7138/hub", {
    .withUrl("http://localhost:5200/hub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    }) // Specify the URL of your SignalR hub
    .build();

  let selectedDate = moment().format("YYYY-MM-DD");
  const dispatch = createEventDispatcher();

  const scaleParamMin = 0.97;
  const scaleParam = 1.03;

  onMount(async () => {
    const allPoints = chart.data;

    connection.start().catch((err) => console.error(err));

    connection.on("ReceiveMessage", async (receivedMessage) => {
      //console.log(receivedMessage);
      if (chart.id === receivedMessage.dataId) {
        allPoints.push(receivedMessage);

        const filteredPoints = await filterPoints(
          allPoints,
          selectedDate,
          chartId,
          chart.id
        );

        updateDataChart(
          chartId,
          filteredPoints,
          x,
          y,
          xAxisSvg,
          yAxisSvg,
          svg,
          margin,
          svgMinimap,
          minimapLine,
          minimapXScale,
          minimapYScale
        );
      }
    });

    if (!allPoints || !allPoints.length) {
      return;
    }
    const datePicker = d3.select(`#date-${chartId}`);

    // let x, y, xAxis, yAxis, svg;

    const points = await filterPoints(
      allPoints,
      selectedDate,
      chartId,
      chart.id
    );

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

    const yMin: any = d3.min(points, (d: any) => d.value);
    const yMax: any = d3.max(points, (d: any) => d.value);
    const firstPoint = getFirstPoint(points);

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

    const brush = d3
      .brush()
      .extent([
        [0, 0],
        [width, height],
      ])
      .on("end", brushed);

    const svg = createSVG(`#chart-${chartId}`, svgWidth, svgHeigth, margin);
    const svgMinimap = createSVG(
      `#minimap-${chartId}`,
      svgWidth,
      svgMinimapHeigth,
      margin
    );
    const xAxisSvg = svg
      .append("g")
      .attr("transform", `translate(${margin.left}, ${height})`)
      .call(xAxis);
    const yAxisSvg = svg
      .append("g")
      .attr("transform", `translate(${margin.left}, 0)`)
      .call(yAxis);

    const gBrush = svg.append("g").attr("class", "brush").call(brush);

    datePicker.on("change", async (e) => {
      const newDate = datePicker.node().value;
      selectedDate = newDate;
      const filteredPoints = await filterPoints(allPoints, newDate, chartId, chart.id);

      if (!filteredPoints || !filteredPoints.length) {
        d3.select(".line").remove();
        d3.selectAll(`.dot-${chartId}`).remove();
        return;
      }
      // console.log(xAxis);
      updateDataChart(
        chartId,
        filteredPoints,
        x,
        y,
        xAxisSvg,
        yAxisSvg,
        svg,
        margin,
        svgMinimap,
        minimapLine,
        minimapXScale,
        minimapYScale
      );
    });

    const valueLine = createValueLine(x, y);
    const minimapLine = createValueLine(minimapXScale, minimapYScale);
    const path = createPath(svg, points, valueLine, margin);
    const miniMapPath = createPath(svgMinimap, points, minimapLine, margin);

    const tracker = createTracker(
      svgMinimap,
      trackerWidth,
      trackerHeight,
      svgWidth
    );

    const circle1 = createCircle(chartId, svg, points, margin, x, y);

    const drag = createDragger(
      tracker,
      path,
      circle1,
      width,
      margin,
      minimapXScale,
      trackerWidth,
      x,
      y,
      xAxisSvg,
      chart.id,
      dispatch
    );

    tracker.call(drag);

    const zoom = crateZoom(width, height, x, y);

    svg.call(zoom);

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

    function brushed(event) {
      if (!event.sourceEvent) return; // Only transition after input.
      if (!event.selection) return; // Ignore empty selections.

      const [x0, x1] = event.selection;
      const newXDomain = [x.invert(x0), x.invert(x1)];

      let extent = event.selection; // looks like [ [12,11], [132,178]]
      let circles = svg.selectAll(`.dot-${chartId}`);
      // Is the circle in the selection?
      let isBrushed =
        extent[0][0] <= circles.attr("cx") &&
        extent[1][0] >= circles.attr("cx") && // Check X coordinate
        extent[0][1] <= circles.attr("cy") &&
        extent[1][1] >= circles.attr("cy"); // And Y coordinate
      if (isBrushed) {
        circles.transition().duration(200).style("fill", "green");
      } else {
        circles.transition().duration(200).style("fill", "pink");
      }

      // Call an API to load data for the new domain.
      // console.log(newXDomain);
    }
  });


</script>

<div>
  <div>{chart.name}</div>
  <div>
    <input id="date-{chartId}" type="date" bind:value={selectedDate} />
  </div>
  {#if chart.data}
    <div id="chart-{chartId}" />
    <div id="minimap-{chartId}" />
  {:else}
    No data available
  {/if}
</div>

<style>
  @import "./Chart.scss";
</style>
