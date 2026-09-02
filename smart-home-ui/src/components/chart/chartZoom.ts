import * as d3 from "d3";

export const crateZoom = (width, height, xScale, onZoom = () => {}) => {
  const baseXScale = xScale.copy();

  return d3
    .zoom()
    .filter((event) => event.type === "wheel")
    .scaleExtent([1, 100])
    .extent([
      [0, 0],
      [width, height],
    ])
    .translateExtent([
      [0, 0],
      [width, height],
    ])
    .on("zoom", (event) => {
      xScale.domain(event.transform.rescaleX(baseXScale).domain());
      onZoom(xScale);
    });
};
