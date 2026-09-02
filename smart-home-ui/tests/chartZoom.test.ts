import assert from "node:assert/strict";
import test from "node:test";

import * as d3 from "d3";

import { crateZoom } from "../src/components/chart/chartZoom.ts";

test("chart zoom derives each X domain from a stable scale and preserves Y", () => {
  const x = d3.scaleLinear().domain([0, 100]).range([0, 100]);
  const y = d3.scaleLinear().domain([0, 100]).range([100, 0]);
  const zoom = crateZoom(100, 100, x);
  const handleZoom = zoom.on("zoom");
  const event = { transform: d3.zoomIdentity.scale(2) };

  handleZoom?.(event);
  assert.deepEqual(x.domain(), [0, 50]);
  assert.deepEqual(y.domain(), [0, 100]);

  handleZoom?.(event);
  assert.deepEqual(x.domain(), [0, 50]);
  assert.deepEqual(y.domain(), [0, 100]);
});
