import assert from "node:assert/strict";
import test from "node:test";

import { getFirstPoint } from "../src/components/chart/chartDates.ts";

test("getFirstPoint always returns a valid Date for a short realtime series", () => {
  const result = getFirstPoint([{ dateTime: "2026-08-15T15:30:02" }]);

  assert.ok(result instanceof Date);
  assert.ok(Number.isFinite(result.getTime()));
});

test("getFirstPoint limits long series to the latest twenty minutes", () => {
  const points = Array.from({ length: 31 }, (_, index) => ({
    dateTime: new Date(Date.UTC(2026, 7, 15, 15, index, 0)),
  }));

  assert.equal(
    getFirstPoint(points).getTime(),
    new Date(Date.UTC(2026, 7, 15, 15, 10, 0)).getTime(),
  );
});
