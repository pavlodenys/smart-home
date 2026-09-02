import assert from "node:assert/strict";
import test from "node:test";

import { ScenarioActionType } from "../src/types.ts";
import { buildScenarioPayload } from "../src/components/scenario/scenarioPayload.ts";

test("notification payload uses threshold and omits devices", () => {
  const payload = buildScenarioPayload({
    selectedSensor: 4,
    selectedDevice: 2,
    threshold: 30,
    hysteresis: 2,
    operator: 1,
    actionType: ScenarioActionType.Notification,
    command: " Moisture is {value}% ",
  });

  assert.deepEqual(payload, {
    sensors: [{ sensorId: 4 }],
    devices: [],
    threshold: 30,
    hysteresis: 2,
    operator: 1,
    actionType: ScenarioActionType.Notification,
    command: "Moisture is {value}%",
  });
  assert.equal("value" in payload, false);
  assert.equal("sensorValue" in payload, false);
});

test("device payload contains exactly one selected device", () => {
  const payload = buildScenarioPayload({
    selectedSensor: 4,
    selectedDevice: 2,
    threshold: 30,
    hysteresis: 0,
    operator: 1,
    actionType: ScenarioActionType.Device,
    command: "",
  });

  assert.deepEqual(payload.devices, [{ deviceId: 2 }]);
});
