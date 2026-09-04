<script lang="ts">
  import { onMount, createEventDispatcher, tick } from "svelte";
  import type { ChartData } from "../../types";
  import * as d3 from "d3";
  import moment from "moment";
  import * as signalR from "@microsoft/signalr";
  import { httpFetch } from "../../api/httpServise";

  import {
    formatDate,
    smallFormatDate,
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
    getSensorDetailsUrl,
    crateZoom,
    syncTrackerToDomain,
    decimatePoints,
  } from "./d3Utils";

  //TODO: add real-time update

  export let chart: ChartData;
  export let chartId;
  export let sensorId: number | undefined;

  const connection = new signalR.HubConnectionBuilder()
    //.withUrl("https://localhost:7138/hub", {
    .withUrl("http://localhost:5200/hub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    }) // Specify the URL of your SignalR hub
    .withAutomaticReconnect()
    .build();

  let selectedDate = moment().format("YYYY-MM-DD");
  const dispatch = createEventDispatcher();
  let chartHost: HTMLDivElement;

  const scaleParamMin = 0.97;
  const scaleParam = 1.03;

  // shared with the reactive block below so server-paged points can be merged in after mount
  let allPoints: any[] = [];
  let x, y, svg, svgMinimap, xAxisSvg, yAxisSvg, margin, minimapLine, minimapXScale, minimapYScale;
  let tracker, trackerWidth, svgWidth, minimapElementWidth;

  // merge newly fetched pages (loaded via panning) into the chart once mounted
  $: if (svg && chart?.data && chart.data !== allPoints) {
    const existingIds = new Set(allPoints.map((p: any) => p.id));
    const newPoints = chart.data.filter((p: any) => !existingIds.has(p.id));

    if (newPoints.length) {
      allPoints = [...allPoints, ...newPoints];
      filterPoints(allPoints, selectedDate, chartId, chart.id, sensorId).then((filtered) => {
        updateDataChart(
          chartId,
          filtered,
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
        if (tracker) {
          syncTrackerToDomain(tracker, minimapXScale, x, trackerWidth, minimapElementWidth);
        }
      });
    }
  }

  onMount(async () => {
    await tick();
    allPoints = chart.data ?? [];
    if (allPoints.length) {
      const latestPoint = allPoints.reduce((latest: any, point: any) =>
        new Date(point.dateTime).getTime() > new Date(latest.dateTime).getTime()
          ? point
          : latest
      );
      selectedDate = moment(latestPoint.dateTime).format("YYYY-MM-DD");
    }

    connection.start().catch((err) => console.error(err));

    // a dropped connection silently stops all future updates without this; on reconnect,
    // re-fetch the day so any points missed while disconnected get backfilled
    connection.onreconnected(async () => {
      if (!svg || sensorId == null) {
        return;
      }

      const sensor = await httpFetch.get(getSensorDetailsUrl(sensorId, selectedDate));
      const matchingChart = sensor?.chartData?.find((c) => c.id === chart.id);
      const existingIds = new Set(allPoints.map((p: any) => p.id));
      const newPoints = (matchingChart?.data ?? []).filter((p: any) => !existingIds.has(p.id));

      if (newPoints.length) {
        allPoints = [...allPoints, ...newPoints];
        const filteredPoints = await filterPoints(
          allPoints,
          selectedDate,
          chartId,
          chart.id,
          sensorId
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
        if (tracker) {
          syncTrackerToDomain(tracker, minimapXScale, x, trackerWidth, minimapElementWidth);
        }
      }
    });

    connection.on("ReceiveMessage", async (receivedMessage) => {
      //console.log(receivedMessage);
      if (chart.id === receivedMessage.dataId) {
        allPoints.push(receivedMessage);

        const filteredPoints = await filterPoints(
          allPoints,
          selectedDate,
          chartId,
          chart.id,
          sensorId
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
        if (tracker) {
          syncTrackerToDomain(tracker, minimapXScale, x, trackerWidth, minimapElementWidth);
        }
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
      chart.id,
      sensorId
    );

    // if (!points || !points.length) {
    //   return;
    // }

    trackerWidth = 20;
    var trackerHeight = 50;
    margin = { top: 12, right: 18, bottom: 42, left: 42 };
    const availableWidth = chartHost?.clientWidth || 720;
    const width = Math.max(280, availableWidth - margin.left - margin.right - 20);
    const height = Math.max(230, Math.min(310, width * 0.48));
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

    ({ x, y } = createScales(width, height, xDomain, yDomain));
    ({ x: minimapXScale, y: minimapYScale } = createScales(
      minimapWidth,
      minimapHeight,
      xDomainMap,
      yDomainMap
    ));

    const xAxis = createAx(x, d3.axisBottom, 5, d3.timeFormat("%H-%M-%S"));
    const yAxis = createAx(y, d3.axisLeft, 5);

    svgWidth = width + margin.left + margin.right + 20;
    const svgHeigth = height + margin.top + margin.bottom + 10;    const svgMinimapHeigth = minimapHeight + margin.top + margin.bottom + 10;

    const brush = d3
      .brush()
      .extent([
        [0, 0],
        [width, height],
      ])
      .on("end", brushed);

    svg = createSVG(`#chart-${chartId}`, svgWidth, svgHeigth, margin);
    svgMinimap = createSVG(
      `#minimap-${chartId}`,
      svgWidth,
      svgMinimapHeigth,
      margin
    );
    xAxisSvg = svg
      .append("g")
      .attr("transform", `translate(0, ${height})`)
      .call(xAxis);

    xAxisSvg
      .selectAll(".tick text")
      .attr("transform", "translate(-10, 0) rotate(-40)") // Rotate the tick labels by -40 degrees
      .style("text-anchor", "end");
    yAxisSvg = svg
      .append("g")
      .call(yAxis);

    const gBrush = svg.append("g").attr("class", "brush").call(brush);

    datePicker.on("change", async (e) => {
      const newDate = (datePicker.node() as HTMLInputElement).value;
      selectedDate = newDate;
      const filteredPoints = await filterPoints(
        allPoints,
        newDate,
        chartId,
        chart.id,
        sensorId
      );

      if (!filteredPoints || !filteredPoints.length) {
        // scope cleanup to this chart's own svg/minimap so other charts on the page aren't affected
        svg.select(".line").remove();
        svg.selectAll(`.dot-${chartId}`).remove();
        svgMinimap.select(".line").remove();
        xAxisSvg.selectAll("*").remove();
        yAxisSvg.selectAll("*").remove();

        svg.select(".no-data-label").remove();
        svg
          .append("text")
          .attr("class", "no-data-label")
          .attr("x", width / 2)
          .attr("y", height / 2)
          .attr("text-anchor", "middle")
          .text("No data for this date");
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
      syncTrackerToDomain(tracker, minimapXScale, x, trackerWidth, minimapElementWidth);
    });

    svg
      .append("defs")
      .append("clipPath")
      .attr("id", "chart-area-clip")
      .append("rect")
      .attr("x", 0)
      .attr("y", 0)
      .attr("width", width)
      .attr("height", height);
    const chartArea = svg
      .append("g")
      .attr("class", "chart-area")
      .attr("clip-path", "url(#chart-area-clip)");

    const valueLine = createValueLine(x, y);
    minimapLine = createValueLine(minimapXScale, minimapYScale);
    const path = createPath(chartArea, points, valueLine, margin);
    // the minimap is only a few hundred pixels wide, and one DOM circle per point gets
    // expensive well before a path does, so both use a decimated subset of large series
    const miniMapPath = createPath(svgMinimap, decimatePoints(points, 400), minimapLine, margin);

    const circle1 = createCircle(chartId, chartArea, decimatePoints(points, 300), margin, x, y);

    // the tracker lives in the minimap's own coordinate space (already offset by margin.left via
    // its parent g), so its usable width matches what the dragger clamps against, not the full svg width
    minimapElementWidth = width + margin.right + 20;

    tracker = createTracker(
      svgMinimap,
      trackerWidth,
      trackerHeight,
      minimapElementWidth
    );
    syncTrackerToDomain(tracker, minimapXScale, x, trackerWidth, minimapElementWidth);

    const drag = createDragger(
      tracker,
      path,
      chartArea,
      chartId,
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

    const zoom = crateZoom(width, height, x, () => {
      xAxisSvg.call(createAx(x, d3.axisBottom, 5, d3.timeFormat("%H-%M-%S")));
      xAxisSvg
        .selectAll(".tick text")
        .attr("transform", "translate(-10, 0) rotate(-40)")
        .style("text-anchor", "end");

      path.attr("d", valueLine);
      chartArea
        .selectAll(`.dot-${chartId}`)
        .attr("cx", (point) => x(new Date(formatDate(point.dateTime))))
        .attr("cy", (point) => y(point.value));
    });

    d3.select(svg.node().ownerSVGElement).call(zoom);

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

<div class="chart-component">
  <div class="chart-toolbar">
    <div class="chart-heading">
      <h3>{chart.name || "Sensor readings"}</h3>
      {#if chart.description}
        <p>{chart.description}</p>
      {/if}
    </div>
    <label class="date-filter" for="date-{chartId}">
      <span>Date</span>
      <input id="date-{chartId}" type="date" bind:value={selectedDate} />
    </label>
  </div>
  {#if chart.data}
    <div class="chart-visual" bind:this={chartHost}>
      <div class="chart-plot" id="chart-{chartId}" />
      <div class="chart-minimap" id="minimap-{chartId}" />
      <p class="chart-help">Scroll over the chart to zoom. Drag the lower handle to explore earlier readings.</p>
    </div>
  {:else}
    <div class="chart-empty">No readings are available for this data source.</div>
  {/if}
</div>

<style>
  @import "./Chart.scss";
</style>
