import { ScenarioActionType } from "../../types";

export interface ScenarioFormValues {
  selectedSensor: number;
  selectedDevice?: number;
  threshold: number;
  hysteresis: number;
  operator: number;
  actionType: number;
  command: string;
}

export const buildScenarioPayload = (values: ScenarioFormValues) => ({
  sensors: [{ sensorId: values.selectedSensor }],
  devices:
    values.actionType === ScenarioActionType.Device && values.selectedDevice
      ? [{ deviceId: values.selectedDevice }]
      : [],
  threshold: Number(values.threshold),
  hysteresis: Number(values.hysteresis),
  operator: Number(values.operator),
  actionType: Number(values.actionType),
  command: values.command.trim(),
});
